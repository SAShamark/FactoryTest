using DG.Tweening;
using UnityEngine;

namespace Gameplay.Entities
{
    public class GateControl : MonoBehaviour
    {
        [SerializeField] private Transform _door;
        [SerializeField, Min(0f)] private float _openOffset = 4.2f;
        [SerializeField, Min(0.01f)] private float _openDuration = 0.75f;
        [SerializeField, Min(0.01f)] private float _closeDuration = 0.65f;

        private Vector3 _closedLocalPosition;
        private Tween _doorTween;

        private void Awake()
        {
            if (_door != null)
                _closedLocalPosition = _door.localPosition;
        }

        public void Open()
        {
            MoveDoor(_closedLocalPosition + Vector3.right * _openOffset, _openDuration, Ease.OutCubic);
        }

        public void Close()
        {
            MoveDoor(_closedLocalPosition, _closeDuration, Ease.InOutCubic);
        }

        private void MoveDoor(Vector3 targetPosition, float duration, Ease ease)
        {
            if (_door == null)
                return;

            _doorTween?.Kill();
            _doorTween = _door.DOLocalMove(targetPosition, duration).SetEase(ease)
                .SetLink(gameObject).OnComplete(() => _doorTween = null);
        }

        private void OnDestroy()
        {
            _doorTween?.Kill();
        }
    }
}
