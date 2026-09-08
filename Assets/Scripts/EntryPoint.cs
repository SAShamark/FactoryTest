using Services;
using UnityEngine;
using Services.Currency;
using Services.Sequence;
using UI;
using Zenject;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    private CurrencyService _currencyService;
    private IGameplaySequence _gameplaySequence;
    private UIManager _uiManager;

    [Inject]
    private void Construct(CurrencyService currencyService, IGameplaySequence gameplaySequence, UIManager uiManager)
    {
        _currencyService = currencyService;
        _gameplaySequence = gameplaySequence;
        _uiManager = uiManager;
    }

    private void Awake()
    {
        Application.targetFrameRate = ValueConstants.TARGET_FRAME_RATE;
        _gameManager.Initialize(_currencyService, _gameplaySequence, _uiManager);
    }

    private void OnDestroy()
    {
        _gameManager.Dispose();
    }

    private void LateUpdate()
    {
        _gameManager.LateUpdate();
    }

    public void StartGameplay()
    {
        _gameManager.StartGameplay();
    }
}
