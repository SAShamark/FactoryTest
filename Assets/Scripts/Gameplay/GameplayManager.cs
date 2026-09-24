using System;
using Gameplay.CameraLogic;
using Gameplay.Entities;
using Gameplay.Entities.BaseUnit;
using Gameplay.Entities.Character;
using Gameplay.Entities.Enemies;
using Services.Sequence;
using UnityEngine;

namespace Gameplay
{
    [Serializable]
    public class GameplayManager
    {
        private enum GameState
        {
            Ready,
            Playing,
            Paused,
            DeathSequence,
            VictorySequence,
            Finished
        }

        private sealed class ElapsedTimer
        {
            public float Elapsed { get; private set; }
            public bool IsRunning { get; private set; }

            public void Start()
            {
                Elapsed = 0f;
                IsRunning = true;
            }

            public void Stop() => IsRunning = false;

            public void Advance(float deltaTime) => Elapsed += deltaTime;
        }

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

        private readonly ElapsedTimer _timeRamp = new();
        private readonly ElapsedTimer _shootingDelay = new();
        private readonly ElapsedTimer _deathSequence = new();
        private readonly ElapsedTimer _victorySequence = new();
        private bool _isShootingAllowed;
        private float _deathSequenceStartTimeScale;
        private bool _isFinishAlignmentStarted;
        private bool _isFinishGateOpened;
        private bool _isCameraFrozen;
        private GameState _state;
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

