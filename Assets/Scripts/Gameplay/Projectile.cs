using Gameplay.Enemies;
using Services.ObjectPool;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float _speed = 25f;
    [SerializeField] private float _lifetime = 3f;

    private float _remainingLifetime;

    public void Launch(Vector3 position, Quaternion rotation)
    {
        transform.SetParent(null, true);
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
        if (other.TryGetComponent(out EnemyControl enemy))
            enemy.Hit();

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (TryGetComponent(out BasePoolDestroyable poolDestroyable))
            poolDestroyable.DestroyObject();
    }
}
