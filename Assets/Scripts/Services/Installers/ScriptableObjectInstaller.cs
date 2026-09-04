using UnityEngine;
using Zenject;

namespace Services.Installers
{
    public class ScriptableObjectInstaller : MonoInstaller
    {
        // [SerializeField] private CharacterSpellCollection _characterSpellCollection;


        public override void InstallBindings()
        {
            //InstallBindingAsSingle(_characterSpellCollection);
        }


        private void InstallBindingAsSingle<T>(T scriptableObject) where T : ScriptableObject
        {
            Container.BindInterfacesAndSelfTo<T>()
                .FromNewScriptableObject(scriptableObject)
                .AsSingle();
        }
    }
}