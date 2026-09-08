using UnityEngine;

namespace Services.Sequence
{
    public class GameplaySequence : IGameplaySequence
    {
        public float TimeScale => Time.timeScale;

        public void StartGame()
        {
            SetTimeScale(1f);
        }

        public void StopGame()
        {
            SetTimeScale(0f);
        }

        public void SetTimeScale(float timeScale)
        {
            Time.timeScale = Mathf.Max(0f, timeScale);
        }
    }
}
