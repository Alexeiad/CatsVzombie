using System.Collections.Generic;
using UnityEngine;

public class BaseManager
{
    private List<Building> _buildings = new List<Building>();
    private List<BuildPlot> _plots;
    private ResourceManager _resourceManager;

    public BaseManager(List<BuildPlot> buildPlots, ResourceManager resourceManager)
    {
        _plots = buildPlots;
        _resourceManager = resourceManager;
    }

    // Построить здание на участке
    // Возвращает true если удалось, false если нет места или ресурсов
    public bool BuildBuilding(BuildPlot plot, BuildingType type, int level, int foodCost, int waterCost, int materialCost)
    {
        if (!_resourceManager.HasEnoughResources(foodCost, waterCost, materialCost))
        {
            Debug.Log("Не хватает ресурсов");
            return false;
        }

        if (!plot.CanBuild(level))
        {
            Debug.Log("Нельзя построить здание на этом участке по уровне/месту");
            return false;
        }

        Building newBuilding = new Building(type, level, foodCost, waterCost, materialCost);

        plot.AddBuilding(newBuilding);
        _buildings.Add(newBuilding);
        _resourceManager.SpendResources(foodCost, waterCost, materialCost);

        Debug.Log($"Построено здание {type} лвл {level}");
        return true;
    }

    public IReadOnlyList<Building> GetBuildings() => _buildings.AsReadOnly();
}