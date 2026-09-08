using Services.Currency;
using Services.Sequence;
using Services.Storage;
using UnityEngine;
using Zenject;

namespace Services.Installers
{
    public class CommonInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<StorageService>().AsSingle();
            Container.BindInterfacesAndSelfTo<CurrencyService>().AsSingle().NonLazy();
            Container.Bind<IGameplaySequence>().To<GameplaySequence>().AsSingle();
        }
    }
}
