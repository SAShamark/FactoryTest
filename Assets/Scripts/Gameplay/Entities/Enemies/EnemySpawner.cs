using System;
using System.Collections.Generic;
using Gameplay.Enemies;
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

        private readonly List<EnemyControl> _aliveEnemies = new();
        private readonly List<int> _availableLanes = new();
        private ObjectPool<EnemyControl> _enemyPool;
        private bool _isInitialized;
        private bool _isSpawning;
        private bool _hasSpawnLimit;
        private float _maxSpawnZ;
        private float _elapsedSeconds;
        private float _spawnTimer;

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

        public void Initialize()
        {
            if (_isInitialized)
                return;

            if (_config != null && _enemyPrefab != null)
                _enemyPool = new ObjectPool<EnemyControl>(_enemyPrefab, _config.InitialPoolSize, _enemyContainer);

            _spawnTimer = _config != null ? _config.InitialDelay : 0f;
            _isInitialized = true;
        }

        public void StartSpawn(bool resetProgress = false)
        {
            Initialize();

            if (resetProgress)
            {
                _elapsedSeconds = 0f;
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

        internal void LateUpdate()
        {
            if (!_isSpawning || _config == null || _target == null || _enemyPool == null)
                return;

            if (HasNoSpawnSpace())
            {
                StopSpawn();
                return;
            }

            _elapsedSeconds += Time.deltaTime;

            CleanupInactiveEnemies();
            DespawnEnemiesBehindTarget();
            TickSpawning();
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

            SpawnWave();
            _spawnTimer += _config.GetSpawnInterval(_elapsedSeconds);
        }

        private void SpawnWave()
        {
            int freeSlots = _config.GetMaxAliveEnemies(_elapsedSeconds) - _aliveEnemies.Count;
            if (freeSlots <= 0)
                return;

            int spawnCount = Mathf.Min(_config.GetSpawnCount(_elapsedSeconds), freeSlots);
            RefillLaneBag();

            for (int i = 0; i < spawnCount; i++)
                SpawnEnemy(i);
        }

        private void SpawnEnemy(int waveIndex)
        {
            EnemyControl enemy = _enemyPool.GetFreeElement();
            Vector3 position = GetSpawnPosition(waveIndex);
            Quaternion rotation = Quaternion.Euler(0f, _config.EnemyYaw, 0f);

            enemy.Spawn(
                position,
                rotation,
                _target,
                _config.ActivationDistance,
                _config.ContactDistance,
                _config.ContactDamage,
                _config.MoveSpeed,
                _config.CatchUpDistanceBehindTarget,
                _config.CatchUpMoveSpeed,
                _config.RotationSpeed);
            _aliveEnemies.Add(enemy);
        }

        private Vector3 GetSpawnPosition(int waveIndex)
        {
            int lane = TakeRandomLane();
            float x = _config.GetLaneOffset(lane);
            float z = _target.position.z + _config.SpawnDistance;
            z += Random.Range(-_config.SpawnDistanceJitter, _config.SpawnDistanceJitter);
            z += waveIndex * _config.ForwardSpacingInWave;

            if (_hasSpawnLimit)
            {
                float waveMaxZ = _maxSpawnZ - waveIndex * _config.ForwardSpacingInWave;
                z = Mathf.Min(z, waveMaxZ);
            }

            return new Vector3(x, _config.SpawnY, z);
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
