using System;
using Unity.Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    [Serializable]
    public class CameraShake 
    {
        private const int ImpulseChannel = 1;
        private const float DamageDownwardBias = -0.25f;
        private const float DamageRandomness = 0.2f;
        private const float DeathHorizontalSpread = 0.35f;

        [Header("Damage")]
        [SerializeField, Min(0f)] private float _damageStrength = 0.7f;
        [SerializeField, Min(0.01f)] private float _damageDuration = 0.28f;

        [Header("Death")]
        [SerializeField, Min(0f)] private float _deathStrength = 1f;
        [SerializeField, Min(0.01f)] private float _deathDuration = 0.3f;

        private CinemachineImpulseDefinition _damageImpulse;
        private CinemachineImpulseDefinition _deathImpulse;

        public void Initialize(params CinemachineCamera[] listeningCameras)
        {
            CinemachineImpulseManager.Instance.IgnoreTimeScale = true;

            foreach (CinemachineCamera listeningCamera in listeningCameras)
                ConfigureListener(listeningCamera);

            _damageImpulse = CreateImpulse(
                CinemachineImpulseDefinition.ImpulseShapes.Recoil,
                _damageDuration);
            _deathImpulse = CreateImpulse(
                CinemachineImpulseDefinition.ImpulseShapes.Explosion,
                _deathDuration);
        }

        public void PlayDamage(Vector3 hitPosition, Vector3 targetPosition)
        {
            Vector3 direction = GetImpactDirection(hitPosition, targetPosition);
            _damageImpulse.CreateEvent(hitPosition, direction * _damageStrength);
        }

        public void PlayDeath(Vector3 position)
        {
            Vector3 direction = new Vector3(
                Random.Range(-DeathHorizontalSpread, DeathHorizontalSpread),
                -1f,
                Random.Range(-DeathHorizontalSpread, DeathHorizontalSpread)).normalized;

            _deathImpulse.CreateEvent(position, direction * _deathStrength);
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

        private static void ConfigureListener(CinemachineCamera listeningCamera)
        {
            if (listeningCamera == null)
                return;

            CinemachineImpulseListener listener = listeningCamera.GetComponent<CinemachineImpulseListener>();
            if (listener == null)
                listener = listeningCamera.gameObject.AddComponent<CinemachineImpulseListener>();

            listener.ApplyAfter = CinemachineCore.Stage.Noise;
            listener.ChannelMask = ImpulseChannel;
            listener.Gain = 1f;
            listener.UseCameraSpace = true;
            listener.SignalCombinationMode = CinemachineImpulseListener.SignalCombinationModes.Additive;
        }

        private static Vector3 GetImpactDirection(Vector3 hitPosition, Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - hitPosition;
            direction.y = DamageDownwardBias;

            if (direction.sqrMagnitude < 0.001f)
                direction = Vector3.down;

            direction += Random.insideUnitSphere * DamageRandomness;
            return direction.normalized;
        }
    }
}
