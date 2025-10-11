
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    private EntityList _entities = new EntityList();
    
   
    public override void InstallBindings()
    {
        Container.Bind<EntityList>()

                        .FromInstance(_entities)
                            .AsSingle();


        
                               

    }
}
[Serializable]
public class EntityList : List<Transform> { }




