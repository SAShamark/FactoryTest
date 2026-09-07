using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Entities
{
    public class EnvironmentControl : MonoBehaviour
    {
        [SerializeField] private Transform _start;
        [SerializeField] private Transform _end;
        [SerializeField] private GroundControl _groundControl;
        [SerializeField] private List<ParticleSystem> _victoryEffects;

        private GateControl _finishGate;

        public float FinishCenterX => _end != null ? _end.position.x : 0f;
        public float FinishZ => _end != null ? _end.position.z : 0f;

        public void Initialize(float levelDistance)
        {
            _end.position = _start.position + Vector3.forward * levelDistance;
            _finishGate = _end.GetComponent<GateControl>();
            _groundControl.Initialize();
        }

        public void OpenFinishGate()
        {
            _finishGate?.Open();
        }

        public void OpenFinishGateWithVictoryEffects()
        {
            OpenFinishGate();
            PlayVictoryEffects();
        }

        public void CloseFinishGate()
        {
            _finishGate?.Close();
        }

        public void PlayVictoryEffects()
        {
            foreach (ParticleSystem effect in _victoryEffects)
            {
                if (effect == null)
                    continue;

                effect.Play();
            }
        }
    }
}
