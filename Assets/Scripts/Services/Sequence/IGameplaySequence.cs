namespace Services.Sequence
{
    public interface IGameplaySequence
    {
        void StartGame();
        void StopGame();
        void SetTimeScale(float timeScale);
    }
}
