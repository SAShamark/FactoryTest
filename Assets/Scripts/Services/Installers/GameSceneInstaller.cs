using Gameplay;
using Gameplay.Entities.BaseUnit;
using UI;
using UnityEngine;
using Zenject;

namespace Services.Installers
{
    public class GameSceneInstaller : MonoInstaller
    {
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private FloatingTextControl _floatingTextPrefab;
        [SerializeField] private CameraController _cameraController;
        [SerializeField, Min(1)] private int _floatingTextPoolSize = 16;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<UIManager>().FromInstance(_uiManager).AsSingle();
            Container.Bind<FloatingTextService>().FromInstance(new FloatingTextService(
                _floatingTextPrefab,
                _cameraController,
                transform,
                _floatingTextPoolSize)).AsSingle();
        }
    }
}
