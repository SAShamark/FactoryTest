using System;
using Gameplay;
using Gameplay.Intro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class GameManager : IDisposable
{
    [SerializeField] private GameplayManager _gameplayManager;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private CutsceneManager _cutsceneManager;

    private bool _hasStarted;
    private bool _gameplayStarted;

    public void Initialize()
    {
        _gameplayManager.Initialize();
        _uiManager.Initialize();
        _cutsceneManager.Initialize();

        _cutsceneManager.Completed += StartGameplay;

        _uiManager.OnPlay += Play;
        _uiManager.OnPause += Pause;
        _uiManager.OnContinue += Continue;
        _uiManager.OnRestart += Restart;

        _gameplayManager.LevelCompleted += _uiManager.ShowLevelCompleted;
        _gameplayManager.LevelFailed += _uiManager.ShowResult;
    }

    internal void LateUpdate()
    {
        _cutsceneManager.LateUpdate();

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
            _uiManager.ShowIntro();
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

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Dispose()
    {
        _cutsceneManager.Completed -= StartGameplay;

        _uiManager.OnPlay -= Play;
        _uiManager.OnPause -= Pause;
        _uiManager.OnContinue -= Continue;
        _uiManager.OnRestart -= Restart;

        _gameplayManager.LevelCompleted -= _uiManager.ShowLevelCompleted;
        _gameplayManager.LevelFailed -= _uiManager.ShowResult;
        _gameplayManager.Dispose();
    }
}
