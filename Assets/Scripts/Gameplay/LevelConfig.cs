using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Gameplay/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField, Min(1f)] private float _targetDistance = 300f;

        public float TargetDistance => _targetDistance;
    }
}
