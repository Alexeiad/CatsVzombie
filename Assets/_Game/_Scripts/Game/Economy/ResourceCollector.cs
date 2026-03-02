using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System;

public class ResourceCollector : MonoBehaviour
{
    [SerializeField] private CollectorDataSO _collectorDataSO;
    [SerializeField] private int _maxOfflineHours = 168; // 7 дней

    [Inject] private ResourceManager _resourceManager;
    [Inject] private List<BuildingBehaviour> _buildingsBehaviours;

    private const string LAST_SAVE_KEY = "LastSaveTime";
    private const string SESSION_LOADED_KEY = "SessionOfflineLoaded"; // флаг в рамках сессии

    private float _hourTimer; // таймер текущего часа
    private const float HOUR_SECONDS = 3600f;

    // Статический флаг - сбрасывается только при полном перезапуске приложения
    private static bool _offlineAlreadyProcessed = false;

    private void Start()
    {
        // Оффлайн считаем только один раз за запуск приложения
        if (!_offlineAlreadyProcessed)
        {
            ProcessOfflineProgress();
            _offlineAlreadyProcessed = true;
        }

        // Восстанавливаем таймер текущего часа
        _hourTimer = PlayerPrefs.GetFloat("HourTimer", 0f);
    }

    private void Update()
    {
        _hourTimer += Time.deltaTime;

        if (_hourTimer >= HOUR_SECONDS)
        {
            _hourTimer -= HOUR_SECONDS;
            CollectResources(1);
            SaveState();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveState();
    }

    private void OnApplicationQuit()
    {
        SaveState();
    }

    // OnApplicationFocus убираем — он вызывается при смене сцены и даёт баги

    private void SaveState()
    {
        PlayerPrefs.SetString(LAST_SAVE_KEY, DateTime.UtcNow.Ticks.ToString());
        PlayerPrefs.SetFloat("HourTimer", _hourTimer);
        PlayerPrefs.Save();
    }

    private void ProcessOfflineProgress()
    {
        if (!PlayerPrefs.HasKey(LAST_SAVE_KEY))
        {
            SaveState();
            return;
        }

        try
        {
            long savedTicks = long.Parse(PlayerPrefs.GetString(LAST_SAVE_KEY));
            DateTime savedTime = new DateTime(savedTicks, DateTimeKind.Utc);
            DateTime currentTime = DateTime.UtcNow;

            if (currentTime <= savedTime) return;

            float savedHourTimer = PlayerPrefs.GetFloat("HourTimer", 0f);
            double secondsPassed = (currentTime - savedTime).TotalSeconds + savedHourTimer;

            // Считаем полные часы
            int fullHours = Mathf.FloorToInt((float)(secondsPassed / HOUR_SECONDS));
            fullHours = Mathf.Min(fullHours, _maxOfflineHours);

            // Остаток для таймера
            _hourTimer = (float)(secondsPassed % HOUR_SECONDS);

            if (fullHours > 0)
            {
                Debug.Log($"[Offline] Прошло {fullHours} ч., начисляем ресурсы");
                CollectResources(fullHours);
                ShowOfflineNotification(fullHours);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Offline] Ошибка: {e.Message}");
            _hourTimer = 0f;
        }
    }

    private void CollectResources(int hours)
    {
        if (_buildingsBehaviours == null || _collectorDataSO == null) return;

        int totalWater = 0, totalFood = 0, totalMaterials = 0, totalDiamonds = 0;

        foreach (var building in _buildingsBehaviours)
        {
            if (building == null) continue;

            foreach (var resource in _collectorDataSO.resoursesToGives)
            {
                if (building.baseType == resource.buildingType
                    && building.levelType == resource.buildingLevelType)
                {
                    totalWater += resource.waterToGive * hours;
                    totalFood += resource.foodToGive * hours;
                    totalMaterials += resource.materialsToGive * hours;
                    totalDiamonds += resource.diamondToGive * hours;
                }
            }
        }

        if (totalWater > 0) _resourceManager.AddWater(totalWater);
        if (totalFood > 0) _resourceManager.AddFood(totalFood);
        if (totalMaterials > 0) _resourceManager.AddMaterials(totalMaterials);
        if (totalDiamonds > 0) _resourceManager.AddDiamonds(totalDiamonds);

        _resourceManager.SaveToJson();
    }

    private void ShowOfflineNotification(int hours)
    {
        string time = hours >= 24
            ? $"{hours / 24} дн. {hours % 24} ч."
            : $"{hours} ч.";
        Debug.Log($"[Offline] Вы отсутствовали {time}, ресурсы начислены");
        // Здесь подключи свой UI
    }

    public float GetHourProgress() => _hourTimer / HOUR_SECONDS;
    public float GetTimeUntilNextHour() => HOUR_SECONDS - _hourTimer;

    public string GetFormattedTimeRemaining()
    {
        float r = GetTimeUntilNextHour();
        return r >= 60
            ? $"{(int)(r / 60)} мин. {(int)(r % 60)} сек."
            : $"{(int)r} сек.";
    }
}