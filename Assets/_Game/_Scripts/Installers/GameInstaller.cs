
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    private EntityList _entities = new EntityList();



    public override void InstallBindings()
    {
        Container.Bind<EntityList>().AsSingle().NonLazy();

    }
}
[Serializable]
public class EntityList : List<Transform> { }




