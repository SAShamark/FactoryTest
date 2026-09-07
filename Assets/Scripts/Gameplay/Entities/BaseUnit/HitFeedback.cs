using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gameplay.Entities.BaseUnit
{
    [Serializable]
    public class HitFeedback
    {
        private readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Transform _hitEffect;

        [SerializeField, ColorUsage(true, true)]
        private Color _hitColor = new(2f, 0.18f, 0f, 1f);

        [SerializeField] private float _flashInDuration = 0.05f;
        [SerializeField] private float _flashOutDuration = 0.14f;
        [SerializeField] private float _pulseScaleMultiplier = 1.04f;
        [SerializeField] private float _pulseInDuration = 0.05f;
        [SerializeField] private float _pulseOutDuration = 0.08f;

        private Material[] _materials;
        private Color[] _defaultColors;
        private int[] _colorPropertyIds;
        private ParticleSystem _hitParticleSystem;
        private Tween _flashTween;
        private Tween _pulseTween;
        private Tween _darkenTween;
        private float _flashAmount;
        private Vector3 _defaultVisualScale;

        public void Initialize()
        {
            if (_visualRoot == null)
                return;

            _defaultVisualScale = _visualRoot.localScale;

            Renderer[] renderers = _visualRoot.GetComponentsInChildren<Renderer>(true);
            List<Material> materials = new();

            foreach (var renderer1 in renderers)
            {
                Material[] rendererMaterials = renderer1.materials;

                foreach (var material in rendererMaterials)
                {
                    materials.Add(material);
                }
            }

            _materials = materials.ToArray();
            _defaultColors = new Color[_materials.Length];
            _colorPropertyIds = new int[_materials.Length];

            for (int i = 0; i < _materials.Length; i++)
            {
                _colorPropertyIds[i] = _materials[i].HasProperty(BaseColorId) ? BaseColorId : ColorId;
                _defaultColors[i] = _materials[i].GetColor(_colorPropertyIds[i]);
            }

            if (_hitEffect != null)
            {
                _hitParticleSystem = _hitEffect.GetComponent<ParticleSystem>();
                _hitParticleSystem?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public void Play(Vector3 hitPosition)
        {
            if (_visualRoot == null || _materials == null)
                return;

            if (_hitEffect != null && _hitParticleSystem != null)
            {
                _hitEffect.position = hitPosition;
                _hitParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                _hitParticleSystem.Play(true);
            }

            _flashTween?.Kill();
            _flashTween = DOTween.Sequence()
                .Append(DOTween.To(() => _flashAmount, SetFlashAmount, 1f, _flashInDuration)
                    .SetEase(Ease.OutQuad))
                .Append(DOTween.To(() => _flashAmount, SetFlashAmount, 0f, _flashOutDuration)
                    .SetEase(Ease.InQuad)).OnComplete(() => _flashTween = null).SetLink(_visualRoot.gameObject);

            _pulseTween?.Kill();
            _pulseTween = DOTween.Sequence()
                .Append(_visualRoot.DOScale(
                        _defaultVisualScale * _pulseScaleMultiplier,
                        _pulseInDuration)
                    .SetEase(Ease.OutQuad))
                .Append(_visualRoot.DOScale(_defaultVisualScale, _pulseOutDuration)
                    .SetEase(Ease.InOutQuad)).OnComplete(() => _pulseTween = null).SetLink(_visualRoot.gameObject);
        }

        public void Reset()
        {
            if (_visualRoot == null || _materials == null)
                return;

            _flashTween?.Kill();
            _pulseTween?.Kill();
            _darkenTween?.Kill();

            _flashAmount = 0f;
            _visualRoot.localScale = _defaultVisualScale;
            ApplyColors();
        }

        private void SetFlashAmount(float amount)
        {
            _flashAmount = amount;
            ApplyColors();
        }

        private void ApplyColors()
        {
            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].SetColor(_colorPropertyIds[i], Color.Lerp(_defaultColors[i], _hitColor, _flashAmount));
            }
        }

        public void Darken(float brightness, float duration)
        {
            if (_visualRoot == null || _materials == null)
                return;

            _flashTween?.Kill();
            _darkenTween?.Kill();

            float clampedBrightness = Mathf.Clamp01(brightness);
            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < _materials.Length; i++)
            {
                Material material = _materials[i];
                int propertyId = _colorPropertyIds[i];
                Color targetColor = _defaultColors[i] * clampedBrightness;
                targetColor.a = _defaultColors[i].a;

                sequence.Join(DOTween.To(
                    () => material.GetColor(propertyId),
                    color => material.SetColor(propertyId, color),
                    targetColor,
                    duration));
            }

            _darkenTween = sequence
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .SetLink(_visualRoot.gameObject)
                .OnComplete(() => _darkenTween = null);
        }

        public void Dispose()
        {
            _flashTween?.Kill();
            _pulseTween?.Kill();
            _darkenTween?.Kill();

            if (_materials == null)
                return;

            foreach (var material in _materials)
            {
                Object.Destroy(material);
            }
        }
    }
}