        public void Initialize(IGameplaySequence gameplaySequence, FloatingTextService floatingTextService)
        {
            _gameplaySequence = gameplaySequence;
            _cameraController.SetFollowTarget(_character.transform);
            _enemySpawner.Initialize(floatingTextService);
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
            if (_state == GameState.DeathSequence)
            {
                UpdateDeathSequence();
                return;
            }

            if (_state == GameState.VictorySequence)
            {
                UpdateVictorySequence();
                return;
            }

            if (_state != GameState.Playing)
            {
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
            if (_state != GameState.Ready)
            {
                return;
            }

            _state = GameState.Playing;
            _character.StartMoving();
            _enemySpawner.StartSpawn(true);
            _isShootingAllowed = false;
            _character.StopShooting();
            StartShootingDelay();
            StartTimeRamp();
        }

        public void PauseGameplay()
        {
            if (_state != GameState.Playing)
            {
                return;
            }

            _state = GameState.Paused;
            _timeRamp.Stop();
            _gameplaySequence.StopGame();
            _enemySpawner.StopSpawn();
        }

        public void ContinueGameplay()
        {
            if (_state != GameState.Paused)
            {
                return;
            }

            _state = GameState.Playing;
            _enemySpawner.StartSpawn();
            StartTimeRamp();
        }

        private void CheckTargetReached()
        {
            if (_state != GameState.Playing)
            {
                return;
            }

            if (!_isFinishAlignmentStarted
                && TravelledDistance >= TargetDistance - _levelConfig.AlignmentStartDistance)
            {
                _isFinishAlignmentStarted = true;
                _character.BeginFinishAlignment(_environmentControl.FinishCenterX, _levelConfig.AlignmentDistance);
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
            {
                StartVictorySequence();
            }
        }

        private void HandleCharacterDied()
        {
            if (_state != GameState.Playing)
            {
                return;
            }

            _state = GameState.DeathSequence;
            _timeRamp.Stop();
            _shootingDelay.Stop();
            _isShootingAllowed = false;
            _character.StopShooting();
            _enemySpawner.StopSpawn();
            _enemySpawner.BeginSurroundingTarget();

            _deathSequence.Start();
            _deathSequenceStartTimeScale = Mathf.Max(Time.timeScale, _deathTimeScale);

            if (_deathPresentationDuration <= 0f)
            {
                CompleteDeathSequence();
            }
        }

        private void StartVictorySequence()
        {
            _state = GameState.VictorySequence;
            _timeRamp.Stop();
            _shootingDelay.Stop();
            _isShootingAllowed = false;
            _character.StopShooting();
            _enemySpawner.StopAndDespawnAllEnemies();
            _environmentControl.StopGroundRecycling();
            _environmentControl.CloseFinishGate();

            _victorySequence.Start();
            LevelCompleted?.Invoke();

            if (_levelConfig.VictoryPresentationDuration <= 0f)
            {
                CompleteVictorySequence();
            }
        }

        private void UpdateVictorySequence()
        {
            _victorySequence.Advance(Time.unscaledDeltaTime);

            if (_victorySequence.Elapsed < _levelConfig.VictoryPresentationDuration)
            {
                return;
            }

            bool exitedViewport = _cameraController
                .HasFollowingTargetExitedViewport(_levelConfig.VictoryExitViewportMargin);
            bool timedOut = _levelConfig.VictoryMaximumDriveDuration <= 0f
                || _victorySequence.Elapsed >= _levelConfig.VictoryMaximumDriveDuration;

            if (exitedViewport || timedOut)
            {
                CompleteVictorySequence();
            }
        }

        private void CompleteVictorySequence()
        {
            _victorySequence.Stop();
            _state = GameState.Finished;
            _character.StopGameplay();
            _gameplaySequence.StartGame();
        }

        private void UpdateDeathSequence()
        {
            _deathSequence.Advance(Time.unscaledDeltaTime);

            if (_deathSlowMotionDuration <= 0f)
            {
                _gameplaySequence.SetTimeScale(_deathTimeScale);
            }
            else
            {
                float slowMotionProgress = Mathf.Clamp01(
                    _deathSequence.Elapsed / _deathSlowMotionDuration);
                float easedProgress = Mathf.SmoothStep(0f, 1f, slowMotionProgress);
                _gameplaySequence.SetTimeScale(Mathf.Lerp(_deathSequenceStartTimeScale, _deathTimeScale,
                    easedProgress));
            }

            if (_deathSequence.Elapsed >= _deathPresentationDuration)
            {
                CompleteDeathSequence();
            }
        }

        private void CompleteDeathSequence()
        {
            _deathSequence.Stop();
            _state = GameState.Finished;
            _gameplaySequence.StopGame();
            LevelFailed?.Invoke();
        }

        private void StartTimeRamp()
        {
            if (_levelConfig.StartRampDuration <= 0f)
            {
                _gameplaySequence.StartGame();
                _timeRamp.Stop();
                return;
            }

            _timeRamp.Start();
            _gameplaySequence.StopGame();
        }

        private void StartShootingDelay()
        {
            _shootingDelay.Start();

            if (_levelConfig.ShootingDelay <= 0f)
            {
                StartShooting();
            }
        }

        private void UpdateShootingDelay()
        {
            if (!_shootingDelay.IsRunning || Time.timeScale <= 0f)
            {
                return;
            }

            _shootingDelay.Advance(Time.unscaledDeltaTime);

            if (_shootingDelay.Elapsed >= _levelConfig.ShootingDelay)
            {
                StartShooting();
            }
        }

        private void StartShooting()
        {
            _shootingDelay.Stop();
            _isShootingAllowed = true;
            UpdateTurretState();
        }

        private void UpdateTurretState()
        {
            if (!_isShootingAllowed)
            {
                return;
            }

            if (_enemySpawner.HasAliveEnemies)
            {
                _character.StartShooting();
            }
            else
                _character.StopShooting();
        }

        private void UpdateTimeRamp()
        {
            if (!_timeRamp.IsRunning)
            {
                return;
            }

            _timeRamp.Advance(Time.unscaledDeltaTime);

            float progress = Mathf.Clamp01(_timeRamp.Elapsed / _levelConfig.StartRampDuration);
            _gameplaySequence.SetTimeScale(_levelConfig.StartRampCurve.Evaluate(progress));

            if (progress >= 1f)
            {
                _gameplaySequence.StartGame();
                _timeRamp.Stop();
            }
        }
    }
}
