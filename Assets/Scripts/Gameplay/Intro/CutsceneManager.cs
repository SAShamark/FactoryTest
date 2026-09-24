using System;
using Gameplay.CameraLogic;
using UnityEngine;
using UnityEngine.Playables;

namespace Gameplay.Intro
{
    [Serializable]
    public class CutsceneManager : IDisposable
    {
        private enum State
        {
            Ready,
            Playing,
            Completed
        }

        [SerializeField] private PlayableDirector _director;
        [SerializeField] private Animator _actor;
        [SerializeField] private CameraController _cameraController;

        private State _state;

        public event Action Completed;

        public void Initialize()
        {
            _state = State.Ready;
            _director.playOnAwake = false;
            _director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            _director.extrapolationMode = DirectorWrapMode.None;
            _director.stopped += HandleDirectorStopped;
            _actor.gameObject.SetActive(true);
            _actor.applyRootMotion = false;
            _actor.updateMode = AnimatorUpdateMode.UnscaledTime;
            _actor.SetBool("IsWalk", false);
            _actor.Play("Idle", 0, 0f);
        }

        public void Play()
        {
            if (_state != State.Ready)
            {
                return;
            }

            _state = State.Playing;
            _cameraController.ActivateGameplayCameraImmediately();
            _director.time = 0d;
            _director.Play();
        }

        public void Dispose()
        {
            _director.stopped -= HandleDirectorStopped;
        }

        private void HandleDirectorStopped(PlayableDirector director)
        {
            if (_state != State.Playing)
            {
                return;
            }

            _state = State.Completed;
            _cameraController.ActivateGameplayCameraImmediately();
            Completed?.Invoke();
        }
    }
}
