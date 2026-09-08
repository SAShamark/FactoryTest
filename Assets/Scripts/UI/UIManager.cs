using System;
using Services.Currency;
using UI.Popups;
using UnityEngine;

namespace UI
{
    [Serializable]
    public class UIManager
    {
        [SerializeField] private GameplayScreen _gameplayScreen;
        [SerializeField] private LevelCompletedPopup _levelCompletedPopup;
        [SerializeField] private ResultPopup _resultPopup;
        [SerializeField] private CurrencyView _currencyView;

        public event Action OnPlay
        {
            add => _gameplayScreen.OnPlay += value;
            remove => _gameplayScreen.OnPlay -= value;
        }

        public event Action OnPause
        {
            add => _gameplayScreen.OnPause += value;
            remove => _gameplayScreen.OnPause -= value;
        }

        public event Action OnContinue
        {
            add => _gameplayScreen.OnContinue += value;
            remove => _gameplayScreen.OnContinue -= value;
        }

        public event Action OnRestart
        {
            add
            {
                _levelCompletedPopup.OnButtonClicked += value;
                _resultPopup.OnButtonClicked += value;
            }
            remove
            {
                _levelCompletedPopup.OnButtonClicked -= value;
                _resultPopup.OnButtonClicked -= value;
            }
        }

        public void Initialize(CurrencyService currencyService)
        {
            _gameplayScreen.Initialize();
            _currencyView.Initialize(currencyService);
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
    }
}
