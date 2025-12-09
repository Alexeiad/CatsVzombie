using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

public class BuildingsBuilder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private WindowBehaviour _windowBehaviour;
    [SerializeField] private ResourceValidator _resourceValidator;
    [SerializeField] private TilemapGridHighlighter _tileHighlighter;

    [Header("Building Configuration")]
    [SerializeField] private BuildingDatabase buildingDatabase;

    [Inject] private BuildingsList _bases;
    [Inject] private BaseSaveManager _baseSaveManager;

    private PlayerMovement _playerMovement;
    private GameObject _selectedBuilding;
    private Camera _camera;
    private Dictionary<Vector2Int, GameObject> builtObjects = new Dictionary<Vector2Int, GameObject>();

    private BuildingType _buildingType;
    private BuildingLevelType _buildingLevelType;
    private Vector2Int _gridPosition;
    private Vector3 _buildPosition;

    [Inject]
    private void Construct(PlayerMovement player)
    {
        _playerMovement = player;
        _camera = _playerMovement.GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        _bases = _baseSaveManager.GetBases();
        LoadSavedBuildings();
    }

    #region Public Methods

    public void SelectType(BuildingType buildingType, BuildingLevelType buildingLevelType)
    {
        _buildingType = buildingType;
        _buildingLevelType = buildingLevelType;
    }

    public void Deselect()
    {
        _selectedBuilding = null;
    }

    public void GetBuilding()
    {
        SelectBuilding();

        if (_selectedBuilding == null)
        {
            Debug.LogWarning("No building selected!");
            return;
        }

        CalculateBuildPosition();

        if (CanBuildHere(_gridPosition))
        {
            /*
            if (_resourceValidator != null)
            {
                if (!_resourceValidator.IsEnoughResourses(_buildingType, _buildingLevelType))
                {
                    // Можно добавить уведомление для игрока
                    Debug.Log("Недостаточно ресурсов для постройки!");
                    
                    return;
                }
            }*/

            BuildSelectedBuilding();
        }
        else
        {
            ShowBuildingInfoIfExists();
        }
    }

    public void BuildForLevelUp()
    {
        // Определяем целевой уровень для улучшения
        BuildingLevelType targetLevel;
        if (_buildingLevelType < BuildingLevelType.middle)
            targetLevel = BuildingLevelType.middle;
        else if (_buildingLevelType < BuildingLevelType.high)
            targetLevel = BuildingLevelType.high;
        else
            return; // Уже максимальный уровень

        /*
        if (_resourceValidator != null)
        {
            if (!_resourceValidator.IsEnoughResourses(_buildingType, targetLevel))
            {
                // Можно добавить уведомление для игрока
                Debug.Log("Недостаточно ресурсов для улучшения!");
                
                return;
            }
        }*/

        // Устанавливаем новый уровень
        _buildingLevelType = targetLevel;

        SelectBuilding();

        if (_selectedBuilding == null)
            return;

        // Используем текущую позицию для апгрейда
        Vector3Int tilePosition = tilemap.WorldToCell(_buildPosition);
        _gridPosition = new Vector2Int(tilePosition.x, tilePosition.y);
        _buildPosition = tilemap.GetCellCenterWorld(tilePosition);

        BuildSelectedBuilding();
    }

    public bool TryBuildImmediate(BuildingType buildingType, BuildingLevelType buildingLevelType, Vector3 worldPosition)
    {
        _buildingType = buildingType;
        _buildingLevelType = buildingLevelType;

        // Проверяем достаточно ли ресурсов
        if (_resourceValidator != null)
        {
            if (!_resourceValidator.IsEnoughResourses(buildingType, buildingLevelType))
            {
                Debug.Log($"Недостаточно ресурсов для постройки {buildingType} уровня {buildingLevelType}");
                return false;
            }
        }

        SelectBuilding();

        if (_selectedBuilding == null)
            return false;

        Vector3Int tilePosition = tilemap.WorldToCell(worldPosition);
        _gridPosition = new Vector2Int(tilePosition.x, tilePosition.y);
        _buildPosition = tilemap.GetCellCenterWorld(tilePosition);

        if (!CanBuildHere(_gridPosition))
            return false;

        BuildSelectedBuilding();
        return true;
    }

    public void DeleteBuilding()
    {
        Vector3Int tilePosition = tilemap.WorldToCell(_buildPosition);
        Vector2Int gridPosition = new Vector2Int(tilePosition.x, tilePosition.y);

        if (builtObjects.TryGetValue(gridPosition, out GameObject building))
        {
            var buildingBehaviour = building.GetComponent<BuildingBehaviour>();
            if (buildingBehaviour != null)
            {
                _buildingType = buildingBehaviour.baseType;
                _buildingLevelType = buildingBehaviour.levelType;
            }

            builtObjects.Remove(gridPosition);
            SaveBuildingData(_buildPosition, true);
            Destroy(building);
        }
    }

    public bool IsPositionOccupied(Vector2Int gridPosition)
    {
        return builtObjects.ContainsKey(gridPosition);
    }

    public bool CanAffordBuilding(BuildingType buildingType, BuildingLevelType buildingLevelType)
    {
 
        return _resourceValidator.IsEnoughResourses(buildingType, buildingLevelType);
    }

    #endregion

    #region Private Methods

    private void SelectBuilding()
    {
        _selectedBuilding = buildingDatabase.GetBuildingPrefab(_buildingType, _buildingLevelType);
        
    }

    private void CalculateBuildPosition()
    {
        Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int tilePosition = tilemap.WorldToCell(mouseWorldPos);
        _gridPosition = new Vector2Int(tilePosition.x, tilePosition.y);
        _buildPosition = tilemap.GetCellCenterWorld(tilePosition);
    }

    private void BuildSelectedBuilding()
    {
        // Списываем ресурсы перед постройкой
        if (_resourceValidator != null)
        {
            if (!_resourceValidator.IsEnoughResourses(_buildingType, _buildingLevelType))
            {
                Debug.LogError("Не удалось списать ресурсы для постройки!");
                return;
            }
        }

        GameObject newObject = Instantiate(_selectedBuilding, _buildPosition, Quaternion.identity);
        newObject.GetComponent<BuildingBehaviour>().Construct(_playerMovement);
        builtObjects[_gridPosition] = newObject;
        SaveBuildingData(newObject.transform.position, false);
    }

    private void ShowBuildingInfoIfExists()
    {
        if (builtObjects.TryGetValue(_gridPosition, out var building))
        {
            var buildingBehaviour = building.GetComponent<BuildingBehaviour>();
            if (buildingBehaviour != null)
            {
                _windowBehaviour.ShowImage(
                    buildingBehaviour.baseType,
                    buildingBehaviour.levelType,
                    CallbackType.Base
                );
            }
            _resourceValidator.CostInfo(_buildingType, _buildingLevelType);
        }
    }

    private void SaveBuildingData(Vector3 worldPosition, bool remove)
    {
        string buildingKey = buildingDatabase.GetSaveKey(_buildingType, _buildingLevelType);

        if (string.IsNullOrEmpty(buildingKey))
        {
            Debug.LogWarning($"No save key found for building type: {_buildingType}");
            return;
        }

        if (remove)
        {
            _baseSaveManager.RemoveBase(buildingKey, worldPosition);
        }
        else
        {
            var newData = new BuildingsSaveData(buildingKey, worldPosition);
            _baseSaveManager.AddBase(newData);
        }

        _baseSaveManager.SaveNow();
    }

    private void LoadSavedBuildings()
    {
        // Очищаем существующие постройки
        ClearExistingBuildings();

        // Загружаем сохраненные здания
        foreach (BuildingsSaveData baseData in _bases)
        {
            GameObject buildingPrefab = buildingDatabase.GetPrefabBySaveKey(baseData.ID);

            if (buildingPrefab == null)
            {
                Debug.LogWarning($"No prefab found for save key: {baseData.ID}");
                continue;
            }

            Vector3Int tilePos = tilemap.WorldToCell(baseData.Position);
            Vector2Int gridPos = new Vector2Int(tilePos.x, tilePos.y);

            if (!builtObjects.ContainsKey(gridPos))
            {
                var newBuilding = Instantiate(buildingPrefab, baseData.Position, Quaternion.identity, null);
                var buildingBehaviour = newBuilding.GetComponent<BuildingBehaviour>();

                if (buildingBehaviour != null)
                {
                    buildingBehaviour.Construct(_playerMovement);
                }

                builtObjects[gridPos] = newBuilding;
            }
            else
            {
                Debug.LogWarning($"Position {gridPos} already occupied, skipping building: {baseData.ID}");
            }
        }
    }

    private void ClearExistingBuildings()
    {
        foreach (var builtObject in builtObjects.Values)
        {
            if (builtObject != null)
                Destroy(builtObject);
        }
        builtObjects.Clear();
    }

    private bool CanBuildHere(Vector2Int gridPosition)
    {
        // Проверяем, что позиция не занята другим зданием
        if (builtObjects.ContainsKey(gridPosition))
            return false;

        // Проверяем, что в этой клетке tilemap есть тайл
        Vector3Int tilePosition = new Vector3Int(gridPosition.x, gridPosition.y, 0);
        return tilemap.HasTile(tilePosition);
    }

    #endregion

    #region Gizmos (опционально)

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        // Визуализация сетки построек
        Gizmos.color = Color.yellow;

        foreach (var kvp in builtObjects)
        {
            if (kvp.Value != null)
            {
                Gizmos.DrawWireCube(kvp.Value.transform.position, Vector3.one * 0.8f);
            }
        }
    }

    #endregion
}


[Serializable]
public class BuildingTiers
{
    [SerializeField] private GameObject tier1;
    [SerializeField] private GameObject tier2;

    public GameObject Tier1 => tier1;
    public GameObject Tier2 => tier2;

    public GameObject GetTier(BuildingLevelType levelType)
    {
        return levelType switch
        {
            BuildingLevelType.low => tier1,
            BuildingLevelType.middle => tier2,
            _ => tier1
        };
    }
}

[Serializable]
public class BuildingConfigEntry
{
    [SerializeField] private BuildingType buildingType;
    [SerializeField] private BuildingTiers tiers;
    [SerializeField] private string saveKey;

    public BuildingType BuildingType => buildingType;
    public BuildingTiers Tiers => tiers;
    public string SaveKey => saveKey;
}

[CreateAssetMenu(fileName = "BuildingDatabase", menuName = "")]
public class BuildingDatabase : ScriptableObject
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