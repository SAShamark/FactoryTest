using System;
using Gameplay;
using UI;
using UnityEngine;

[Serializable]
public class GameManager : IDisposable
{
    [SerializeField] private GameplayManager _gameplayManager;
    [SerializeField] private GameplayScreen _gameplayScreen;

    public void Initialize()
    {
        _gameplayManager.Initialize();
        _gameplayScreen.Initialize();

        _gameplayScreen.OnPlay += Play;
        _gameplayScreen.OnPause += Pause;
        _gameplayScreen.OnContinue += Continue;
    }

    internal void LateUpdate()
    {
        _gameplayManager.LateUpdate();
    }

    private void Play()
    {
        _gameplayManager.StartGameplay();
    }

    private void Pause()
    {
        _gameplayManager.PauseGameplay();
    }

    private void Continue()
    {
        _gameplayManager.ContinueGameplay();
    }

    public void Dispose()
    {
        _gameplayScreen.OnPlay -= Play;
        _gameplayScreen.OnPause -= Pause;
        _gameplayScreen.OnContinue -= Continue;
    }
}
