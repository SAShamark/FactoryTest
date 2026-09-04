using System;
using UnityEngine;

namespace Gameplay.Character
{
    [Serializable]
    public class MovementLogic
    {
        [SerializeField] private float _speed = 8f;
        [SerializeField] private float _swayAmplitude = 3f;
        [SerializeField] private float _swayFrequency = 0.04f;

        private float _distance;
        private float _lateral;

        public void Tick(Transform body, float deltaTime)
        {
            _distance += _speed * deltaTime;

            // Perlin noise never leaves 0..1, so the offset stays inside the amplitude
            // and the car cannot wander off the road no matter how long it drives.
            float previousLateral = _lateral;
            _lateral = (Mathf.PerlinNoise(_distance * _swayFrequency, 0f) * 2f - 1f) * _swayAmplitude;

            float lateralDelta = _lateral - previousLateral;
            float forwardDelta = _speed * deltaTime;
            body.position += new Vector3(lateralDelta, 0f, forwardDelta);

            // The body faces wherever it actually travels, so the yaw can never
            // disagree with the movement.
            float yaw = Mathf.Atan2(lateralDelta, forwardDelta) * Mathf.Rad2Deg;
            body.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }

    [Serializable]
    public class TurretAimLogic
    {
        [SerializeField] private float _rotationSpeed = 240f;
        [SerializeField] private float _maxAngle = 75f;

        private float _targetAngle;
        private float _angle;

        public float Angle => _angle;

        public void SetTarget(Transform body, Vector3 aimPoint)
        {
            Vector3 localPoint = body.InverseTransformPoint(aimPoint);
            float angle = Mathf.Atan2(localPoint.x, localPoint.z) * Mathf.Rad2Deg;
            _targetAngle = Mathf.Clamp(angle, -_maxAngle, _maxAngle);
        }

        public void Tick(Transform body, Transform turret, float deltaTime)
        {
            _angle = Mathf.MoveTowardsAngle(_angle, _targetAngle, _rotationSpeed * deltaTime);
            turret.rotation = body.rotation * Quaternion.Euler(0f, _angle, 0f);
        }
    }
}