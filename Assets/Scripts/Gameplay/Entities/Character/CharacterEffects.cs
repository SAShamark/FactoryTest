using System.Collections.Generic;
using DG.Tweening;
using Gameplay.Entities.BaseUnit;
using UnityEngine;

namespace Gameplay.Entities.Character
{
    public class CharacterEffects : MonoBehaviour
    {
        [SerializeField] private HitFeedback _hitFeedback;
        [SerializeField] private List<GameObject> _lights;
        [SerializeField] private ParticleSystem _smokeParticleSystem;
        [SerializeField] private ParticleSystem _boomEffect;
        [SerializeField] private ParticleSystem _dieEffect;
        [SerializeField] private List<GameObject> _wheels;
        [SerializeField] private CameraController _cameraController;

        [Header("Death")]
        [SerializeField, Range(0f, 1f)] private float _deadBrightness = 0.12f;
        [SerializeField, Min(0f)] private float _darkenDuration = 0.45f;
        [SerializeField, Min(0f)] private float _deathSmokeDelay = 0.3f;
        [SerializeField, Min(0f)] private float _wheelFlyDistance = 1.35f;
        [SerializeField, Min(0f)] private float _wheelFlyHeight = 0.65f;
        [SerializeField, Min(0.01f)] private float _wheelFlyDuration = 0.75f;

        private bool _isDead;
        private Tween _smokeDelayTween;

        public HitFeedback HitFeedback => _hitFeedback;

        public void PlayDamageShake(Vector3 hitPosition)
        {
            _cameraController.PlayDamageShake(hitPosition);
        }

        public void ActivateCar()
        {
            foreach (var item in _lights)
            {
                item.SetActive(true);
            }

            PlayParticle(_smokeParticleSystem);
        }

        public void Die()
        {
            if (_isDead)
                return;

            _isDead = true;

            foreach (GameObject lightObject in _lights)
                lightObject.SetActive(false);

            _hitFeedback.Darken(_deadBrightness, _darkenDuration);
            _cameraController.PlayDeathShake(transform.position);
            PlayParticle(_boomEffect);
            ScatterWheels();

            _smokeDelayTween = DOVirtual.DelayedCall(
                    _deathSmokeDelay,
                    () => PlayParticle(_dieEffect),
                    true)
                .SetLink(gameObject)
                .OnComplete(() => _smokeDelayTween = null);
        }

        private void ScatterWheels()
        {
            for (int i = 0; i < _wheels.Count; i++)
            {
                GameObject wheelObject = _wheels[i];
                if (wheelObject == null)
                    continue;

                Transform wheel = wheelObject.transform;
                Vector3 localPosition = transform.InverseTransformPoint(wheel.position);
                float side = Mathf.Approximately(localPosition.x, 0f)
                    ? (i % 2 == 0 ? -1f : 1f)
                    : Mathf.Sign(localPosition.x);
                float forward = i % 2 == 0 ? -0.35f : 0.35f;

                Vector3 direction = (transform.right * side + transform.forward * forward).normalized;
                Vector3 targetPosition = wheel.position + direction * _wheelFlyDistance;

                wheel.SetParent(null, true);
                wheel.DOJump(targetPosition, _wheelFlyHeight, 1, _wheelFlyDuration)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(true)
                    .SetLink(wheelObject);
                wheel.DORotate(
                        new Vector3(Random.Range(220f, 420f),
                            Random.Range(-180f, 180f),
                            Random.Range(220f, 420f)), _wheelFlyDuration, RotateMode.FastBeyond360)
                    .SetRelative().SetEase(Ease.OutQuad).SetUpdate(true).SetLink(wheelObject);
            }
        }

        private static void PlayParticle(ParticleSystem effect)
        {
            if (effect == null)
                return;

            ParticleSystem.MainModule main = effect.main;
            main.useUnscaledTime = true;

            effect.gameObject.SetActive(true);
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            effect.Play(true);
        }

        private void OnDestroy()
        {
            _smokeDelayTween?.Kill();
        }
    }
}
