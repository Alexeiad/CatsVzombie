
using UnityEngine;
using Zenject;

internal class BaseManagerInstaller:MonoInstaller
{
    [SerializeField] private BaseSaveManager _saveManager;
    public override void InstallBindings()
    {
        Container.Bind<BaseSaveManager>().FromInstance(_saveManager).AsSingle().NonLazy();
    }
}


