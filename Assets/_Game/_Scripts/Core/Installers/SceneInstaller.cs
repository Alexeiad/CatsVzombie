
using Zenject;
using UnityEngine;

public class SceneInstaller : MonoInstaller
{
    [SerializeField] private DynamicJoystick _joystick;

    public override void InstallBindings()
    {
        Container.BindInstance(_joystick).AsSingle();
    }
}