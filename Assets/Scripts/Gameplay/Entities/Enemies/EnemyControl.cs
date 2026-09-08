using System;
using DG.Tweening;
using Gameplay.Entities.BaseUnit;
using Services.ObjectPool;
using UnityEngine;
using Random = UnityEngine.Random;

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
        [SerializeField] private ParticleSystem _deathEffect;
        [SerializeField, Min(0f)] private float _deathJumpDistance = 0.7f;
        [SerializeField, Min(0f)] private float _deathJumpHeight = 0.8f;
        [SerializeField, Min(0.01f)] private float _deathDuration = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _deathEndScale = 0.15f;

        protected override HitFeedback HitFeedback => _hitFeedback;

        private Transform _target;
        private BaseUnitControl _targetUnit;
        private bool _isChasing;
        private bool _hasDealtContactDamage;
        private bool _isRunAnimationPlaying;
        private float _activationDistanceSqr;
        private float _contactDamage;
        private float _moveSpeed;
        private float _rotationSpeed;
        private float _roadHalfWidth;
        private float _wanderSpeed;
        private float _wanderPauseDuration;
        private float _wanderMoveDuration;
        private float _wanderTimer;
        private bool _isWandering;
        private Vector3 _wanderDestination;
        private bool _isSurrounding;
        private Vector3 _surroundOffset;
        private Collider[] _colliders;
        private Vector3 _defaultScale;
        private Tween _deathTween;
        private FloatingTextControl _floatingText;

        public event Action KilledByPlayer;

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
            _hasDealtContactDamage = false;
            _isSurrounding = false;
            SetCollidersEnabled(true);

            if (_animator != null)
                _animator.enabled = true;

            SetRunAnimation(false);
        }

        public void Spawn(Vector3 position, Quaternion rotation, Transform target, float activationDistance,
            float contactDamage, float moveSpeed, float rotationSpeed, float roadHalfWidth,
            float wanderSpeed, float wanderPauseDuration, float wanderMoveDuration,
            FloatingTextControl floatingText)
        {
            Spawn(position, rotation);
            _target = target;
            _targetUnit = target.GetComponentInParent<BaseUnitControl>();
            _activationDistanceSqr = activationDistance * activationDistance;
            _contactDamage = contactDamage;
            _moveSpeed = moveSpeed;
            _rotationSpeed = rotationSpeed;
            _roadHalfWidth = roadHalfWidth;
            _wanderSpeed = wanderSpeed;
            _wanderPauseDuration = wanderPauseDuration;
            _wanderMoveDuration = wanderMoveDuration;
            _floatingText = floatingText;
            _isWandering = false;
            _wanderTimer = _wanderPauseDuration;
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

            if (!_isChasing)
            {
                if (toTarget.sqrMagnitude > _activationDistanceSqr)
                {
                    UpdateIdleWander();
                    return;
                }

                _isChasing = true;
                _isWandering = false;
            }

            MoveToTarget(toTarget);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryDamageTarget(other);
        }

        private void OnTriggerStay(Collider other)
        {
            // The car may become alive while already overlapping this enemy.
            TryDamageTarget(other);
        }

        private void TryDamageTarget(Collider other)
        {
            if (!IsAlive || _hasDealtContactDamage || _isSurrounding
                || _targetUnit == null || !_targetUnit.IsAlive)
                return;

            if (other.GetComponentInParent<BaseUnitControl>() != _targetUnit)
                return;

            // Reserve the hit before health/death callbacks or another collider can trigger it again.
            _hasDealtContactDamage = true;
            BaseUnitControl targetUnit = _targetUnit;
            targetUnit.PlayHitFeedback(transform.position);
            targetUnit.ApplyDamage(_contactDamage);
            Die();
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
            Move(toTarget, _moveSpeed);
        }

        private void UpdateIdleWander()
        {
            _wanderTimer -= Time.deltaTime;

            if (_isWandering)
            {
                Vector3 toDestination = _wanderDestination - transform.position;
                toDestination.y = 0f;

                if (_wanderTimer <= 0f || toDestination.sqrMagnitude <= 0.01f)
                {
                    _isWandering = false;
                    _wanderTimer = _wanderPauseDuration;
                    SetRunAnimation(false);
                    return;
                }

                Move(toDestination, _wanderSpeed);
                return;
            }

            if (_wanderTimer > 0f)
                return;

            BeginIdleWander();
        }

        private void BeginIdleWander()
        {
            const float colliderPadding = 0.5f;
            float minX = -Mathf.Max(0f, _roadHalfWidth - colliderPadding);
            float maxX = Mathf.Max(0f, _roadHalfWidth - colliderPadding);
            float direction = Random.value < 0.5f ? -1f : 1f;
            float distance = _wanderSpeed * _wanderMoveDuration;
            float targetX = Mathf.Clamp(transform.position.x + direction * distance, minX, maxX);

            if (Mathf.Abs(targetX - transform.position.x) < 0.1f)
                targetX = Mathf.Clamp(transform.position.x - direction * distance, minX, maxX);

            _wanderDestination = new Vector3(targetX, transform.position.y, transform.position.z);
            _wanderTimer = _wanderMoveDuration;
            _isWandering = true;
        }

        private void Move(Vector3 toTarget, float speed)
        {
            if (toTarget.sqrMagnitude <= 0.01f)
            {
                SetRunAnimation(false);
                return;
            }

            SetRunAnimation(true);
            Vector3 direction = toTarget.normalized;
            transform.position += direction * (speed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime);
        }

        public void Hit(float damage, Vector3 hitPosition)
        {
            if (!IsAlive || damage <= 0f)
                return;

            float appliedDamage = Mathf.Min(damage, Health.CurrentHealth);
            bool isLethal = appliedDamage >= Health.CurrentHealth;
            PlayHitFeedback(hitPosition);
            ApplyDamage(damage);

            Vector3 textPosition = transform.position + Vector3.up * 2.2f;
            if (isLethal)
            {
                KilledByPlayer?.Invoke();
                _floatingText?.ShowReward(textPosition);
            }
            else
            {
                _floatingText?.ShowDamage(appliedDamage, textPosition);
            }
        }

        public void Despawn()
        {
            _target = null;
            _targetUnit = null;
            _isChasing = false;
            _isWandering = false;
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
            _isWandering = false;
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
            _deathEffect.Play();
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
