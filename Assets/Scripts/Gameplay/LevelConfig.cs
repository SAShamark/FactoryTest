using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "ScriptableObjects/Gameplay/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Progression")]
        [SerializeField, Min(1f)] private float _targetDistance = 300f;

        [Header("Start Sequence")]
        [SerializeField, Min(0f)] private float _startRampDuration = 1.25f;
        [SerializeField] private AnimationCurve _startRampCurve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField, Min(0f)] private float _shootingDelay = 3f;

        [Header("Finish Sequence")]
        [SerializeField, Min(0f)] private float _noSpawnZoneDistance = 10f;
        [SerializeField, Min(0f)] private float _alignmentStartDistance = 30f;
        [SerializeField, Min(0.01f)] private float _alignmentDistance = 12f;
        [SerializeField, Min(0f)] private float _gateOpenDistance = 22f;
        [SerializeField, Min(0f)] private float _cameraFreezeDistance = 18f;
        [SerializeField, Min(0f)] private float _victoryPresentationDuration = 2f;
        [SerializeField, Min(0f)] private float _victoryExitViewportMargin = 0.15f;
        [SerializeField, Min(0f)] private float _victoryMaximumDriveDuration = 5f;

        public float TargetDistance => _targetDistance;
        public float StartRampDuration => _startRampDuration;
        public AnimationCurve StartRampCurve => _startRampCurve;
        public float ShootingDelay => _shootingDelay;
        public float NoSpawnZoneDistance => _noSpawnZoneDistance;
        public float AlignmentStartDistance => _alignmentStartDistance;
        public float AlignmentDistance => _alignmentDistance;
        public float GateOpenDistance => _gateOpenDistance;
        public float CameraFreezeDistance => _cameraFreezeDistance;
        public float VictoryPresentationDuration => _victoryPresentationDuration;
        public float VictoryExitViewportMargin => _victoryExitViewportMargin;
        public float VictoryMaximumDriveDuration => _victoryMaximumDriveDuration;
    }
}
