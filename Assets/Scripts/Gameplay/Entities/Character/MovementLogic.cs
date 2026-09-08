using System;
using UnityEngine;

namespace Gameplay.Entities.Character
{
    [Serializable]
    public class MovementLogic
    {
        [SerializeField] private float _speed = 8f;
        [SerializeField] private float _swayAmplitude = 3f;
        [SerializeField] private float _swayFrequency = 0.04f;
        [SerializeField, Min(0f)] private float _rotationResponsiveness = 18f;

        private float _distance;
        private float _lateral;
        private bool _isFinishAlignmentActive;
        private bool _isFinishAligned;
        private float _alignmentStartDistance;
        private float _alignmentDistance;
        private float _alignmentTargetX;

        public float Distance => _distance;

        public void Tick(Transform body, float deltaTime)
        {
            _distance += _speed * deltaTime;

            float targetX = GetTargetX();

            float lateralDelta = targetX - body.position.x;
            float forwardDelta = _speed * deltaTime;
            body.position += new Vector3(lateralDelta, 0f, forwardDelta);

            float yaw = Mathf.Atan2(lateralDelta, forwardDelta) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, yaw, 0f);
            float rotationBlend = 1f - Mathf.Exp(-_rotationResponsiveness * deltaTime);
            body.rotation = Quaternion.Slerp(body.rotation, targetRotation, rotationBlend);
        }

        public void BeginFinishAlignment(float targetX, float alignmentDistance)
        {
            if (_isFinishAlignmentActive || _isFinishAligned)
                return;

            _isFinishAlignmentActive = true;
            _alignmentStartDistance = _distance;
            _alignmentDistance = Mathf.Max(0.01f, alignmentDistance);
            _alignmentTargetX = targetX;
        }

        private float GetTargetX()
        {
            if (_isFinishAligned)
                return _alignmentTargetX;

            if (!_isFinishAlignmentActive)
            {
                _lateral = SampleLateral();
                return _lateral;
            }

            float progress = Mathf.Clamp01(
                (_distance - _alignmentStartDistance) / _alignmentDistance);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            float targetX = Mathf.Lerp(SampleLateral(), _alignmentTargetX, easedProgress);

            if (progress >= 1f)
            {
                _isFinishAlignmentActive = false;
                _isFinishAligned = true;
                targetX = _alignmentTargetX;
            }

            return targetX;
        }

        private float SampleLateral()
        {
            float noiseCoordinate = _distance * _swayFrequency;
            float noise = Mathf.PerlinNoise(noiseCoordinate, 0f) * 2f - 1f;

            float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(noiseCoordinate));

            return noise * _swayAmplitude * fadeIn;
        }
    }
}
