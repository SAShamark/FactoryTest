using System;
using Services.ObjectPool;
using UnityEngine;

namespace Gameplay.Entities.Character
{
    [Serializable]
    public class TurretShooter
    {
        [SerializeField] private Transform _turret;
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private Transform _projectileContainer;
        [SerializeField] private int _initialPoolSize = 12;
        [SerializeField] private float _shotsPerSecond = 4f;
        [SerializeField] private float _muzzleDistance = 1.5f;

        private ObjectPool<Projectile> _projectilePool;
        private float _nextShotTime;

        public void Initialize()
        {
            _projectileContainer.SetParent(null, true);

            _projectilePool = new ObjectPool<Projectile>(_projectilePrefab, _initialPoolSize, _projectileContainer);
        }

        public void LateUpdate()
        {
            if (Time.timeScale <= 0f || _projectilePool == null)
                return;

            Fire();
        }

        private void Fire()
        {
            if (Time.time < _nextShotTime || _shotsPerSecond <= 0f)
                return;

            _nextShotTime = Time.time + 1f / _shotsPerSecond;

            Projectile projectile = _projectilePool.GetFreeElement();
            Vector3 muzzlePosition = _turret.position + _turret.forward * _muzzleDistance;
            projectile.Launch(muzzlePosition, _turret.rotation);
        }
    }
}
