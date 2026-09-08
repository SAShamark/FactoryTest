using DG.Tweening;
using Services.Currency;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class CurrencyView : MonoBehaviour
    {
        [SerializeField] private CurrencyType _type;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Image _image;
        [SerializeField] private float _countDuration = 0.35f;
        [SerializeField] private Vector3 _punchStrength = new(0.18f, 0.18f, 0f);
        [SerializeField] private float _punchDuration = 0.25f;

        private IBank _bank;
        private int _displayedValue;
        private Tween _countTween;
        private Tween _punchTween;
        private Vector3 _textDefaultScale;
        private CurrencyService _currencyService;

        [Inject]
        private void Construct(CurrencyService currencyService)
        {
            _currencyService = currencyService;
        }
        
        public void Initialize()
        {
            _textDefaultScale = _text.transform.localScale;
            _image.sprite = _currencyService.CurrencyCollection.GetSprite(_type);

            _bank = _currencyService.GetCurrencyByType(_type);
            _bank.OnCurrencyChanged += SetCurrency;

            _displayedValue = _bank.Currency;
            _text.text = _displayedValue.ToString();
        }

        private void OnDestroy()
        {
            if (_bank != null)
                _bank.OnCurrencyChanged -= SetCurrency;

            _countTween?.Kill();
            _punchTween?.Kill();
        }

        private void SetCurrency(int value)
        {
            _countTween?.Kill();

            if (!gameObject.activeInHierarchy)
            {
                SetDisplayedValue(value);
                return;
            }

            _countTween = DOTween.To(() => _displayedValue, SetDisplayedValue, value, _countDuration)
                .SetLink(gameObject);

            PlayPunch();
        }

        private void SetDisplayedValue(int value)
        {
            _displayedValue = value;
            _text.text = value.ToString();
        }

        private void PlayPunch()
        {
            _punchTween?.Kill();
            _text.transform.localScale = _textDefaultScale;
            _punchTween = _text.transform
                .DOPunchScale(_punchStrength, _punchDuration, 8, 0.8f)
                .SetLink(gameObject);
        }
    }
}
