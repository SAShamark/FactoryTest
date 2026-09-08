using System;
using UnityEngine;

namespace Gameplay.Entities.BaseUnit
{
    public abstract class BaseUnitControl : MonoBehaviour
    {
        [SerializeField] private Health _health;

        public Health Health => _health;
        public bool IsAlive { get; protected set; }

        public event Action Died;

        protected abstract HitFeedback HitFeedback { get; }

        protected void InitializeUnit()
        {
            HitFeedback?.Reset();
            _health.Init();
            _health.OnDeath -= Die;
            _health.OnDeath += Die;
            IsAlive = true;
        }

        protected virtual void Awake()
        {
            HitFeedback?.Initialize();
        }

        protected virtual void OnDestroy()
        {
            HitFeedback?.Dispose();
            _health.OnDeath -= Die;
        }

        public void ApplyDamage(float damage)
        {
            _health.ApplyDamage(damage);
        }

        public virtual void PlayHitFeedback(Vector3 hitPosition)
        {
            HitFeedback?.Play(hitPosition);
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
