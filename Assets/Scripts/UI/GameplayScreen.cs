using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameplayScreen : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _continueButton;
        

        private void Awake()
        {
            _pauseButton.onClick.AddListener(Pause);
            _continueButton.onClick.AddListener(Continue);
        }

        private void Pause()
        {
            Time.timeScale = 0f;
            _continueButton.gameObject.SetActive(true);
        }

        private void Continue()
        {
            Time.timeScale = 1f;
            _continueButton.gameObject.SetActive(false);
        }
    }
}