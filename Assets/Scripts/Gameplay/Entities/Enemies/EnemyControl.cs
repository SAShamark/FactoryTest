using System;
using Gameplay.Entities.BaseUnit;
using Services.ObjectPool;
using UnityEngine;

namespace Gameplay.Entities.Enemies
{
    public class EnemyControl : BaseUnitControl
    {
        private enum MovementState
        {
            Wandering,
            Chasing,
            Surrounding
        }

        [SerializeField] private EnemyAnimationControl _animationControl;
        [SerializeField] private EnemyEffects _effects;

        private readonly EnemyMovementLogic _movementLogic = new();

        private Transform _target;
        private BaseUnitControl _targetUnit;
        private MovementState _movementState;
        private EnemyConfig _config;
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
            _movementState = MovementState.Wandering;
            SetCollidersEnabled(true);
            _animationControl.SetEnabled(true);
            _animationControl.SetRun(false);
        }

        public void Spawn(Vector3 position, Quaternion rotation, Transform target,
            EnemyConfig config, FloatingTextService floatingText)
        {
            Spawn(position, rotation);
            _target = target;
            _targetUnit = target.GetComponentInParent<BaseUnitControl>();
            _config = config;
            _effects.SetFloatingText(floatingText);
            _movementLogic.Initialize(config);
        }

        private void Update()
        {
            if (_target == null)
            {
                return;
            }

            if (_movementState == MovementState.Surrounding)
            {
                _animationControl.SetRun(_movementLogic.Surround(
                    transform, _target.position + _surroundOffset, _target.position));
                return;
            }

            Vector3 toTarget = _target.position - transform.position;
            toTarget.y = 0f;

            if (_movementState == MovementState.Wandering)
            {
                if (toTarget.sqrMagnitude > _config.ActivationDistance * _config.ActivationDistance)
                {
                    _animationControl.SetRun(_movementLogic.Wander(transform));
                    return;
                }

                _movementState = MovementState.Chasing;
                _movementLogic.StopWander();
            }

            _animationControl.SetRun(_movementLogic.Chase(transform, toTarget));
        }

        public void BeginSurrounding(Vector3 offset)
        {
            if (!IsAlive)
            {
                return;
            }

            _movementState = MovementState.Surrounding;
            _surroundOffset = offset;
        }

        public void Hit(float damage, Vector3 hitPosition)
        {
            if (!IsAlive || damage <= 0f)
            {
                return;
            }

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
            _movementState = MovementState.Wandering;
            _movementLogic.StopWander();
            _animationControl.SetRun(false);

            GetComponent<BasePoolDestroyable>().DestroyObject();
        }

        protected override void Die()
        {
            if (!IsAlive)
            {
                return;
            }

            MarkAsDead();
            _target = null;
            _targetUnit = null;
            _movementState = MovementState.Wandering;
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
            if (!IsAlive || _movementState == MovementState.Surrounding
                || _targetUnit == null || !_targetUnit.IsAlive)
                return;

            if (other.GetComponentInParent<BaseUnitControl>() != _targetUnit)
            {
                return;
            }

            BaseUnitControl targetUnit = _targetUnit;
            targetUnit.PlayHitFeedback(transform.position);
            targetUnit.ApplyDamage(_config.ContactDamage);
            Die();
        }

        private void SetCollidersEnabled(bool isEnabled)
        {
            foreach (Collider enemyCollider in _colliders)
                enemyCollider.enabled = isEnabled;
        }
    }
}
