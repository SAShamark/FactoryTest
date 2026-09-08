using System;
using DG.Tweening;
using Gameplay.Entities.BaseUnit;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Entities.Enemies
{
    public class EnemyEffects : MonoBehaviour
    {
        private const float FloatingTextHeight = 2.2f;

        [SerializeField] private HitFeedback _hitFeedback;
        [SerializeField] private ParticleSystem _deathEffect;

        [Header("Death Feedback")]
        [SerializeField, Min(0f)] private float _deathJumpDistance = 0.7f;
        [SerializeField, Min(0f)] private float _deathJumpHeight = 0.8f;
        [SerializeField, Min(0.01f)] private float _deathDuration = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _deathEndScale = 0.15f;

        private FloatingTextService _floatingText;
        private Vector3 _defaultScale;
        private Tween _deathTween;

        public HitFeedback HitFeedback => _hitFeedback;

        private void Awake()
        {
            _defaultScale = transform.localScale;
        }

        public void SetFloatingText(FloatingTextService floatingText)
        {
            _floatingText = floatingText;
        }

        public void ResetVisuals()
        {
            _deathTween?.Kill();
            _deathTween = null;
            transform.localScale = _defaultScale;
        }

        public void ShowDamage(float damage)
        {
            _floatingText?.ShowDamage(damage, GetFloatingTextPosition());
        }

        public void ShowReward()
        {
            _floatingText?.ShowReward(GetFloatingTextPosition());
        }

        public void PlayDeath(Action onComplete)
        {
            _deathEffect.Play();

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
                    onComplete?.Invoke();
                });
        }

        private Vector3 GetFloatingTextPosition()
        {
            return transform.position + Vector3.up * FloatingTextHeight;
        }

        private void OnDestroy()
        {
            _deathTween?.Kill();
        }
    }
}
