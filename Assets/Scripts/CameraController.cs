using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    private const int ImpulseChannel = 1;

    [SerializeField] private CinemachineCamera _gameplayCamera;
    [SerializeField] private CinemachineCamera _menuCamera;
    [SerializeField] private Transform _following;

    [Header("Shake")]
    [SerializeField, Min(0f)] private float _damageShakeStrength = 0.45f;
    [SerializeField, Min(0.01f)] private float _damageShakeDuration = 0.28f;
    [SerializeField, Min(0f)] private float _deathShakeStrength = 1f;
    [SerializeField, Min(0.01f)] private float _deathShakeDuration = 1.1f;

    private CinemachineCamera _activeCamera;
    private CinemachineImpulseDefinition _damageImpulse;
    private CinemachineImpulseDefinition _deathImpulse;
    private Transform _frozenFollowTarget;

    private void Awake()
    {
        Follow(_following);
        _activeCamera = GetActiveCamera();
        InitializeShake();
    }

    public void Follow(Transform following)
    {
        _following = following;
        SetGameplayCameraTarget(following);
    }

    public void ActivateCutsceneCamera()
    {
        SwitchCamera(_menuCamera);
    }

    public void ActivateGameplayCamera()
    {
        SwitchCamera(_gameplayCamera);
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

    public void PlayDamageShake(Vector3 hitPosition)
    {
        Vector3 direction = GetImpactDirection(hitPosition);
        _damageImpulse.CreateEvent(hitPosition, direction * _damageShakeStrength);
    }

    public void PlayDeathShake(Vector3 position)
    {
        Vector3 direction = new Vector3(
            Random.Range(-0.35f, 0.35f),
            -1f,
            Random.Range(-0.35f, 0.35f)).normalized;

        _deathImpulse.CreateEvent(position, direction * _deathShakeStrength);
    }

    private void SwitchCamera(CinemachineCamera cameraToActivate)
    {
        if (cameraToActivate == null)
            return;

        if (_activeCamera != null && _activeCamera != cameraToActivate)
            _activeCamera.gameObject.SetActive(false);

        cameraToActivate.gameObject.SetActive(true);
        _activeCamera = cameraToActivate;
    }

    private CinemachineCamera GetActiveCamera()
    {
        if (_gameplayCamera != null && _gameplayCamera.gameObject.activeSelf)
            return _gameplayCamera;

        if (_menuCamera != null && _menuCamera.gameObject.activeSelf)
            return _menuCamera;

        return null;
    }

    private void SetGameplayCameraTarget(Transform targetTransform)
    {
        CameraTarget target = _gameplayCamera.Target;
        target.TrackingTarget = targetTransform;
        target.LookAtTarget = targetTransform;
        target.CustomLookAtTarget = true;
        _gameplayCamera.Target = target;
    }

    private void InitializeShake()
    {
        CinemachineImpulseManager.Instance.IgnoreTimeScale = true;

        ConfigureListener(_gameplayCamera);
        ConfigureListener(_menuCamera);

        _damageImpulse = CreateImpulse(
            CinemachineImpulseDefinition.ImpulseShapes.Recoil,
            _damageShakeDuration);
        _deathImpulse = CreateImpulse(
            CinemachineImpulseDefinition.ImpulseShapes.Explosion,
            _deathShakeDuration);
    }

    private static CinemachineImpulseDefinition CreateImpulse(
        CinemachineImpulseDefinition.ImpulseShapes shape,
        float duration)
    {
        return new CinemachineImpulseDefinition
        {
            ImpulseChannel = ImpulseChannel,
            ImpulseShape = shape,
            ImpulseDuration = duration,
            ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform
        };
    }

    private static void ConfigureListener(CinemachineCamera camera)
    {
        if (camera == null)
            return;

        CinemachineImpulseListener listener = camera.GetComponent<CinemachineImpulseListener>();
        if (listener == null)
            listener = camera.gameObject.AddComponent<CinemachineImpulseListener>();

        listener.ApplyAfter = CinemachineCore.Stage.Noise;
        listener.ChannelMask = ImpulseChannel;
        listener.Gain = 1f;
        listener.UseCameraSpace = true;
        listener.SignalCombinationMode = CinemachineImpulseListener.SignalCombinationModes.Additive;
    }

    private Vector3 GetImpactDirection(Vector3 hitPosition)
    {
        Vector3 targetPosition = _following != null ? _following.position : transform.position;
        Vector3 direction = targetPosition - hitPosition;
        direction.y = -0.25f;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector3.down;

        direction += Random.insideUnitSphere * 0.2f;
        return direction.normalized;
    }
}
