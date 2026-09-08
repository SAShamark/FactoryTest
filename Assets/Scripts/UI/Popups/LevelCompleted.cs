using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups
{
    public class LevelCompletedPopup : BasePopup
    {
        [SerializeField] private Button _button;

        public event Action RestartRequested;

        protected override void Awake()
        {
            base.Awake();
            _button.onClick.AddListener(ButtonClicked);
        }

        protected override void OnDestroy()
        {
            _button.onClick.RemoveListener(ButtonClicked);
            base.OnDestroy();
        }

        private void ButtonClicked()
        {
            RestartRequested?.Invoke();
        }
    }
}
