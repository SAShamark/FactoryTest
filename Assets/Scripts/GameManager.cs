using System;
using Gameplay;
using Gameplay.Entities.BaseUnit;
using Gameplay.Intro;
using Services.Currency;
using Services.Sequence;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class GameManager : IDisposable
{
    [SerializeField] private GameplayManager _gameplayManager;
    [SerializeField] private CutsceneManager _cutsceneManager;

    private CurrencyService _currencyService;
    private IGameplaySequence _gameplaySequence;
    private UIManager _uiManager;
    private FloatingTextService _floatingTextService;

    private bool _hasStarted;
    private bool _gameplayStarted;

    public void Initialize(
        CurrencyService currencyService,
        IGameplaySequence gameplaySequence,
        UIManager uiManager,
        FloatingTextService floatingTextService)
    {
        _currencyService = currencyService;
        _gameplaySequence = gameplaySequence;
        _uiManager = uiManager;
        _floatingTextService = floatingTextService;

        _gameplayManager.Initialize(_gameplaySequence, _floatingTextService);
        _cutsceneManager.Initialize();

        _cutsceneManager.Completed += StartGameplay;

        _uiManager.CommandRequested += HandleUICommand;

        _gameplayManager.LevelCompleted += _uiManager.ShowLevelCompleted;
        _gameplayManager.LevelFailed += _uiManager.ShowResult;
        _gameplayManager.EnemyKilled += RewardForKill;
    }

    internal void LateUpdate()
    {
        if (!_gameplayStarted)
            return;

        _gameplayManager.LateUpdate();
        _uiManager.SetProgress(_gameplayManager.TravelledDistance, _gameplayManager.TargetDistance);
    }

    private void Play()
    {
        if (_hasStarted)
            return;

        _hasStarted = true;
        if (_cutsceneManager != null)
        {
            _cutsceneManager.Play();
            return;
        }

        StartGameplay();
    }

    public void StartGameplay()
    {
        if (_gameplayStarted)
            return;

        _gameplayStarted = true;
        _uiManager.ShowLaunch();
        _gameplayManager.StartGameplay();
    }

    private void Pause()
    {
        if (!_gameplayStarted)
            return;
        _gameplayManager.PauseGameplay();
    }

    private void Continue()
    {
        if (!_gameplayStarted)
            return;
        _gameplayManager.ContinueGameplay();
    }

    private void RewardForKill()
    {
        _currencyService.GetCurrencyByType(CurrencyType.Coin).EarnCurrency(1);
    }

    private void HandleUICommand(UICommand command)
    {
        switch (command)
        {
            case UICommand.Play:
                Play();
                break;
            case UICommand.Pause:
                Pause();
                break;
            case UICommand.Continue:
                Continue();
                break;
            case UICommand.Restart:
                Restart();
                break;
        }
    }

    private void Restart()
    {
        _gameplaySequence.StartGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Dispose()
    {
        _cutsceneManager.Completed -= StartGameplay;
        _cutsceneManager.Dispose();

        _uiManager.CommandRequested -= HandleUICommand;

        _gameplayManager.LevelCompleted -= _uiManager.ShowLevelCompleted;
        _gameplayManager.LevelFailed -= _uiManager.ShowResult;
        _gameplayManager.EnemyKilled -= RewardForKill;
        _gameplayManager.Dispose();
    }
}
