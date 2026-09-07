using UnityEngine;

namespace Gameplay.Enemies
{
    [CreateAssetMenu(fileName = "EnemySpawnerConfig", menuName = "Gameplay/Enemy Spawner Config")]
    public class EnemySpawnerConfig : ScriptableObject
    {
        [Header("Pool")]
        [SerializeField, Min(1)] private int _initialPoolSize = 72;

        [Header("Spawn Rate")]
        [SerializeField, Min(0f)] private float _initialDelay = 0.75f;
        [SerializeField, Min(0.05f)] private float _startSpawnInterval = 1.35f;
        [SerializeField, Min(0.05f)] private float _minSpawnInterval = 0.45f;
        [SerializeField, Min(0f)] private float _spawnIntervalDecreasePerMinute = 0.25f;

        [Header("Waves")]
        [SerializeField, Min(1)] private int _startSpawnCount = 4;
        [SerializeField, Min(1)] private int _maxSpawnCount = 16;
        [SerializeField, Min(1f)] private float _spawnCountIncreaseEverySeconds = 30f;
        [SerializeField, Min(1)] private int _startMaxAliveEnemies = 32;
        [SerializeField, Min(1)] private int _maxAliveEnemies = 96;
        [SerializeField, Min(0f)] private float _maxAliveIncreasePerMinute = 16f;

        [Header("Road")]
        [SerializeField, Min(1)] private int _laneCount = 3;
        [SerializeField, Min(0f)] private float _roadHalfWidth = 3.5f;
        [SerializeField, Min(0f)] private float _laneJitter = 0.35f;
        [SerializeField, Min(0f)] private float _spawnDistance = 42f;
        [SerializeField, Min(0f)] private float _spawnDistanceJitter = 8f;
        [SerializeField, Min(0f)] private float _forwardSpacingInWave = 3f;
        [SerializeField, Min(0f)] private float _minimumSpawnAheadDistance = 8f;
        [SerializeField, Min(0f)] private float _despawnDistanceBehindTarget = 14f;
        [SerializeField] private float _spawnY = 0f;
        [SerializeField] private float _enemyYaw = 180f;

        [Header("Chase")]
        [SerializeField, Min(0f)] private float _activationDistance = 18f;
        [SerializeField, Min(0f)] private float _moveSpeed = 5.5f;
        [SerializeField, Min(0f)] private float _catchUpDistanceBehindTarget = 1.5f;
        [SerializeField, Min(0f)] private float _catchUpMoveSpeed = 11f;
        [SerializeField, Min(0f)] private float _rotationSpeed = 360f;

        [Header("Contact Damage")]
        [SerializeField, Min(0f)] private float _contactDistance = 1.4f;
        [SerializeField, Min(0f)] private float _contactDamage = 15f;

        [Header("Death Sequence")]
        [SerializeField, Min(0f)] private float _deathSurroundRadius = 2.6f;
        [SerializeField, Min(0f)] private float _deathSurroundRadiusJitter = 0.45f;

        public int InitialPoolSize => _initialPoolSize;
        public float InitialDelay => _initialDelay;
        public float SpawnDistance => _spawnDistance;
        public float SpawnDistanceJitter => _spawnDistanceJitter;
        public float ForwardSpacingInWave => _forwardSpacingInWave;
        public float MinimumSpawnAheadDistance => _minimumSpawnAheadDistance;
        public float DespawnDistanceBehindTarget => _despawnDistanceBehindTarget;
        public float SpawnY => _spawnY;
        public float EnemyYaw => _enemyYaw;
        public float ActivationDistance => _activationDistance;
        public float ContactDistance => _contactDistance;
        public float ContactDamage => _contactDamage;
        public float MoveSpeed => _moveSpeed;
        public float CatchUpDistanceBehindTarget => _catchUpDistanceBehindTarget;
        public float CatchUpMoveSpeed => _catchUpMoveSpeed;
        public float RotationSpeed => _rotationSpeed;
        public int LaneCount => Mathf.Max(1, _laneCount);

        public float GetDeathSurroundRadius()
        {
            return Mathf.Max(0f, _deathSurroundRadius
                + Random.Range(-_deathSurroundRadiusJitter, _deathSurroundRadiusJitter));
        }

        public float GetSpawnInterval(float elapsedSeconds)
        {
            float elapsedMinutes = elapsedSeconds / 60f;
            return Mathf.Max(_minSpawnInterval, _startSpawnInterval - elapsedMinutes * _spawnIntervalDecreasePerMinute);
        }

        public int GetSpawnCount(float elapsedSeconds)
        {
            int extraEnemies = Mathf.FloorToInt(elapsedSeconds / _spawnCountIncreaseEverySeconds);
            return Mathf.Clamp(_startSpawnCount + extraEnemies, 1, _maxSpawnCount);
        }

        public int GetMaxAliveEnemies(float elapsedSeconds)
        {
            int extraEnemies = Mathf.FloorToInt(elapsedSeconds / 60f * _maxAliveIncreasePerMinute);
            return Mathf.Clamp(_startMaxAliveEnemies + extraEnemies, 1, _maxAliveEnemies);
        }

        public float GetLaneOffset(int laneIndex)
        {
            if (LaneCount == 1)
                return 0f;

            float t = laneIndex / (LaneCount - 1f);
            float laneCenter = Mathf.Lerp(-_roadHalfWidth, _roadHalfWidth, t);
            return laneCenter + Random.Range(-_laneJitter, _laneJitter);
        }
    }
}
