using System;
using UnityEngine;

namespace Gameplay.Entities.Character
{
    [Serializable]
    public class TurretAimLogic
    {
        [SerializeField] private float _rotationSpeed = 240f;
        [SerializeField] private float _maxAngle = 75f;

        private float _targetAngle;

        public float Angle { get; private set; }

        public void Initialize(Transform body, Transform turret)
        {
            Quaternion localRotation = Quaternion.Inverse(body.rotation) * turret.rotation;
            Angle = Mathf.Clamp(Mathf.DeltaAngle(0f, localRotation.eulerAngles.y), -_maxAngle, _maxAngle);
            _targetAngle = Angle;
        }

        public void SetTarget(Transform body, Vector3 aimPoint)
        {
            Vector3 localPoint = body.InverseTransformPoint(aimPoint);
            float angle = Mathf.Atan2(localPoint.x, localPoint.z) * Mathf.Rad2Deg;
            _targetAngle = Mathf.Clamp(angle, -_maxAngle, _maxAngle);
        }

        public void Tick(Transform body, Transform turret, float deltaTime)
        {
            Angle = Mathf.MoveTowardsAngle(Angle, _targetAngle, _rotationSpeed * deltaTime);
            turret.rotation = body.rotation * Quaternion.Euler(0f, Angle, 0f);
        }
    }
}
