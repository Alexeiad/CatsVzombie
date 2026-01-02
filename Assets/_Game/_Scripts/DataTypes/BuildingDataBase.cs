using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingDatabase", menuName = "")]
public class BuildingDataBase : ScriptableObject
{
    [SerializeField] private List<BuildingConfigEntry> buildingEntries = new List<BuildingConfigEntry>();

    private Dictionary<BuildingType, BuildingConfigEntry> _buildingDictionary;
    private Dictionary<string, BuildingConfigEntry> _saveKeyDictionary;

    public GameObject GetBuildingPrefab(BuildingType type, BuildingLevelType levelType)
    {
        InitializeIfNeeded();

        if (_buildingDictionary.TryGetValue(type, out var entry))
        {
            return entry.Tiers.GetTier(levelType);
        }

        Debug.LogWarning($"No building found for type: {type}");
        return null;
    }

    public string GetSaveKey(BuildingType type, BuildingLevelType levelType)
    {
        InitializeIfNeeded();

        if (_buildingDictionary.TryGetValue(type, out var entry))
        {
            return GetLevelSaveKey(entry.SaveKey, levelType);
        }

        Debug.LogWarning($"No save key found for type: {type}");
        return string.Empty;
    }

    public GameObject GetPrefabBySaveKey(string saveKey)
    {
        InitializeIfNeeded();

        // Пытаемся найти точное совпадение (уровень 2)
        if (_saveKeyDictionary.TryGetValue(saveKey, out var entry))
        {
            return GetPrefabByFullSaveKey(saveKey);
        }

        // Если точное совпадение не найдено, ищем базовый ключ (уровень 1)
        foreach (var kvp in _buildingDictionary)
        {
            var baseKey = kvp.Value.SaveKey;
            if (saveKey == baseKey || saveKey.StartsWith(baseKey))
            {
                return GetPrefabByFullSaveKey(saveKey);
            }
        }

        Debug.LogWarning($"No prefab found for save key: {saveKey}");
        return null;
    }

    private GameObject GetPrefabByFullSaveKey(string saveKey)
    {
        foreach (var entry in buildingEntries)
        {
            // Проверяем базовый ключ (уровень 1)
            if (saveKey == entry.SaveKey)
                return entry.Tiers.Tier1;

            // Проверяем ключ с суффиксом (уровень 2)
            if (saveKey == entry.SaveKey + "_Middle")
                return entry.Tiers.Tier2;
        }

        return null;
    }

    private string GetLevelSaveKey(string baseKey, BuildingLevelType levelType)
    {
        return levelType switch
        {
            BuildingLevelType.low => baseKey,
            BuildingLevelType.middle => baseKey + "_Middle",
            _ => baseKey
        };
    }

    private void InitializeIfNeeded()
    {
        if (_buildingDictionary != null && _saveKeyDictionary != null)
            return;

        _buildingDictionary = new Dictionary<BuildingType, BuildingConfigEntry>();
        _saveKeyDictionary = new Dictionary<string, BuildingConfigEntry>();

        foreach (var entry in buildingEntries)
        {
            _buildingDictionary[entry.BuildingType] = entry;

            // Добавляем оба варианта ключей в словарь
            _saveKeyDictionary[entry.SaveKey] = entry;
            _saveKeyDictionary[entry.SaveKey + "_Middle"] = entry;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // В редакторе переинициализируем при изменении
        _buildingDictionary = null;
        _saveKeyDictionary = null;
    }
#endif
}