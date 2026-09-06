using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameplayScreen : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _continueButton;

        public event Action OnPlay;
        public event Action OnPause;
        public event Action OnContinue;

        private void Awake()
        {
            _playButton.onClick.AddListener(Play);
            _pauseButton.onClick.AddListener(Pause);
            _continueButton.onClick.AddListener(Continue);
        }

        public void Initialize()
        {
            ShowBeforePlay();
        }

        private void Play()
        {
            OnPlay?.Invoke();
            ShowPlaying();
        }

        private void Pause()
        {
            OnPause?.Invoke();
            ShowPaused();
        }

        private void Continue()
        {
            OnContinue?.Invoke();
            ShowPlaying();
        }

        private void ShowBeforePlay()
        {
            _playButton.gameObject.SetActive(true);
            _pauseButton.gameObject.SetActive(false);
            _continueButton.gameObject.SetActive(false);
        }

        private void ShowPlaying()
        {
            _playButton.gameObject.SetActive(false);
            _pauseButton.gameObject.SetActive(true);
            _continueButton.gameObject.SetActive(false);
        }

        private void ShowPaused()
        {
            _playButton.gameObject.SetActive(false);
            _pauseButton.gameObject.SetActive(false);
            _continueButton.gameObject.SetActive(true);
        }
    }
}
