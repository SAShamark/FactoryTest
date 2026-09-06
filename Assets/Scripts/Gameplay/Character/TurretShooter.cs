using Services.ObjectPool;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Character
{
    public class TurretShooter : MonoBehaviour
    {
        [SerializeField] private Transform _turret;
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private Transform _projectileContainer;
        [SerializeField] private int _initialPoolSize = 12;
        [SerializeField] private float _shotsPerSecond = 5f;
        [SerializeField] private float _muzzleDistance = 1.5f;

        private ObjectPool<Projectile> _projectilePool;
        private float _nextShotTime;

        private void Awake()
        {
            _projectilePool = new ObjectPool<Projectile>(
                _projectilePrefab,
                _initialPoolSize,
                _projectileContainer);
        }

        private void LateUpdate()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null || !pointer.press.isPressed)
                return;

            Fire();
        }

        public void Fire()
        {
            if (Time.time < _nextShotTime)
                return;

            _nextShotTime = Time.time + 1f / _shotsPerSecond;

            Projectile projectile = _projectilePool.GetFreeElement();
            Vector3 muzzlePosition = _turret.position + _turret.forward * _muzzleDistance;
            projectile.Launch(muzzlePosition, _turret.rotation);
        }
    }
}
