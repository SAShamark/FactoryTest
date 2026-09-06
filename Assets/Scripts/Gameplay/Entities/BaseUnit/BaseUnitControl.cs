using System;
using UnityEngine;

namespace Gameplay.Entities.BaseUnit
{
    public abstract class BaseUnitControl : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private HitFeedback _hitFeedback;
        [SerializeField] private float _aimHeight = 1f;

        public Health Health => _health;
        public bool IsAlive { get; protected set; }
        public Vector3 AimPosition => transform.position + Vector3.up * _aimHeight;

        public event Action Died;

        protected HitFeedback HitFeedback => _hitFeedback;

        protected void InitializeUnit()
        {
            _health.Init();
            _health.OnDeath -= Die;
            _health.OnDeath += Die;
            IsAlive = true;
        }

        protected virtual void OnDestroy()
        {
            _health.OnDeath -= Die;
        }

        public void ApplyDamage(float damage)
        {
            _health.ApplyDamage(damage);
        }

        public virtual void PlayHitFeedback(Vector3 hitPosition)
        {
            _hitFeedback.Play(hitPosition);
        }

        protected void MarkAsDead()
        {
            IsAlive = false;
            _health.HideBar();
            Died?.Invoke();
        }

        protected abstract void Die();
    }
}
