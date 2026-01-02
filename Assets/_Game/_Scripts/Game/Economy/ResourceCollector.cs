using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.IO;
using System;

public class ResourceCollector : MonoBehaviour
{
    [SerializeField] private CollectorDataSO _collectorDataSO;
    [SerializeField] private float _collectionInterval = 1f;

    [Inject] private ResourceManager _resourceManager;
    [Inject] private List<BuildingBehaviour> _buildingsBehaviours;

    private float _timer;
    private DateTime _lastSaveTime;
    private bool _isFirstLaunch = true;

    private void Start()
    {
        LoadOfflineProgress();
        _lastSaveTime = DateTime.UtcNow;
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

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // При сворачивании приложения
            SaveOfflineTime();
        }
        else
        {
            // При разворачивании приложения
            LoadOfflineProgress();
            _lastSaveTime = DateTime.UtcNow;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // При потере фокуса (сворачивании)
            SaveOfflineTime();
        }
        else
        {
            // При получении фокуса (разворачивании)
            LoadOfflineProgress();
            _lastSaveTime = DateTime.UtcNow;
        }
    }

    private void OnApplicationQuit()
    {
        SaveOfflineTime();
    }

    private void CollectResources(int multiplier = 1)
    {
        if (_buildingsBehaviours == null || _collectorDataSO == null) return;

        int totalWater = 0;
        int totalFood = 0;
        int totalMaterials = 0;
        int totalDiamonds = 0;

        foreach (BuildingBehaviour buildingsBehaviour in _buildingsBehaviours)
        {
            if (buildingsBehaviour == null) continue;

            foreach (ResoursesToGive resoursesToGive in _collectorDataSO.resoursesToGives)
            {
                if (buildingsBehaviour.baseType == resoursesToGive.buildingType
                    && buildingsBehaviour.levelType == resoursesToGive.buildingLevelType)
                {
                    totalWater += resoursesToGive.waterToGive * multiplier;
                    totalFood += resoursesToGive.foodToGive * multiplier;
                    totalMaterials += resoursesToGive.materialsToGive * multiplier;
                    totalDiamonds += resoursesToGive.diamondToGive * multiplier;
                }
            }
        }

        // Добавляем ресурсы один раз для оптимизации
        if (totalWater > 0) _resourceManager.AddWater(totalWater);
        if (totalFood > 0) _resourceManager.AddFood(totalFood);
        if (totalMaterials > 0) _resourceManager.AddMaterials(totalMaterials);
        if (totalDiamonds > 0) _resourceManager.AddDiamonds(totalDiamonds);

        _resourceManager.SaveToJson();
    }

    private void SaveOfflineTime()
    {
        long seconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerPrefs.SetString("LastSave", seconds.ToString());

        // Сохраняем текущий прогресс зданий
        SaveBuildingsProgress();

        PlayerPrefs.Save();
    }

    private void SaveBuildingsProgress()
    {
        // Сохраняем время последнего апдейта каждого здания
        foreach (var building in _buildingsBehaviours)
        {
            if (building != null)
            {
                string key = $"Building_{building.GetInstanceID()}_LastUpdate";
                PlayerPrefs.SetString(key, DateTime.UtcNow.Ticks.ToString());
            }
        }
    }

    private void LoadOfflineProgress()
    {
        if (!PlayerPrefs.HasKey("LastSave"))
        {
            _isFirstLaunch = true;
            return;
        }

        try
        {
            long savedSeconds = long.Parse(PlayerPrefs.GetString("LastSave"));
            long currentSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long secondsPassed = currentSeconds - savedSeconds;

            // Ограничиваем максимальное время оффлайн (например, 24 часа)
            long maxOfflineSeconds = 24 * 60 * 60;
            secondsPassed = Math.Min(secondsPassed, maxOfflineSeconds);

            if (secondsPassed > 0)
            {
                int cycles = Mathf.FloorToInt(secondsPassed / _collectionInterval);

                // Если прошло много времени, ограничиваем количество циклов
                int maxCycles = 3600; // Максимум 1 час оффлайн прогресса за раз
                cycles = Mathf.Min(cycles, maxCycles);

                if (cycles > 0)
                {
                    // Для первого запуска показываем уведомление
                    if (_isFirstLaunch)
                    {
                        Debug.Log($"Оффлайн прогресс: {secondsPassed} секунд, {cycles} циклов");
                        // Здесь можно показать UI-уведомление о полученных ресурсах
                    }

                    CollectResources(cycles);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка загрузки оффлайн прогресса: {e.Message}");
        }

        _isFirstLaunch = false;
    }

    // Дополнительный метод для принудительного расчета оффлайн прогресса
    public void ForceOfflineProgressCalculation()
    {
        LoadOfflineProgress();
        _lastSaveTime = DateTime.UtcNow;
    }

    // Метод для получения времени с последнего сохранения
    public TimeSpan GetTimeSinceLastSave()
    {
        return DateTime.UtcNow - _lastSaveTime;
    }
}