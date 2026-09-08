namespace Services.Sequence
{
    public interface IGameplaySequence
    {
        float TimeScale { get; }

        void StartGame();
        void StopGame();
        void SetTimeScale(float timeScale);
    }
}
