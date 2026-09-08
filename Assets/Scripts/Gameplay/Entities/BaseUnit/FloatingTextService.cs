using Services.ObjectPool;
using UnityEngine;

namespace Gameplay.Entities.BaseUnit
{
    public class FloatingTextService
    {
        private readonly CameraController _cameraController;
        private readonly ObjectPool<FloatingTextControl> _pool;

        public FloatingTextService(
            FloatingTextControl prefab,
            CameraController cameraController,
            Transform container,
            int initialPoolSize)
        {
            _cameraController = cameraController;

            if (prefab == null)
            {
                Debug.LogError($"{nameof(FloatingTextService)} is missing prefab.");
                return;
            }

            _pool = new ObjectPool<FloatingTextControl>(
                prefab,
                Mathf.Max(1, initialPoolSize),
                container);
        }

        public void ShowDamage(float damage, Vector3 worldPosition)
        {
            int displayedDamage = Mathf.Max(1, Mathf.RoundToInt(damage));
            Show(displayedDamage.ToString(), false, worldPosition);
        }

        public void ShowReward(Vector3 worldPosition)
        {
            Show("+1", true, worldPosition);
        }

        private void Show(string value, bool isReward, Vector3 worldPosition)
        {
            if (_pool == null)
                return;

            Camera viewCamera = _cameraController != null ? _cameraController.OutputCamera : null;
            _pool.GetFreeElement().Play(value, isReward, worldPosition, viewCamera);
        }
    }
}
