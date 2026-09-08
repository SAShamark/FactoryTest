using System;
using System.Collections.Generic;
using Gameplay.Entities.BaseUnit;
using Services.ObjectPool;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Entities.Enemies
{
    [Serializable]
    public class EnemySpawner
    {
        [SerializeField] private EnemySpawnerConfig _config;
        [SerializeField] private Transform _target;
        [SerializeField] private EnemyControl _enemyPrefab;
        [SerializeField] private Transform _enemyContainer;
        [SerializeField] private CameraController _cameraController;

        private readonly List<EnemyControl> _aliveEnemies = new();
        private readonly List<int> _availableLanes = new();
        private ObjectPool<EnemyControl> _enemyPool;
        private FloatingTextService _floatingTextService;
        private bool _isInitialized;
        private bool _isSpawning;
        private bool _isInitialWavePending;
        private bool _hasSpawnLimit;
        private float _maxSpawnZ;
        private float _elapsedSeconds;
        private float _spawnTimer;

        public event Action EnemyKilled;

        public bool HasAliveEnemies
        {
            get
            {
                for (int i = 0; i < _aliveEnemies.Count; i++)
                {
                    EnemyControl enemy = _aliveEnemies[i];
                    if (enemy != null && enemy.gameObject.activeSelf && enemy.IsAlive)
                        return true;
                }

                return false;
            }
        }

        public void Initialize(FloatingTextService floatingTextService)
        {
            if (_isInitialized)
                return;

            _floatingTextService = floatingTextService;

            if (_config != null && _enemyPrefab != null)
                _enemyPool = new ObjectPool<EnemyControl>(_enemyPrefab, _config.InitialPoolSize, _enemyContainer);

            _spawnTimer = _config != null ? _config.InitialDelay : 0f;
            _isInitialized = true;
        }

        internal void LateUpdate()
        {
            if (_config == null || _target == null || _enemyPool == null)
                return;

            CleanupInactiveEnemies();
            DespawnEnemiesBehindTarget();

            if (!_isSpawning)
                return;

            if (HasNoSpawnSpace())
            {
                StopSpawn();
                return;
            }

            _elapsedSeconds += Time.deltaTime;
            TickSpawning();
        }

        public void StartSpawn(bool resetProgress = false)
        {
            if (resetProgress)
            {
                _elapsedSeconds = 0f;
                _isInitialWavePending = true;
                DespawnAllEnemies();
            }

            _spawnTimer = _config != null ? _config.InitialDelay : 0f;
            _isSpawning = true;
        }

        public void StopSpawn()
        {
            _isSpawning = false;
        }

        public void SetSpawnLimit(float finishZ, float safeZoneDistance)
        {
            _maxSpawnZ = finishZ - Mathf.Max(0f, safeZoneDistance);
            _hasSpawnLimit = true;
        }

        public void StopAndDespawnAllEnemies()
        {
            StopSpawn();
            DespawnAllEnemies();
        }

        public void BeginSurroundingTarget()
        {
            StopSpawn();
            CleanupInactiveEnemies();

            int enemyCount = _aliveEnemies.Count;
            if (enemyCount == 0 || _target == null || _config == null)
                return;

            float angleStep = 360f / enemyCount;
            float angleOffset = Random.Range(0f, 360f);

            for (int i = 0; i < enemyCount; i++)
            {
                float angle = angleOffset + angleStep * i;
                float radius = _config.GetDeathSurroundRadius();
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
                _aliveEnemies[i].BeginSurrounding(offset);
            }
        }

        private bool HasNoSpawnSpace()
        {
            if (!_hasSpawnLimit)
                return false;

            return _target.position.z + _config.MinimumSpawnAheadDistance >= _maxSpawnZ;
        }

        private void TickSpawning()
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f)
                return;

            SpawnNextEnemy();
            _spawnTimer += _config.GetSpawnInterval(_elapsedSeconds);
        }

        private void SpawnNextEnemy()
        {
            int freeSlots = _config.GetMaxAliveEnemies(_elapsedSeconds) - _aliveEnemies.Count;
            if (freeSlots <= 0)
                return;

            if (SpawnEnemy(_isInitialWavePending))
                _isInitialWavePending = false;
        }

        private bool SpawnEnemy(bool isInitialWave)
        {
            if (!TryGetSpawnPosition(isInitialWave, out Vector3 position))
                return false;

            EnemyControl enemy = _enemyPool.GetFreeElement();
            Quaternion rotation = Quaternion.Euler(0f, _config.EnemyYaw, 0f);

            enemy.Spawn(position, rotation, _target, _config.Enemy, _floatingTextService);
            enemy.KilledByPlayer -= HandleEnemyKilled;
            enemy.KilledByPlayer += HandleEnemyKilled;
            _aliveEnemies.Add(enemy);
            return true;
        }

        private void HandleEnemyKilled()
        {
            EnemyKilled?.Invoke();
        }

        private bool TryGetSpawnPosition(bool isInitialWave, out Vector3 position)
        {
            int lane = TakeRandomLane();
            float x = _config.GetLaneOffset(lane);
            float spawnDistance = isInitialWave
                ? _config.InitialSpawnDistance
                : _config.SpawnDistance;
            float z = _target.position.z + spawnDistance;

            if (!isInitialWave)
                z += Random.Range(-_config.SpawnDistanceJitter, _config.SpawnDistanceJitter);

            float maxSpawnZ = float.PositiveInfinity;

            if (_hasSpawnLimit)
            {
                maxSpawnZ = _maxSpawnZ;
                z = Mathf.Min(z, maxSpawnZ);
            }

            position = new Vector3(x, _config.SpawnY, z);

            if (!TryGetHiddenSpawnZ(position, maxSpawnZ, out float hiddenZ))
                return false;

            position.z = hiddenZ;
            return true;
        }

        private bool TryGetHiddenSpawnZ(Vector3 position, float maxSpawnZ, out float hiddenZ)
        {
            hiddenZ = position.z;

            Camera viewCamera = _cameraController != null ? _cameraController.OutputCamera : null;
            if (viewCamera == null)
                return true;

            float padding = _config.SpawnViewportPadding;
            Vector3 viewportPosition = viewCamera.WorldToViewportPoint(position);

            bool isVisible = viewportPosition.z > 0f
                             && viewportPosition.x >= -padding && viewportPosition.x <= 1f + padding
                             && viewportPosition.y >= -padding && viewportPosition.y <= 1f + padding;

            if (!isVisible)
                return true;

            Ray topEdgeRay = viewCamera.ViewportPointToRay(
                new Vector3(Mathf.Clamp01(viewportPosition.x), 1f + padding, 0f));
            Plane spawnPlane = new Plane(Vector3.up, position);

            if (!spawnPlane.Raycast(topEdgeRay, out float distance))
                return false;

            float z = topEdgeRay.GetPoint(distance).z;
            if (z <= position.z || z > maxSpawnZ)
                return false;

            hiddenZ = z;
            return true;
        }

        private int TakeRandomLane()
        {
            if (_availableLanes.Count == 0)
                RefillLaneBag();

            int index = Random.Range(0, _availableLanes.Count);
            int lane = _availableLanes[index];
            _availableLanes.RemoveAt(index);
            return lane;
        }

        private void RefillLaneBag()
        {
            _availableLanes.Clear();
            for (int i = 0; i < _config.LaneCount; i++)
                _availableLanes.Add(i);
        }

        private void DespawnEnemiesBehindTarget()
        {
            float despawnZ = _target.position.z - _config.DespawnDistanceBehindTarget;

            for (int i = _aliveEnemies.Count - 1; i >= 0; i--)
            {
                EnemyControl enemy = _aliveEnemies[i];
                if (enemy == null || !enemy.gameObject.activeSelf)
                    continue;

                if (enemy.transform.position.z < despawnZ)
                    enemy.Despawn();
            }
        }

        private void CleanupInactiveEnemies()
        {
            for (int i = _aliveEnemies.Count - 1; i >= 0; i--)
            {
                EnemyControl enemy = _aliveEnemies[i];
                if (enemy == null || !enemy.gameObject.activeSelf)
                    _aliveEnemies.RemoveAt(i);
            }
        }

        private void DespawnAllEnemies()
        {
            for (int i = _aliveEnemies.Count - 1; i >= 0; i--)
            {
                EnemyControl enemy = _aliveEnemies[i];
                if (enemy != null && enemy.gameObject.activeSelf)
                    enemy.Despawn();
            }

            _aliveEnemies.Clear();
        }
    }
}
