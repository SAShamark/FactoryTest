using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups
{
    public class BasePopup : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void CloseTrigger()
        {
            gameObject.SetActive(false);
        }

        protected virtual void Awake()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(CloseTrigger);
        }

        protected virtual void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(CloseTrigger);
        }
    }
}
