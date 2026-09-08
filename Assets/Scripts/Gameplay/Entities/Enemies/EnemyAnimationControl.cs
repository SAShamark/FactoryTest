using UnityEngine;

namespace Gameplay.Entities.Enemies
{
    public class EnemyAnimationControl : MonoBehaviour
    {
        private const float CrossFadeDuration = 0.1f;

        private static readonly int IsRun = Animator.StringToHash(nameof(IsRun));
        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int RunState = Animator.StringToHash("Run");

        [SerializeField] private Animator _animator;

        private bool _isRunPlaying;

        public void SetEnabled(bool isEnabled)
        {
            if (_animator == null)
                return;

            _animator.enabled = isEnabled;
        }

        public void SetRun(bool isRun)
        {
            if (_animator == null)
                return;

            _animator.SetBool(IsRun, isRun);

            if (_isRunPlaying == isRun)
                return;

            _isRunPlaying = isRun;
            _animator.CrossFade(isRun ? RunState : IdleState, CrossFadeDuration);
        }
    }
}
