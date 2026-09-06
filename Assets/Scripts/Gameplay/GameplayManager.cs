using System;
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
        [SerializeField, Min(0f)] private float _startRampDuration = 1.25f;
        [SerializeField] private AnimationCurve _startRampCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private bool _isTimeRamping;
        private float _timeRampElapsed;
        private bool _isFinished;

        public float TravelledDistance => _character.TravelledDistance;
        public float TargetDistance => _levelConfig.TargetDistance;

        public event Action LevelCompleted;
        public event Action LevelFailed;

        public void Initialize()
        {
            _enemySpawner.Initialize();
            _enemySpawner.StopSpawn();
            _character.Died += HandleCharacterDied;
            Time.timeScale = 0f;
        }

        public void Dispose()
        {
            _character.Died -= HandleCharacterDied;
        }

        internal void LateUpdate()
        {
            UpdateTimeRamp();
            _enemySpawner.LateUpdate();
            CheckTargetReached();
        }

        public void StartGameplay()
        {
            _enemySpawner.StartSpawn(true);
            StartTimeRamp();
        }

        public void PauseGameplay()
        {
            _isTimeRamping = false;
            Time.timeScale = 0f;
            _enemySpawner.StopSpawn();
        }

        public void ContinueGameplay()
        {
            _enemySpawner.StartSpawn();
            StartTimeRamp();
        }

        private void CheckTargetReached()
        {
            if (_isFinished || TravelledDistance < TargetDistance)
                return;

            Finish();
            LevelCompleted?.Invoke();
        }

        private void HandleCharacterDied()
        {
            if (_isFinished)
                return;

            Finish();
            LevelFailed?.Invoke();
        }

        private void Finish()
        {
            _isFinished = true;
            PauseGameplay();
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
