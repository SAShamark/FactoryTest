using UnityEngine;
using Zenject;
using Services.Currency;

namespace Services.Installers
{
    public class ScriptableObjectInstaller : MonoInstaller
    {
        [SerializeField] private CurrencyCollection _currencyCollection;


        public override void InstallBindings()
        {
            InstallBindingAsSingle(_currencyCollection);
        }


        private void InstallBindingAsSingle<T>(T scriptableObject) where T : ScriptableObject
        {
            Container.BindInterfacesAndSelfTo<T>()
                .FromScriptableObject(scriptableObject)
                .AsSingle();
        }
    }
}
