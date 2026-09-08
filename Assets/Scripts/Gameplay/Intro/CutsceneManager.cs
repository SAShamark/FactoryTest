using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Gameplay.Intro
{
    [Serializable]
    public class CutsceneManager : IDisposable
    {
        [SerializeField] private PlayableDirector _director;
        [SerializeField] private Animator _actor;
        [SerializeField] private CameraController _cameraController;

        private bool _hasStarted;
        private bool _hasCompleted;

        public event Action Completed;

        public void Initialize()
        {
            _hasStarted = false;
            _hasCompleted = false;
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
            if (_hasStarted)
                return;

            _hasStarted = true;
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
            if (!_hasStarted || _hasCompleted)
                return;

            _hasCompleted = true;
            _cameraController.ActivateGameplayCameraImmediately();
            Completed?.Invoke();
        }
    }
}
