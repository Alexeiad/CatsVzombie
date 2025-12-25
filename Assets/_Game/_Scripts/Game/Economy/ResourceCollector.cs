
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ResourceCollector : MonoBehaviour
{
    [SerializeField] private CollectorDataSO _collectorDataSO;

    [Inject] private ResourceManager _resourceManager;
    [Inject] private List<BuildingBehaviour> _buildingsBehaviours;

    [SerializeField] private float _collectionInterval = 1f; // Раз в секунду
    private float _timer;

    public void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _collectionInterval)
        {
            _timer = 0f;
            CollectResources();
        }
    }

    private void CollectResources()
    {
        foreach (BuildingBehaviour buildingsBehaviour in _buildingsBehaviours)
        {
            foreach (ResoursesToGive resoursesToGive in _collectorDataSO.resoursesToGives)
            {
                if (buildingsBehaviour.baseType == resoursesToGive.buildingType
                    && buildingsBehaviour.levelType == resoursesToGive.buildingLevelType)
                {
                    _resourceManager.AddWater(resoursesToGive.waterToGive);
                    _resourceManager.AddFood(resoursesToGive.foodToGive);
                    _resourceManager.AddMaterials(resoursesToGive.materialsToGive);
                    _resourceManager.AddDiamonds(resoursesToGive.diamondToGive);
                }
            }
        }
    }

}
