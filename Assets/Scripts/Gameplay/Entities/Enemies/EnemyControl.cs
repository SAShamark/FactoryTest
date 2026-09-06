using Gameplay.Entities.BaseUnit;
using Services.ObjectPool;
using UnityEngine;

namespace Gameplay.Entities.Enemies
{
    public class EnemyControl : BaseUnitControl
    {
        private static readonly int IsRun = Animator.StringToHash(nameof(IsRun));
        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int RunState = Animator.StringToHash("Run");

        [SerializeField] private Animator _animator;

        private Transform _target;
        private BaseUnitControl _targetUnit;
        private bool _isChasing;
        private bool _isRunAnimationPlaying;
        private float _activationDistanceSqr;
        private float _contactDistanceSqr;
        private float _contactDamage;
        private float _moveSpeed;
        private float _rotationSpeed;

        public void Spawn(Vector3 position, Quaternion rotation)
        {
            InitializeUnit();
            transform.SetPositionAndRotation(position, rotation);
            _target = null;
            _targetUnit = null;
            _isChasing = false;
            SetRunAnimation(false);
        }

        public void Spawn(
            Vector3 position,
            Quaternion rotation,
            Transform target,
            float activationDistance,
            float contactDistance,
            float contactDamage,
            float moveSpeed,
            float rotationSpeed)
        {
            Spawn(position, rotation);
            _target = target;
            _targetUnit = target.GetComponentInParent<BaseUnitControl>();
            _activationDistanceSqr = activationDistance * activationDistance;
            _contactDistanceSqr = contactDistance * contactDistance;
            _contactDamage = contactDamage;
            _moveSpeed = moveSpeed;
            _rotationSpeed = rotationSpeed;
        }

        private void Update()
        {
            if (_target == null)
                return;

            Vector3 toTarget = _target.position - transform.position;
            toTarget.y = 0f;

            if (TryExplodeIntoTarget(toTarget))
                return;

            if (!_isChasing)
            {
                if (toTarget.sqrMagnitude > _activationDistanceSqr)
                    return;

                _isChasing = true;
            }

            MoveToTarget(toTarget);
        }

        private void MoveToTarget(Vector3 toTarget)
        {
            if (toTarget.sqrMagnitude <= 0.01f)
            {
                SetRunAnimation(false);
                return;
            }

            SetRunAnimation(true);
            Vector3 direction = toTarget.normalized;
            transform.position += direction * (_moveSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime);
        }

        private bool TryExplodeIntoTarget(Vector3 toTarget)
        {
            if (toTarget.sqrMagnitude > _contactDistanceSqr)
                return false;

            if (_targetUnit != null && _targetUnit.IsAlive)
            {
                _targetUnit.PlayHitFeedback(transform.position);
                _targetUnit.ApplyDamage(_contactDamage);
            }

            Die();
            return true;
        }

        public void Hit(float damage, Vector3 hitPosition)
        {
            if (!IsAlive)
                return;

            PlayHitFeedback(hitPosition);
            ApplyDamage(damage);
        }

        public void Despawn()
        {
            _target = null;
            _targetUnit = null;
            _isChasing = false;
            SetRunAnimation(false);

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
            MarkAsDead();
            Despawn();
        }

        private void SetRunAnimation(bool isRun)
        {
            if (_animator == null)
                return;

            _animator.SetBool(IsRun, isRun);

            if (_isRunAnimationPlaying == isRun)
                return;

            _isRunAnimationPlaying = isRun;
            _animator.CrossFade(isRun ? RunState : IdleState, 0.1f);
        }
    }
}
