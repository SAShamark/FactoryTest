using System;
using Gameplay.Entities;
using Gameplay.Entities.Character;
using Gameplay.Entities.Enemies;
using Services.Sequence;
using UnityEngine;

namespace Gameplay
{
    [Serializable]
    public class GameplayManager
    {
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private EnvironmentControl _environmentControl;
        [SerializeField] private CharacterControl _character;
        [SerializeField] private LevelConfig _levelConfig;

        [Header("Death Sequence")]
        [SerializeField, Range(0.01f, 1f)] private float _deathTimeScale = 0.2f;
        [SerializeField, Min(0f)] private float _deathSlowMotionDuration = 0.15f;
        [SerializeField, Min(0f)] private float _deathPresentationDuration = 2f;

        [Header("Victory Sequence")]
        [SerializeField] private CameraController _cameraController;

        private bool _isTimeRamping;
        private float _timeRampElapsed;
        private bool _isWaitingForShooting;
        private bool _isShootingAllowed;
        private float _shootingDelayElapsed;
        private bool _isPlayingDeathSequence;
        private float _deathSequenceElapsed;
        private float _deathSequenceStartTimeScale;
        private bool _isFinishAlignmentStarted;
        private bool _isFinishGateOpened;
        private bool _isCameraFrozen;
        private bool _isPlayingVictorySequence;
        private float _victorySequenceElapsed;
        private bool _isFinished;
        private IGameplaySequence _gameplaySequence;

        public float TravelledDistance => _character.TravelledDistance;
        public float TargetDistance => _levelConfig.TargetDistance;

        public event Action LevelCompleted;
        public event Action LevelFailed;

        public event Action EnemyKilled
        {
            add => _enemySpawner.EnemyKilled += value;
            remove => _enemySpawner.EnemyKilled -= value;
        }

        public void Initialize(IGameplaySequence gameplaySequence)
        {
            _gameplaySequence = gameplaySequence;
            _enemySpawner.Initialize();
            _enemySpawner.StopSpawn();
            _environmentControl.Initialize(_levelConfig.TargetDistance);
            _enemySpawner.SetSpawnLimit(_environmentControl.FinishZ, _levelConfig.NoSpawnZoneDistance);
            _character.Died += HandleCharacterDied;
            _gameplaySequence.StopGame();
        }

        public void Dispose()
        {
            _character.Died -= HandleCharacterDied;
        }

        internal void LateUpdate()
        {
            if (_isPlayingDeathSequence)
            {
                UpdateDeathSequence();
                return;
            }

            if (_isPlayingVictorySequence)
            {
                UpdateVictorySequence();
                return;
            }

            UpdateTimeRamp();
            UpdateShootingDelay();
            _enemySpawner.LateUpdate();
            UpdateTurretState();
            CheckTargetReached();
        }

        public void StartGameplay()
        {
            _character.StartMoving();
            _enemySpawner.StartSpawn(true);
            _isShootingAllowed = false;
            _character.StopShooting();
            StartShootingDelay();
            StartTimeRamp();
        }

        public void PauseGameplay()
        {
            if (_isFinished)
                return;

            _isTimeRamping = false;
            _gameplaySequence.StopGame();
            _enemySpawner.StopSpawn();
        }

        public void ContinueGameplay()
        {
            if (_isFinished)
                return;

            _enemySpawner.StartSpawn();
            StartTimeRamp();
        }

        private void CheckTargetReached()
        {
            if (_isFinished)
                return;

            if (!_isFinishAlignmentStarted
                && TravelledDistance >= TargetDistance - _levelConfig.AlignmentStartDistance)
            {
                _isFinishAlignmentStarted = true;
                _character.BeginFinishAlignment(
                    _environmentControl.FinishCenterX,
                    _levelConfig.AlignmentDistance);
            }

            if (!_isFinishGateOpened
                && TravelledDistance >= TargetDistance - _levelConfig.GateOpenDistance)
            {
                _isFinishGateOpened = true;
                _environmentControl.OpenFinishGateWithVictoryEffects();
            }

            if (!_isCameraFrozen
                && TravelledDistance >= TargetDistance - _levelConfig.CameraFreezeDistance)
            {
                _isCameraFrozen = true;
                _cameraController.FreezeGameplayCamera();
            }

            if (TravelledDistance >= TargetDistance)
                StartVictorySequence();
        }

