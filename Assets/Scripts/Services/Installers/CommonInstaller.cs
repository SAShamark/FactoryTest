using Services.Currency;
using Services.Sequence;
using UnityEngine;
using Zenject;

namespace Services.Installers
{
    public class CommonInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<CurrencyService>().AsSingle();
            Container.Bind<IGameplaySequence>().To<GameplaySequence>().AsSingle();

            /*
            Container.BindInterfacesAndSelfTo<TutorialManager>().AsSingle();*/
        }
    }
}