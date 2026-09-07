using Gameplay.Entities.BaseUnit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Entities.Character
{
    public class CharacterControl : BaseUnitControl
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _turret;
        [SerializeField] private MovementLogic _movementLogic;
        [SerializeField] private TurretAimLogic _turretAimLogic;
        [SerializeField] private TurretShooter _turretShooter;
        [SerializeField] private CharacterEffects _characterEffects;

        protected override HitFeedback HitFeedback => _characterEffects.HitFeedback;

        private bool _isMoving;
        private bool _isShooting;

        public float TravelledDistance => _movementLogic.Distance;

        private void Start()
        {
            _turretShooter.Initialize();
        }

        private void Update()
        {
            if (_isMoving)
                _movementLogic.Tick(transform, Time.deltaTime);

            if (TryGetAimPoint(out Vector3 aimPoint))
                _turretAimLogic.SetTarget(transform, aimPoint);

            _turretAimLogic.Tick(transform, _turret, Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (_isShooting)
                _turretShooter.LateUpdate();
        }

        public void StartMoving()
        {
            _isMoving = true;
            InitializeUnit();
        }

        public void StartShooting()
        {
            _isShooting = true;
        }

        public void StopGameplay()
        {
            _isMoving = false;
            _isShooting = false;
        }

        public void StopShooting()
        {
            _isShooting = false;
        }

        public void BeginFinishAlignment(float targetX, float alignmentDistance)
        {
            _movementLogic.BeginFinishAlignment(transform, targetX, alignmentDistance);
        }

        public override void PlayHitFeedback(Vector3 hitPosition)
        {
            base.PlayHitFeedback(hitPosition);
            _characterEffects.PlayDamageShake(hitPosition);
        }

        private bool TryGetAimPoint(out Vector3 aimPoint)
        {
            aimPoint = default;

            Pointer pointer = Pointer.current;
            if (pointer == null)
                return false;

            if (pointer is Touchscreen && !pointer.press.isPressed)
                return false;

            Ray ray = _camera.ScreenPointToRay(pointer.position.ReadValue());
            Plane aimPlane = new Plane(Vector3.up, _turret.position);
            if (!aimPlane.Raycast(ray, out float distance))
                return false;

            aimPoint = ray.GetPoint(distance);
            return true;
        }

        protected override void Die()
        {
            StopGameplay();
            MarkAsDead();
            _characterEffects.Die();
        }
    }
}
