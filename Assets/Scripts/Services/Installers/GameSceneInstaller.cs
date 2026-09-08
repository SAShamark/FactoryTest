using UI;
using UnityEngine;
using Zenject;

namespace Services.Installers
{
    public class GameSceneInstaller : MonoInstaller
    {
        [SerializeField] private UIManager _uiManager;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<UIManager>().FromInstance(_uiManager).AsSingle();
        }
    }
}
