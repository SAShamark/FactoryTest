using UnityEngine;
using Zenject;

namespace Services.Installers
{
    public class CommonInstaller : MonoInstaller
    {
        [SerializeField] private bool _adsTestMode = true;
        
        public override void InstallBindings()
        {
            /*Container.Bind<FirebaseManager>().AsSingle();
            Container.Bind<IStorageService>().To<StorageService>().AsSingle();
            Container.BindInterfacesAndSelfTo<CurrencyService>().AsSingle();
            Container.BindInterfacesAndSelfTo<GraphicsManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<LocalizationManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<OrientationManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<CameraTiltManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<SpellsDataService>().AsSingle();
            CustomizationInstaller();
            Container.Bind<ISceneLoader>().To<SceneLoader>().AsSingle();
            Container.Bind<IGameplaySequence>().To<GameplaySequence>().AsSingle();
            Container.Bind<GameMode>().AsSingle();
            Container.BindInterfacesAndSelfTo<TimerService>().AsSingle();
            Container.Bind<ChestRewards>().AsSingle();
            Container.BindInterfacesAndSelfTo<LevelsManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<EndlessManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<DailyRewardsManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<AccountDataService>().AsSingle();
            Container.BindInterfacesAndSelfTo<NotificationService>().AsSingle();
            Container.BindInterfacesAndSelfTo<IAPManager>().AsSingle();

            AdsInstaller();
            CoroutineInstaller();
            AudioInstaller();
            UIInstaller();
            Container.BindInterfacesAndSelfTo<TutorialManager>().AsSingle();*/
        }

        /*private void CustomizationInstaller()
        {
            Container.BindInterfacesAndSelfTo<EnvironmentDataService>().AsSingle();
            Container.BindInterfacesAndSelfTo<CharacterCustomizationDataService>().AsSingle();
        }

        private void AudioInstaller()
        {
            var projectAudio = Container.InstantiatePrefabForComponent<ProjectAudio>(_projectAudio);
            projectAudio.gameObject.transform.SetParent(null);
            Container.BindInterfacesAndSelfTo<ProjectAudio>().FromInstance(projectAudio).AsSingle();
        }

        private void UIInstaller()
        {
            var projectCanvas = Container.InstantiatePrefabForComponent<ProjectCanvas>(_projectCanvas);
            projectCanvas.gameObject.transform.SetParent(null);
            Container.BindInterfacesAndSelfTo<ProjectCanvas>().FromInstance(projectCanvas).AsSingle();
        }*/
    }
}