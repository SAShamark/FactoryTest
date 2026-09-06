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

        public float TravelledDistance => _movementLogic.Distance;

        private void Start()
        {
            InitializeUnit();
            _turretShooter.Initialize();
        }

        private void Update()
        {
            _movementLogic.Tick(transform, Time.deltaTime);

            if (TryGetAimPoint(out Vector3 aimPoint))
                _turretAimLogic.SetTarget(transform, aimPoint);

            _turretAimLogic.Tick(transform, _turret, Time.deltaTime);
        }

        private void LateUpdate()
        {
            _turretShooter.LateUpdate();
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
            MarkAsDead();
        }
    }
}
