using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameplayScreen : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private GameObject _afterLaunch;
        [SerializeField] private CurrencyView _currencyView;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private Slider _progressSlider;

        private int _displayedPercent = -1;

        public event Action PlayRequested;
        public event Action PauseRequested;
        public event Action ContinueRequested;

        private void Awake()
        {
            _playButton.onClick.AddListener(Play);
            _pauseButton.onClick.AddListener(Pause);
            _continueButton.onClick.AddListener(Continue);
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveListener(Play);
            _pauseButton.onClick.RemoveListener(Pause);
            _continueButton.onClick.RemoveListener(Continue);
        }

        public void Initialize()
        {
            _currencyView.Initialize();
            ShowBeforePlay();
        }

        public void SetProgress(float travelledDistance, float targetDistance)
        {
            float normalizedProgress = Mathf.Clamp01(travelledDistance / targetDistance);
            _progressSlider.normalizedValue = normalizedProgress;

            int percent = Mathf.FloorToInt(normalizedProgress * 100f);
            if (percent == _displayedPercent)
                return;

            _displayedPercent = percent;
            _progressText.text = $"{percent}%";
        }

        public void ShowLaunch()
        {
            _afterLaunch.SetActive(true);
        }

        private void Play()
        {
            ShowPlaying();
            PlayRequested?.Invoke();
        }

        private void Pause()
        {
            PauseRequested?.Invoke();
            ShowPaused();
        }

        private void Continue()
        {
            ContinueRequested?.Invoke();
            ShowPlaying();
        }

        private void ShowBeforePlay()
        {
            _afterLaunch.SetActive(false);
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
