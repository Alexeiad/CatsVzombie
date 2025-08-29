
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    private List<Transform> _entities = new List<Transform>();
   
    public override void InstallBindings()
    {
        Container.Bind<List<Transform>>()
                 .FromInstance(_entities)
                 .AsSingle()
                 .NonLazy();
    }
}
