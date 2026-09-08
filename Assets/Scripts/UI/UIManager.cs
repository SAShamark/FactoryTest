using System;
using UI.Popups;
using UnityEngine;
using Zenject;

namespace UI
{
    public enum UICommand
    {
        Play,
        Pause,
        Continue,
        Restart
    }

    public class UIManager : MonoBehaviour, IInitializable
    {
        [SerializeField] private GameplayScreen _gameplayScreen;
        [SerializeField] private LevelCompletedPopup _levelCompletedPopup;
        [SerializeField] private ResultPopup _resultPopup;

        public event Action<UICommand> CommandRequested;

        public void Initialize()
        {
            _gameplayScreen.PlayRequested += HandlePlayRequested;
            _gameplayScreen.PauseRequested += HandlePauseRequested;
            _gameplayScreen.ContinueRequested += HandleContinueRequested;
            _levelCompletedPopup.RestartRequested += HandleRestartRequested;
            _resultPopup.RestartRequested += HandleRestartRequested;

            _gameplayScreen.Initialize();
            _levelCompletedPopup.CloseTrigger();
            _resultPopup.CloseTrigger();
        }

        public void SetProgress(float travelledDistance, float targetDistance)
        {
            _gameplayScreen.SetProgress(travelledDistance, targetDistance);
        }

        public void ShowLevelCompleted()
        {
            _levelCompletedPopup.Show();
        }

        public void ShowLaunch()
        {
            _gameplayScreen.ShowLaunch();
        }

        public void ShowResult()
        {
            _resultPopup.Show();
        }

        private void HandlePlayRequested()
        {
            CommandRequested?.Invoke(UICommand.Play);
        }

        private void HandlePauseRequested()
        {
            CommandRequested?.Invoke(UICommand.Pause);
        }

        private void HandleContinueRequested()
        {
            CommandRequested?.Invoke(UICommand.Continue);
        }

        private void HandleRestartRequested()
        {
            CommandRequested?.Invoke(UICommand.Restart);
        }

        private void OnDestroy()
        {
            _gameplayScreen.PlayRequested -= HandlePlayRequested;
            _gameplayScreen.PauseRequested -= HandlePauseRequested;
            _gameplayScreen.ContinueRequested -= HandleContinueRequested;
            _levelCompletedPopup.RestartRequested -= HandleRestartRequested;
            _resultPopup.RestartRequested -= HandleRestartRequested;
        }
    }
}
