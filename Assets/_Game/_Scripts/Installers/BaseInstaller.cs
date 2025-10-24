using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BaseInstaller : MonoInstaller
{
    [SerializeField] private int _startFood = 100;
    [SerializeField] private int _startWater = 100;
    [SerializeField] private int _startMaterials = 100;

    private BuildingsList _bases = new BuildingsList();

    public override void InstallBindings()
    {
        ResourceManager resourceManager = new ResourceManager(_startFood, _startWater, _startMaterials);
        Container.Bind<ResourceManager>()
            .FromInstance(resourceManager)
            .AsSingle();

        Container.Bind<BuildingsList>()
            .FromInstance(_bases)
            .AsSingle();

        // Optional: Load saved data immediately
        resourceManager.LoadFromJson();
    }
}

[Serializable]
public class BuildingsSaveData
{
    public string ID;
    public Vector3 Position;

    // ѕустой конструктор нужен дл€ десериализации JsonUtility
    public BuildingsSaveData() { }

    public BuildingsSaveData(string id, Vector3 pos)
    {
        ID = id;
        Position = pos;
    }
}



[Serializable]
public class BuildingsList : List<BuildingsSaveData>
{
    public BuildingsList() : base() { }
    public BuildingsList(IEnumerable<BuildingsSaveData> items) : base(items) { }
}