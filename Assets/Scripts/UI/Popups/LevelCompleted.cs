using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups
{
    public class LevelCompletedPopup : BasePopup
    {
        [SerializeField] private Button _button;

        public event Action OnButtonClicked;

        private void Start()
        {
            _button.onClick.AddListener(ButtonClicked);
        }

        private void ButtonClicked()
        {
            OnButtonClicked?.Invoke();
        }
    }
}