

using TMPro;
using UnityEngine;
using Zenject;

public class ResourceValidator:MonoBehaviour
{
    [SerializeField] private CostOfBuildingSO _costOfBuildingSO;
    [SerializeField] private TextMeshProUGUI _textCost;

    [Inject] private ResourceManager _resourceManager;

    public bool IsEnoughResourses(BuildingType building, BuildingLevelType buildingLevelType)
    {
       

        foreach (CostBuildingType costBuilding in _costOfBuildingSO.costBuildings)
        {
            if (costBuilding.buildingType == building &&
                costBuilding.buildingLevelType == buildingLevelType)
            {
                _resourceManager.AddMaterials(-costBuilding.cost);
                _resourceManager.SaveToJson();
               return _resourceManager.Materials>= costBuilding.cost;
            }
        }

        Debug.LogWarning($"Стоимость для {building} уровня {buildingLevelType} не найдена");
        return false;
    }
    public void CostInfo(BuildingType building, BuildingLevelType buildingLevelType)
    {
        foreach (CostBuildingType costBuilding in _costOfBuildingSO.costBuildings)
        {
            if (costBuilding.buildingType == building &&
                costBuilding.buildingLevelType == buildingLevelType)
            {
                _textCost.text = (-costBuilding.cost).ToString();
            }
        }
    }
}
