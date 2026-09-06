using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Gameplay.Intro
{
    [RequireComponent(typeof(PlayableDirector))]
    public sealed class IntroSequence : MonoBehaviour
    {
        [SerializeField] private PlayableDirector _director;
        [SerializeField] private Animator _actor;

        private bool _hasStarted;
        public event Action Completed;

        public void Initialize()
        {
            _hasStarted = false;
            _director.playOnAwake = false;
            _director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            _director.extrapolationMode = DirectorWrapMode.None;
            _director.stopped -= HandleStopped;
            _director.stopped += HandleStopped;
            _actor.gameObject.SetActive(true);
            _actor.applyRootMotion = false;
            _actor.updateMode = AnimatorUpdateMode.UnscaledTime;
            _actor.SetBool("IsWalk", false);
            _actor.Play("Idle", 0, 0f);
        }

        public void Play()
        {
            if (_hasStarted)
                return;

            _hasStarted = true;
            _director.time = 0d;
            _director.Play();
        }

        private void HandleStopped(PlayableDirector director)
        {
            if (!_hasStarted)
                return;

            // Movement and visibility are authored on Timeline. Further intro steps
            // can subscribe here; completing this first section does not start driving.
            Completed?.Invoke();
        }

        private void OnDestroy()
        {
            if (_director != null)
                _director.stopped -= HandleStopped;
        }
    }
}