        private void HandleCharacterDied()
        {
            if (_isFinished)
                return;

            _isFinished = true;
            _isTimeRamping = false;
            _isWaitingForShooting = false;
            _isShootingAllowed = false;
            _character.StopShooting();
            _enemySpawner.StopSpawn();
            _enemySpawner.BeginSurroundingTarget();

            _deathSequenceElapsed = 0f;
            _deathSequenceStartTimeScale = Mathf.Max(_gameplaySequence.TimeScale, _deathTimeScale);
            _isPlayingDeathSequence = true;

            if (_deathPresentationDuration <= 0f)
                CompleteDeathSequence();
        }

        private void StartVictorySequence()
        {
            _isFinished = true;
            _isTimeRamping = false;
            _isWaitingForShooting = false;
            _isShootingAllowed = false;
            _character.StopShooting();
            _enemySpawner.StopAndDespawnAllEnemies();
            _environmentControl.StopGroundRecycling();
            _environmentControl.CloseFinishGate();

            _victorySequenceElapsed = 0f;
            _isPlayingVictorySequence = true;
            LevelCompleted?.Invoke();

            if (_levelConfig.VictoryPresentationDuration <= 0f)
                CompleteVictorySequence();
        }

        private void UpdateVictorySequence()
        {
            _victorySequenceElapsed += Time.unscaledDeltaTime;

            if (_victorySequenceElapsed < _levelConfig.VictoryPresentationDuration)
                return;

            bool exitedViewport = _cameraController == null
                || _cameraController.HasFollowingTargetExitedViewport(
                    _levelConfig.VictoryExitViewportMargin);
            bool timedOut = _levelConfig.VictoryMaximumDriveDuration <= 0f
                || _victorySequenceElapsed >= _levelConfig.VictoryMaximumDriveDuration;

            if (exitedViewport || timedOut)
                CompleteVictorySequence();
        }

        private void CompleteVictorySequence()
        {
            _isPlayingVictorySequence = false;
            _character.StopGameplay();
            _gameplaySequence.StartGame();
        }

        private void UpdateDeathSequence()
        {
            _deathSequenceElapsed += Time.unscaledDeltaTime;

            if (_deathSlowMotionDuration <= 0f)
            {
                _gameplaySequence.SetTimeScale(_deathTimeScale);
            }
            else
            {
                float slowMotionProgress = Mathf.Clamp01(
                    _deathSequenceElapsed / _deathSlowMotionDuration);
                float easedProgress = Mathf.SmoothStep(0f, 1f, slowMotionProgress);
                _gameplaySequence.SetTimeScale(Mathf.Lerp(
                    _deathSequenceStartTimeScale,
                    _deathTimeScale,
                    easedProgress));
            }

            if (_deathSequenceElapsed >= _deathPresentationDuration)
                CompleteDeathSequence();
        }

        private void CompleteDeathSequence()
        {
            _isPlayingDeathSequence = false;
            _gameplaySequence.StopGame();
            LevelFailed?.Invoke();
        }

        private void StartTimeRamp()
        {
            if (_levelConfig.StartRampDuration <= 0f)
            {
                _gameplaySequence.StartGame();
                _isTimeRamping = false;
                return;
            }

            _timeRampElapsed = 0f;
            _isTimeRamping = true;
            _gameplaySequence.StopGame();
        }

        private void StartShootingDelay()
        {
            _shootingDelayElapsed = 0f;
            _isWaitingForShooting = true;

            if (_levelConfig.ShootingDelay <= 0f)
                StartShooting();
        }

        private void UpdateShootingDelay()
        {
            if (!_isWaitingForShooting || _gameplaySequence.TimeScale <= 0f)
                return;

            _shootingDelayElapsed += Time.unscaledDeltaTime;

            if (_shootingDelayElapsed >= _levelConfig.ShootingDelay)
                StartShooting();
        }

        private void StartShooting()
        {
            _isWaitingForShooting = false;
            _isShootingAllowed = true;
            UpdateTurretState();
        }

        private void UpdateTurretState()
        {
            if (!_isShootingAllowed)
                return;

            if (_enemySpawner.HasAliveEnemies)
                _character.StartShooting();
            else
                _character.StopShooting();
        }

        private void UpdateTimeRamp()
        {
            if (!_isTimeRamping)
                return;

            _timeRampElapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                _timeRampElapsed / _levelConfig.StartRampDuration);
            _gameplaySequence.SetTimeScale(_levelConfig.StartRampCurve.Evaluate(progress));

            if (progress >= 1f)
            {
                _gameplaySequence.StartGame();
                _isTimeRamping = false;
            }
        }
    }
}
