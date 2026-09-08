using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Gameplay.Entities.BaseUnit
{
    public class FloatingTextControl : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private SpriteRenderer _icon;

        [Header("Damage")]
        [SerializeField] private Color _damageColor = Color.white;
        [SerializeField, Min(0.01f)] private float _damageScale = 0.8f;

        [Header("Reward")]
        [SerializeField] private Color _rewardColor = new(1f, 0.78f, 0.08f, 1f);
        [SerializeField, Min(0.01f)] private float _rewardScale = 1.15f;

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float _duration = 0.9f;
        [SerializeField, Min(0f)] private float _riseDistance = 1.4f;
        [SerializeField, Min(0f)] private float _horizontalDrift = 0.2f;
        [SerializeField, Range(0.01f, 1f)] private float _fadeInDuration = 0.1f;
        [SerializeField, Range(0.01f, 1f)] private float _fadeOutDuration = 0.25f;

        private Camera _camera;
        private Sequence _sequence;

        public void SetAsTemplate()
        {
            gameObject.SetActive(false);
        }

        public void ShowDamage(float damage, Vector3 worldPosition)
        {
            int displayedDamage = Mathf.Max(1, Mathf.RoundToInt(damage));
            Spawn(displayedDamage.ToString(), worldPosition, false);
        }

        public void ShowReward(Vector3 worldPosition)
        {
            Spawn("+1", worldPosition, true);
        }

        private void Spawn(string value, Vector3 worldPosition, bool isReward)
        {
            FloatingTextControl instance = Instantiate(this, worldPosition, Quaternion.identity);
            instance.gameObject.SetActive(true);
            instance.Play(value, isReward);
        }

        private void Play(string value, bool isReward)
        {
            _camera = Camera.main;
            _text.text = value;
            _text.color = isReward ? _rewardColor : _damageColor;
            _icon.gameObject.SetActive(isReward);

            float displayScale = isReward ? _rewardScale : _damageScale;
            Vector3 targetScale = Vector3.one * displayScale;
            transform.localScale = targetScale * 0.55f;
            SetAlpha(0f);
            FaceCamera();

            Vector3 endPosition = transform.position + Vector3.up * _riseDistance;
            if (_camera != null)
                endPosition += _camera.transform.right * Random.Range(-_horizontalDrift, _horizontalDrift);

            float fadeOutStart = Mathf.Max(_fadeInDuration, _duration - _fadeOutDuration);
            _sequence = DOTween.Sequence();
            _sequence.Join(transform.DOMove(endPosition, _duration).SetEase(Ease.OutCubic));
            _sequence.Join(transform.DOScale(targetScale, Mathf.Min(0.22f, _duration * 0.35f))
                .SetEase(Ease.OutBack));
            _sequence.Join(DOTween.To(SetAlpha, 0f, 1f, Mathf.Min(_fadeInDuration, _duration)));
            _sequence.Insert(fadeOutStart,
                DOTween.To(SetAlpha, 1f, 0f, Mathf.Min(_fadeOutDuration, _duration - fadeOutStart)));
            _sequence.Insert(fadeOutStart,
                transform.DOScale(targetScale * 0.8f, Mathf.Min(_fadeOutDuration, _duration - fadeOutStart))
                    .SetEase(Ease.InQuad));
            _sequence.SetLink(gameObject).OnComplete(() => Destroy(gameObject));
        }

        private void LateUpdate()
        {
            FaceCamera();
        }

        private void FaceCamera()
        {
            if (_camera != null)
                transform.rotation = _camera.transform.rotation;
        }

        private void SetAlpha(float alpha)
        {
            _text.alpha = alpha;

            Color iconColor = _icon.color;
            iconColor.a = alpha;
            _icon.color = iconColor;
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
        }
    }
}
