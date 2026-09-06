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

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();

                _closeButton.onClick.AddListener(CloseTrigger);
            }

            Debug.Log($"{gameObject.name} popup showed");
        }

        public virtual void CloseTrigger()
        {
            gameObject.SetActive(false);
        }
    }
}