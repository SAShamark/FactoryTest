using UnityEngine;

namespace Services.Sequence
{
    public class GameplaySequence : IGameplaySequence
    {
        public void StartGame()
        {
            Time.timeScale = 1;
        }

        public void StopGame()
        {
            Time.timeScale = 0;
        }
    }
}