using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.IO;

public class ResourceCollector : MonoBehaviour
{
    [SerializeField] private CollectorDataSO _collectorDataSO;

    [Inject] private ResourceManager _resourceManager;
    [Inject] private List<BuildingBehaviour> _buildingsBehaviours;

    [SerializeField] private float _collectionInterval = 1f;
    private float _timer;

    private void Start()
    {
        LoadOfflineProgress();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _collectionInterval)
        {
            _timer = 0f;
            CollectResources();
        }
    }

    private void OnApplicationQuit()
    {
        SaveOfflineTime();
    }

    private void CollectResources()
    {
        if(_buildingsBehaviours!=null)
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
                    _resourceManager.SaveToJson();
                }
            }
        }
    }

    private void SaveOfflineTime()
    {
        long seconds = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerPrefs.SetString("LastSave", seconds.ToString());
        PlayerPrefs.Save();
    }

    private void LoadOfflineProgress()
    {
        if (!PlayerPrefs.HasKey("LastSave")) return;

        long savedSeconds = long.Parse(PlayerPrefs.GetString("LastSave"));
        long currentSeconds = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long secondsPassed = currentSeconds - savedSeconds;

        if (secondsPassed > 0)
        {
            int cycles = Mathf.FloorToInt(secondsPassed / _collectionInterval);
            for (int i = 0; i < cycles; i++)
            {
                CollectResources();
            }
        }
    }
}