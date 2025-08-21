using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BaseInstaller : MonoInstaller
{
    [SerializeField] private int _startFood = 100;
    [SerializeField] private int _startWater = 100;
    [SerializeField] private int _startMaterials = 100;

    public override void InstallBindings()
    {
        // Регистрируем менеджер ресурсов с начальными значениями
        ResourceManager resourceManager = new ResourceManager(_startFood, _startWater, _startMaterials);
        Container.Bind<ResourceManager>().FromInstance(resourceManager).AsSingle();

 
    }
}
