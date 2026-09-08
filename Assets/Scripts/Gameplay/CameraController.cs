using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _gameplayCamera;
        [SerializeField] private CinemachineCamera _menuCamera;
        [SerializeField] private CinemachineBrain _brain;
        [SerializeField] private Transform _following;
        [SerializeField] private int _livePriority = 10;
        [SerializeField] private int _standbyPriority;
        [SerializeField] private CameraShake _cameraShake;

        private Transform _frozenFollowTarget;

        private void Awake()
        {
            Follow(_following);
            _cameraShake.Initialize(_gameplayCamera, _menuCamera);

            SwitchCamera(_menuCamera);
        }

        public void Follow(Transform following)
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
            if (_brain != null)
                _brain.ResetState();
        }

        public void FreezeGameplayCamera()
        {
            if (_gameplayCamera == null || _following == null)
                return;

            if (_frozenFollowTarget == null)
            {
                GameObject freezeTarget = new GameObject("Camera Freeze Target");
                _frozenFollowTarget = freezeTarget.transform;
            }

            _frozenFollowTarget.SetPositionAndRotation(_following.position, _following.rotation);
            SetGameplayCameraTarget(_frozenFollowTarget);
        }

        public Camera OutputCamera => _brain != null ? _brain.OutputCamera : Camera.main;

        public bool HasFollowingTargetExitedViewport(float margin)
        {
            if (_following == null)
                return false;

            Camera outputCamera = OutputCamera;
            if (outputCamera == null)
                return false;

            Vector3 viewportPosition = outputCamera.WorldToViewportPoint(_following.position);
            return viewportPosition.z <= 0f || viewportPosition.y > 1f + Mathf.Max(0f, margin);
        }

        public void PlayDamageShake(Vector3 hitPosition)
        {
            Vector3 targetPosition = _following != null ? _following.position : transform.position;
            _cameraShake.PlayDamage(hitPosition, targetPosition);
        }

        public void PlayDeathShake(Vector3 position)
        {
            _cameraShake.PlayDeath(position);
        }

        private void SwitchCamera(CinemachineCamera cameraToActivate)
        {
            if (cameraToActivate == null)
                return;

            ApplyPriority(_gameplayCamera, cameraToActivate == _gameplayCamera);
            ApplyPriority(_menuCamera, cameraToActivate == _menuCamera);
        }

        private void ApplyPriority(CinemachineCamera cinemachineCamera, bool isLive)
        {
            if (cinemachineCamera == null)
                return;

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
