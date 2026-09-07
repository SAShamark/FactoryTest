using DG.Tweening;
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
        [SerializeField] private HitFeedback _hitFeedback;

        [Header("Death Feedback")]
        [SerializeField] private GameObject _deathEffectPrefab;
        [SerializeField] private Vector3 _deathEffectOffset = new(0f, 0.9f, 0f);
        [SerializeField, Min(0f)] private float _deathJumpDistance = 0.7f;
        [SerializeField, Min(0f)] private float _deathJumpHeight = 0.8f;
        [SerializeField, Min(0.01f)] private float _deathDuration = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _deathEndScale = 0.15f;

        protected override HitFeedback HitFeedback => _hitFeedback;

        private Transform _target;
        private BaseUnitControl _targetUnit;
        private bool _isChasing;
        private bool _isRunAnimationPlaying;
        private float _activationDistanceSqr;
        private float _contactDistanceSqr;
        private float _contactDamage;
        private float _moveSpeed;
        private float _catchUpDistanceBehindTarget;
        private float _catchUpMoveSpeed;
        private float _rotationSpeed;
        private bool _isSurrounding;
        private Vector3 _surroundOffset;
        private Collider[] _colliders;
        private Vector3 _defaultScale;
        private Tween _deathTween;

        protected override void Awake()
        {
            base.Awake();
            _colliders = GetComponentsInChildren<Collider>(true);
            _defaultScale = transform.localScale;
        }

        public void Spawn(Vector3 position, Quaternion rotation)
        {
            _deathTween?.Kill();
            _deathTween = null;
            transform.localScale = _defaultScale;
            InitializeUnit();
            transform.SetPositionAndRotation(position, rotation);
            _target = null;
            _targetUnit = null;
            _isChasing = false;
            _isSurrounding = false;
            SetCollidersEnabled(true);

            if (_animator != null)
                _animator.enabled = true;

            SetRunAnimation(false);
        }

        public void Spawn(Vector3 position, Quaternion rotation, Transform target, float activationDistance,
            float contactDistance, float contactDamage, float moveSpeed, float catchUpDistanceBehindTarget,
            float catchUpMoveSpeed, float rotationSpeed)
        {
            Spawn(position, rotation);
            _target = target;
            _targetUnit = target.GetComponentInParent<BaseUnitControl>();
            _activationDistanceSqr = activationDistance * activationDistance;
            _contactDistanceSqr = contactDistance * contactDistance;
            _contactDamage = contactDamage;
            _moveSpeed = moveSpeed;
            _catchUpDistanceBehindTarget = catchUpDistanceBehindTarget;
            _catchUpMoveSpeed = catchUpMoveSpeed;
            _rotationSpeed = rotationSpeed;
        }

        private void Update()
        {
            if (_target == null)
                return;

            if (_isSurrounding)
            {
                MoveToSurroundPosition();
                return;
            }

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

        public void BeginSurrounding(Vector3 offset)
        {
            if (!IsAlive)
                return;

            _isSurrounding = true;
            _isChasing = true;
            _surroundOffset = offset;
        }

        private void MoveToSurroundPosition()
        {
            Vector3 destination = _target.position + _surroundOffset;
            Vector3 toDestination = destination - transform.position;
            toDestination.y = 0f;

            if (toDestination.sqrMagnitude > 0.04f)
            {
                MoveToTarget(toDestination);
                return;
            }

            SetRunAnimation(false);

            Vector3 lookDirection = _target.position - transform.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude <= 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime);
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
            float moveSpeed = _moveSpeed;
            if (_target != null && _target.position.z - transform.position.z > _catchUpDistanceBehindTarget)
                moveSpeed = Mathf.Max(moveSpeed, _catchUpMoveSpeed);

            transform.position += direction * (moveSpeed * Time.deltaTime);

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
            _isSurrounding = false;
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
            if (!IsAlive)
                return;

            MarkAsDead();
            _target = null;
            _targetUnit = null;
            _isChasing = false;
            _isSurrounding = false;
            SetCollidersEnabled(false);
            SetRunAnimation(false);

            if (_animator != null)
                _animator.enabled = false;

            PlayDeathEffect();
            PlayDeathAnimation();
        }

        private void PlayDeathEffect()
        {
            if (_deathEffectPrefab == null)
                return;

            Instantiate(
                _deathEffectPrefab,
                transform.TransformPoint(_deathEffectOffset),
                Quaternion.identity);
        }

        private void PlayDeathAnimation()
        {
            Vector3 sideDirection = Random.value < 0.5f ? -transform.right : transform.right;
            Vector3 targetPosition = transform.position + sideDirection * _deathJumpDistance;

            Sequence sequence = DOTween.Sequence();
            sequence.Join(transform.DOJump(
                    targetPosition,
                    _deathJumpHeight,
                    1,
                    _deathDuration)
                .SetEase(Ease.OutQuad));
            sequence.Insert(
                _deathDuration * 0.35f,
                transform.DOScale(_defaultScale * _deathEndScale, _deathDuration * 0.65f)
                    .SetEase(Ease.InBack));

            _deathTween = sequence
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    _deathTween = null;
                    Despawn();
                });
        }

        private void SetCollidersEnabled(bool isEnabled)
        {
            if (_colliders == null)
                return;

            foreach (Collider enemyCollider in _colliders)
                enemyCollider.enabled = isEnabled;
        }

        protected override void OnDestroy()
        {
            _deathTween?.Kill();
            base.OnDestroy();
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
