using UnityEngine;

namespace Gameplay.Entities
{
    public class GroundControl : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _segmentPrefab;
        [SerializeField] private int _segmentCount = 4;

        private Transform[] _segments;
        private float _segmentLength;
        private int _rearIndex;

        public void Initialize()
        {
            _segments = new Transform[_segmentCount];
            for (int i = 0; i < _segmentCount; i++)
                _segments[i] = Instantiate(_segmentPrefab, transform);

            _segmentLength = MeasureLength(_segments[0]);
            _rearIndex = 0;

            float rearZ = _target.position.z - _segmentLength;
            for (int i = 0; i < _segmentCount; i++)
                _segments[i].position = SegmentPosition(rearZ + i * _segmentLength);
        }

        private void Update()
        {
            if (_segments == null)
                return;

            Transform rearmost = _segments[_rearIndex];
            if (_target.position.z - rearmost.position.z < _segmentLength)
                return;

            rearmost.position += Vector3.forward * (_segmentCount * _segmentLength);
            _rearIndex = (_rearIndex + 1) % _segmentCount;
        }

        private Vector3 SegmentPosition(float z)
        {
            Vector3 origin = transform.position;
            return new Vector3(origin.x, origin.y, z);
        }

        private static float MeasureLength(Transform segment)
        {
            Renderer[] renderers = segment.GetComponentsInChildren<Renderer>();

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            return bounds.size.z;
        }
    }
}
