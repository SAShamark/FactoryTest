using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.CameraLogic
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _gameplayCamera;
        [SerializeField] private CinemachineCamera _menuCamera;
        [SerializeField] private CinemachineBrain _brain;
        [SerializeField] private int _livePriority = 10;
        [SerializeField] private int _standbyPriority;
        [SerializeField] private CameraShake _cameraShake;

        private Transform _following;
        private Transform _frozenFollowTarget;

        public Camera OutputCamera => _brain.OutputCamera;

        private void Awake()
        {
            _cameraShake.Initialize(_gameplayCamera, _menuCamera);
            SwitchCamera(_menuCamera);
        }

        public void SetFollowTarget(Transform following)
        {
            _following = following;
            SetGameplayCameraTarget(following);
        }

        public void ActivateGameplayCamera()
        {
            SwitchCamera(_gameplayCamera);
        }

        public void ActivateGameplayCameraImmediately()
        {
            ActivateGameplayCamera();
            _brain.ResetState();
        }

        public void FreezeGameplayCamera()
        {
            if (_frozenFollowTarget == null)
            {
                GameObject freezeTarget = new GameObject("Camera Freeze Target");
                _frozenFollowTarget = freezeTarget.transform;
            }

            _frozenFollowTarget.SetPositionAndRotation(_following.position, _following.rotation);
            SetGameplayCameraTarget(_frozenFollowTarget);
        }

        public bool HasFollowingTargetExitedViewport(float margin)
        {
            Vector3 viewportPosition = OutputCamera.WorldToViewportPoint(_following.position);
            return viewportPosition.z <= 0f || viewportPosition.y > 1f + Mathf.Max(0f, margin);
        }

        public void PlayDamageShake(Vector3 hitPosition)
        {
            _cameraShake.PlayDamage(hitPosition, _following.position);
        }

        public void PlayDeathShake(Vector3 position)
        {
            _cameraShake.PlayDeath(position);
        }

        private void SwitchCamera(CinemachineCamera cameraToActivate)
        {
            ApplyPriority(_gameplayCamera, cameraToActivate == _gameplayCamera);
            ApplyPriority(_menuCamera, cameraToActivate == _menuCamera);
        }

        private void ApplyPriority(CinemachineCamera cinemachineCamera, bool isLive)
        {
            cinemachineCamera.gameObject.SetActive(true);
            cinemachineCamera.Priority = isLive ? _livePriority : _standbyPriority;
        }

        private void SetGameplayCameraTarget(Transform targetTransform)
        {
            CameraTarget target = _gameplayCamera.Target;
            target.TrackingTarget = targetTransform;
            target.LookAtTarget = targetTransform;
            target.CustomLookAtTarget = true;
            _gameplayCamera.Target = target;
        }
    }
}
