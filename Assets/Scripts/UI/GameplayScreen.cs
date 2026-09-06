using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameplayScreen : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private TextMeshProUGUI _progressText;

        private int _displayedMeters = -1;

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

        public void SetProgress(float travelledDistance, float targetDistance)
        {
            int meters = Mathf.FloorToInt(travelledDistance);

            if (meters == _displayedMeters)
                return;

            _displayedMeters = meters;
            _progressText.text = $"{meters} / {Mathf.CeilToInt(targetDistance)} m";
        }

        public void ShowFinished()
        {
            _playButton.gameObject.SetActive(false);
            _pauseButton.gameObject.SetActive(false);
            _continueButton.gameObject.SetActive(false);
        }

        private void Play()
        {
            ShowPlaying();
            OnPlay?.Invoke();
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
