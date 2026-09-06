using Gameplay.Entities.Enemies;
using Services.ObjectPool;
using UnityEngine;

namespace Gameplay.Entities.Character
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _speed = 25f;
        [SerializeField] private float _lifetime = 3f;
        [SerializeField] private float _damage = 50f;

        private float _remainingLifetime;

        public void Launch(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            _remainingLifetime = _lifetime;
        }

        private void Update()
        {
            transform.position += transform.forward * (_speed * Time.deltaTime);

            _remainingLifetime -= Time.deltaTime;
            if (_remainingLifetime <= 0f)
                ReturnToPool();
        }

        private void OnTriggerEnter(Collider other)
        {
            EnemyControl enemy = other.GetComponentInParent<EnemyControl>();
            if (enemy != null)
                enemy.Hit(_damage, transform.position);

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (TryGetComponent(out BasePoolDestroyable poolDestroyable))
                poolDestroyable.DestroyObject();
        }
    }
}
