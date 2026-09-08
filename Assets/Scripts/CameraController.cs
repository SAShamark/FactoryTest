using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    private const int ImpulseChannel = 1;

    [SerializeField] private CinemachineCamera _gameplayCamera;
    [SerializeField] private CinemachineCamera _menuCamera;
    [SerializeField] private CinemachineBrain _brain;
    [SerializeField] private Transform _following;
    [SerializeField] private int _livePriority = 10;
    [SerializeField] private int _standbyPriority = 0;

    [Header("Shake")]
    [SerializeField, Min(0f)] private float _damageShakeStrength = 0.45f;
    [SerializeField, Min(0.01f)] private float _damageShakeDuration = 0.28f;
    [SerializeField, Min(0f)] private float _deathShakeStrength = 1f;
    [SerializeField, Min(0.01f)] private float _deathShakeDuration = 1.1f;

    private CinemachineImpulseDefinition _damageImpulse;
    private CinemachineImpulseDefinition _deathImpulse;
    private Transform _frozenFollowTarget;

    private void Awake()
    {
        Follow(_following);
        InitializeShake();

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

        // Timeline owns the cinematic blend. Clear the underlying gameplay blend,
        // which otherwise stays frozen while Time.timeScale is zero.
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

    public bool HasFollowingTargetExitedViewport(float margin)
    {
        if (_following == null)
            return false;

        Camera outputCamera = _brain != null ? _brain.OutputCamera : Camera.main;
        if (outputCamera == null)
            return false;

        Vector3 viewportPosition = outputCamera.WorldToViewportPoint(_following.position);
        return viewportPosition.z <= 0f || viewportPosition.y > 1f + Mathf.Max(0f, margin);
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
