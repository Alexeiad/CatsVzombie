
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    private EntityList _entities = new EntityList();
    private BaseList _bases = new BaseList();
   
    public override void InstallBindings()
    {
        Container.Bind<EntityList>()

                        .FromInstance(_entities)
                            .AsSingle();


        Container.Bind<BaseList>()

                        .FromInstance(_bases)
                            .AsSingle();
                               

    }
}

[Serializable]
public class BaseElementsSaveData
{
    public string ID;
    public Vector3 Position;

    // ѕустой конструктор нужен дл€ десериализации JsonUtility
    public BaseElementsSaveData() { }

    public BaseElementsSaveData(string id, Vector3 pos)
    {
        ID = id;
        Position = pos;
    }
}

[Serializable]
public class EntityList : List<Transform> { }

[Serializable]
public class BaseList : List<BaseElementsSaveData>
{
    public BaseList() : base() { }
    public BaseList(IEnumerable<BaseElementsSaveData> items) : base(items) { }
}

