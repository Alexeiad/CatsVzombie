using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.IO;
using System;

public class ResourceCollector : MonoBehaviour
{
    [SerializeField] private CollectorDataSO _collectorDataSO;
    [SerializeField] private float _collectionInterval = 60f; // по умолчанию 1 минута

    [Header("Настройки интервала")]
    [SerializeField] private bool _useCustomInterval = false;
    [SerializeField] private int _hours = 0;
    [SerializeField] private int _minutes = 0;
    [SerializeField] private int _seconds = 1;

    [Inject] private ResourceManager _resourceManager;
    [Inject] private List<BuildingBehaviour> _buildingsBehaviours;

    private float _timer;
    private DateTime _lastSaveTime;
    private bool _isFirstLaunch = true;
    private float _actualInterval;

    private void Start()
    {
        InitializeInterval();
        LoadOfflineProgress();
        _lastSaveTime = DateTime.UtcNow;
    }

    private void InitializeInterval()
    {
        if (_useCustomInterval)
        {
            // Рассчитываем общее время в секундах
            _actualInterval = _hours * 3600 + _minutes * 60 + _seconds;
        }
        else
        {
            _actualInterval = _collectionInterval;
        }

        Debug.Log($"Интервал сбора установлен: {_actualInterval} секунд " +
                  $"({_actualInterval / 3600:0.##} часов, {_actualInterval / 60:0.##} минут)");
    }

    private void Update()
    {
        if (_actualInterval <= 0) return;

        _timer += Time.deltaTime;
        if (_timer >= _actualInterval)
        {
            _timer = 0f;
            CollectResources();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGameState();
        }
        else
        {
            LoadOfflineProgress();
            _lastSaveTime = DateTime.UtcNow;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveGameState();
        }
        else
        {
            LoadOfflineProgress();
            _lastSaveTime = DateTime.UtcNow;
        }
    }

    private void OnApplicationQuit()
    {
        SaveGameState();
    }

    private void SaveGameState()
    {
        // Сохраняем текущее время и прогресс таймера
        long currentTicks = DateTime.UtcNow.Ticks;
        PlayerPrefs.SetString("LastSaveTime", currentTicks.ToString());

        // Сохраняем текущее значение таймера
        PlayerPrefs.SetFloat("CurrentTimer", _timer);

        // Сохраняем прогресс зданий
        SaveBuildingsProgress();

        PlayerPrefs.Save();

        // Также сохраняем время локально
        _lastSaveTime = DateTime.UtcNow;
    }

    private void SaveBuildingsProgress()
    {
        foreach (var building in _buildingsBehaviours)
        {
            if (building != null)
            {
                string key = $"Building_{building.GetInstanceID()}_LastUpdate";
                PlayerPrefs.SetString(key, DateTime.UtcNow.Ticks.ToString());
            }
        }
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

        if (totalWater > 0) _resourceManager.AddWater(totalWater);
        if (totalFood > 0) _resourceManager.AddFood(totalFood);
        if (totalMaterials > 0) _resourceManager.AddMaterials(totalMaterials);
        if (totalDiamonds > 0) _resourceManager.AddDiamonds(totalDiamonds);

        _resourceManager.SaveToJson();
    }

    private void LoadOfflineProgress()
    {
        if (!PlayerPrefs.HasKey("LastSaveTime"))
        {
            _isFirstLaunch = true;
            return;
        }

        try
        {
            // Получаем время последнего сохранения
            long savedTicks = long.Parse(PlayerPrefs.GetString("LastSaveTime"));
            DateTime savedTime = new DateTime(savedTicks);
            DateTime currentTime = DateTime.UtcNow;

            // Получаем сохраненное значение таймера
            float savedTimer = PlayerPrefs.GetFloat("CurrentTimer", 0f);

            // Вычисляем прошедшее время в секундах
            TimeSpan timePassed = currentTime - savedTime;
            double totalSecondsPassed = timePassed.TotalSeconds + savedTimer;

            // Ограничиваем максимальное время оффлайн (например, 7 дней)
            double maxOfflineSeconds = 7 * 24 * 60 * 60; // 7 дней
            totalSecondsPassed = Math.Min(totalSecondsPassed, maxOfflineSeconds);

            if (totalSecondsPassed > 0 && _actualInterval > 0)
            {
                // Рассчитываем сколько полных циклов прошло
                int fullCycles = Mathf.FloorToInt((float)totalSecondsPassed / _actualInterval);

                // Рассчитываем остаток времени для текущего цикла
                float remainingTime = (float)totalSecondsPassed % _actualInterval;

                // Ограничиваем максимальное количество циклов для обработки
                int maxCycles = 1000; // Максимум 1000 циклов за раз
                fullCycles = Mathf.Min(fullCycles, maxCycles);

                if (fullCycles > 0)
                {
                    if (_isFirstLaunch)
                    {
                        Debug.Log($"Оффлайн прогресс: {timePassed.TotalHours:0.##} часов прошло, " +
                                 $"{fullCycles} полных циклов, остаток: {remainingTime:0.##} секунд");

                        // Здесь можно показать UI-уведомление
                        ShowOfflineRewardNotification(timePassed, fullCycles);
                    }

                    CollectResources(fullCycles);
                }

                // Устанавливаем таймер с учетом остатка времени
                _timer = remainingTime;

                Debug.Log($"Таймер установлен на {_timer:0.##} секунд (из {_actualInterval:0.##})");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка загрузки оффлайн прогресса: {e.Message}");
            _timer = 0f;
        }

        _isFirstLaunch = false;
    }

    private void ShowOfflineRewardNotification(TimeSpan timePassed, int cycles)
    {
        // Визуальное уведомление для игрока
        // Здесь можно реализовать UI-уведомление о полученных ресурсах
        Debug.Log($"Вы отсутствовали {FormatTimeSpan(timePassed)} и получили ресурсы за {cycles} циклов!");
    }

    private string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
            return $"{timeSpan.Days} дн. {timeSpan.Hours} ч.";
        if (timeSpan.TotalHours >= 1)
            return $"{timeSpan.Hours} ч. {timeSpan.Minutes} мин.";
        return $"{timeSpan.Minutes} мин. {timeSpan.Seconds} сек.";
    }

    // Метод для принудительного расчета оффлайн прогресса
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

    // Метод для получения текущего прогресса таймера (от 0 до 1)
    public float GetTimerProgress()
    {
        if (_actualInterval <= 0) return 0f;
        return _timer / _actualInterval;
    }

    // Метод для получения оставшегося времени до следующего сбора
    public float GetTimeRemaining()
    {
        return Mathf.Max(0, _actualInterval - _timer);
    }

    // Метод для получения форматированного оставшегося времени
    public string GetFormattedTimeRemaining()
    {
        float remaining = GetTimeRemaining();

        if (remaining >= 3600) // больше часа
            return $"{(int)(remaining / 3600)} ч. {(int)((remaining % 3600) / 60)} мин.";
        else if (remaining >= 60) // больше минуты
            return $"{(int)(remaining / 60)} мин. {(int)(remaining % 60)} сек.";
        else
            return $"{(int)remaining} сек.";
    }
}