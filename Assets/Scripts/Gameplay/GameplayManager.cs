using System;
using Gameplay.Entities;
using Gameplay.Entities.Character;
using Gameplay.Entities.Enemies;
using UnityEngine;

namespace Gameplay
{
    [Serializable]
    public class GameplayManager
    {
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private CharacterControl _character;
        [SerializeField] private LevelConfig _levelConfig;
        [SerializeField] private EnvironmentControl _environmentControl;
        [SerializeField, Min(0f)] private float _startRampDuration = 1.25f;
        [SerializeField] private AnimationCurve _startRampCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField, Min(0f)] private float _shootingDelay = 3f;

        [Header("Death Sequence")]
        [SerializeField, Range(0.01f, 1f)] private float _deathTimeScale = 0.2f;
        [SerializeField, Min(0f)] private float _deathSlowMotionDuration = 0.15f;
        [SerializeField, Min(0f)] private float _deathPresentationDuration = 2f;

        [Header("Victory Sequence")]
        [SerializeField] private CameraController _cameraController;
        [SerializeField, Min(0f)] private float _victoryNoSpawnZoneDistance = 10f;
        [SerializeField, Min(0f)] private float _finishAlignmentStartDistance = 30f;
        [SerializeField, Min(0.01f)] private float _finishAlignmentDistance = 12f;
        [SerializeField, Min(0f)] private float _gateOpenDistance = 22f;
        [SerializeField, Min(0f)] private float _cameraFreezeDistance = 18f;
        [SerializeField, Min(0f)] private float _victoryPresentationDuration = 2f;

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

        public float TravelledDistance => _character.TravelledDistance;
        public float TargetDistance => _levelConfig.TargetDistance;

        public event Action LevelCompleted;
        public event Action LevelFailed;

        public void Initialize()
        {
            _enemySpawner.Initialize();
            _enemySpawner.StopSpawn();
            _environmentControl.Initialize(_levelConfig.TargetDistance);
            _enemySpawner.SetSpawnLimit(
                _environmentControl.FinishZ,
                _victoryNoSpawnZoneDistance);
            _character.Died += HandleCharacterDied;
            Time.timeScale = 0f;
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
            Time.timeScale = 0f;
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
                && TravelledDistance >= TargetDistance - _finishAlignmentStartDistance)
            {
                _isFinishAlignmentStarted = true;
                _character.BeginFinishAlignment(
                    _environmentControl.FinishCenterX,
                    _finishAlignmentDistance);
            }

            if (!_isFinishGateOpened && TravelledDistance >= TargetDistance - _gateOpenDistance)
            {
                _isFinishGateOpened = true;
                _environmentControl.OpenFinishGateWithVictoryEffects();
            }

            if (!_isCameraFrozen && TravelledDistance >= TargetDistance - _cameraFreezeDistance)
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
            _deathSequenceStartTimeScale = Mathf.Max(Time.timeScale, _deathTimeScale);
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
            _environmentControl.CloseFinishGate();

            _victorySequenceElapsed = 0f;
            _isPlayingVictorySequence = true;

            if (_victoryPresentationDuration <= 0f)
                CompleteVictorySequence();
        }

        private void UpdateVictorySequence()
        {
            _victorySequenceElapsed += Time.unscaledDeltaTime;

            if (_victorySequenceElapsed >= _victoryPresentationDuration)
                CompleteVictorySequence();
        }

        private void CompleteVictorySequence()
        {
            _isPlayingVictorySequence = false;
            Time.timeScale = 0f;
            LevelCompleted?.Invoke();
        }

        private void UpdateDeathSequence()
        {
            _deathSequenceElapsed += Time.unscaledDeltaTime;

            if (_deathSlowMotionDuration <= 0f)
            {
                Time.timeScale = _deathTimeScale;
            }
            else
            {
                float slowMotionProgress = Mathf.Clamp01(
                    _deathSequenceElapsed / _deathSlowMotionDuration);
                float easedProgress = Mathf.SmoothStep(0f, 1f, slowMotionProgress);
                Time.timeScale = Mathf.Lerp(
                    _deathSequenceStartTimeScale,
                    _deathTimeScale,
                    easedProgress);
            }

            if (_deathSequenceElapsed >= _deathPresentationDuration)
                CompleteDeathSequence();
        }

        private void CompleteDeathSequence()
        {
            _isPlayingDeathSequence = false;
            Time.timeScale = 0f;
            LevelFailed?.Invoke();
        }

        private void StartTimeRamp()
        {
            if (_startRampDuration <= 0f)
            {
                Time.timeScale = 1f;
                _isTimeRamping = false;
                return;
            }

            _timeRampElapsed = 0f;
            _isTimeRamping = true;
            Time.timeScale = 0f;
        }

        private void StartShootingDelay()
        {
            _shootingDelayElapsed = 0f;
            _isWaitingForShooting = true;

            if (_shootingDelay <= 0f)
                StartShooting();
        }

        private void UpdateShootingDelay()
        {
            if (!_isWaitingForShooting || Time.timeScale <= 0f)
                return;

            _shootingDelayElapsed += Time.unscaledDeltaTime;

            if (_shootingDelayElapsed >= _shootingDelay)
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

            float progress = Mathf.Clamp01(_timeRampElapsed / _startRampDuration);
            Time.timeScale = _startRampCurve.Evaluate(progress);

            if (progress >= 1f)
            {
                Time.timeScale = 1f;
                _isTimeRamping = false;
            }
        }
    }
}
