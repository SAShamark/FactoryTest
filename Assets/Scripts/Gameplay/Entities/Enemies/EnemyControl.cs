using System;
using Gameplay.Entities.BaseUnit;
using Services.ObjectPool;
using UnityEngine;

namespace Gameplay.Entities.Enemies
{
    public class EnemyControl : BaseUnitControl
    {
        [SerializeField] private EnemyAnimationControl _animationControl;
        [SerializeField] private EnemyEffects _effects;

        private readonly EnemyMovementLogic _movementLogic = new();

        private Transform _target;
        private BaseUnitControl _targetUnit;
        private bool _isChasing;
        private bool _isSurrounding;
        private bool _hasDealtContactDamage;
        private EnemyConfig _config;
        private float _activationDistanceSqr;
        private Vector3 _surroundOffset;
        private Collider[] _colliders;

        protected override HitFeedback HitFeedback => _effects.HitFeedback;

        public event Action KilledByPlayer;

        protected override void Awake()
        {
            base.Awake();
            _colliders = GetComponentsInChildren<Collider>(true);
        }

        public void Spawn(Vector3 position, Quaternion rotation)
        {
            _effects.ResetVisuals();
            InitializeUnit();
            transform.SetPositionAndRotation(position, rotation);
            _target = null;
            _targetUnit = null;
            _isChasing = false;
            _isSurrounding = false;
            _hasDealtContactDamage = false;
            SetCollidersEnabled(true);
            _animationControl.SetEnabled(true);
            _animationControl.SetRun(false);
        }

        public void Spawn(Vector3 position, Quaternion rotation, Transform target,
            EnemyConfig config, FloatingTextControl floatingText)
        {
            Spawn(position, rotation);
            _target = target;
            _targetUnit = target.GetComponentInParent<BaseUnitControl>();
            _config = config;
            _activationDistanceSqr = config.ActivationDistance * config.ActivationDistance;
            _effects.SetFloatingText(floatingText);
            _movementLogic.Initialize(config);
        }

        private void Update()
        {
            if (_target == null)
                return;

            if (_isSurrounding)
            {
                _animationControl.SetRun(
                    _movementLogic.Surround(transform, _target.position + _surroundOffset, _target.position));
                return;
            }

            Vector3 toTarget = _target.position - transform.position;
            toTarget.y = 0f;

            if (!_isChasing)
            {
                if (toTarget.sqrMagnitude > _activationDistanceSqr)
                {
                    _animationControl.SetRun(_movementLogic.Wander(transform));
                    return;
                }

                _isChasing = true;
                _movementLogic.StopWander();
            }

            _animationControl.SetRun(_movementLogic.Chase(transform, toTarget));
        }

        public void BeginSurrounding(Vector3 offset)
        {
            if (!IsAlive)
                return;

            _isSurrounding = true;
            _isChasing = true;
            _surroundOffset = offset;
        }

        public void Hit(float damage, Vector3 hitPosition)
        {
            if (!IsAlive || damage <= 0f)
                return;

            float appliedDamage = Mathf.Min(damage, Health.CurrentHealth);
            bool isLethal = appliedDamage >= Health.CurrentHealth;
            PlayHitFeedback(hitPosition);
            ApplyDamage(damage);

            if (isLethal)
            {
                KilledByPlayer?.Invoke();
                _effects.ShowReward();
            }
            else
            {
                _effects.ShowDamage(appliedDamage);
            }
        }

        public void Despawn()
        {
            _target = null;
            _targetUnit = null;
            _isChasing = false;
            _isSurrounding = false;
            _movementLogic.StopWander();
            _animationControl.SetRun(false);

            if (TryGetComponent(out BasePoolDestroyable poolDestroyable))
            {
                poolDestroyable.DestroyObject();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected override void Die()
        {
            if (!IsAlive)
                return;

            MarkAsDead();
            _target = null;
            _targetUnit = null;
            _isChasing = false;
            _isSurrounding = false;
            _movementLogic.StopWander();
            SetCollidersEnabled(false);
            _animationControl.SetRun(false);
            _animationControl.SetEnabled(false);
            _effects.PlayDeath(Despawn);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryDamageTarget(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryDamageTarget(other);
        }

        private void TryDamageTarget(Collider other)
        {
            if (!IsAlive || _hasDealtContactDamage || _isSurrounding
                || _targetUnit == null || !_targetUnit.IsAlive)
                return;

            if (other.GetComponentInParent<BaseUnitControl>() != _targetUnit)
                return;

            _hasDealtContactDamage = true;
            BaseUnitControl targetUnit = _targetUnit;
            targetUnit.PlayHitFeedback(transform.position);
            targetUnit.ApplyDamage(_config.ContactDamage);
            Die();
        }

        private void SetCollidersEnabled(bool isEnabled)
        {
            if (_colliders == null)
                return;

            foreach (Collider enemyCollider in _colliders)
                enemyCollider.enabled = isEnabled;
        }
    }
}
