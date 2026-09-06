using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    private void Awake()
    {
        _gameManager.Initialize();
    }

    private void OnDestroy()
    {
        _gameManager.Dispose();
    }

    private void LateUpdate()
    {
        _gameManager.LateUpdate();
    }
}
