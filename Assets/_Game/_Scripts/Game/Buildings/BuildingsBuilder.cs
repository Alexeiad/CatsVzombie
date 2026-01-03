using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Zenject;

public class BuildingsBuilder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private WindowBehaviour _windowBehaviour;
    [SerializeField] private ResourceValidator _resourceValidator;
    [SerializeField] private TilemapGridHighlighter _tileHighlighter;
    [SerializeField] private bool _hasNotBaseLevel;

    [Header("Building Configuration")]
    [SerializeField] private BuildingDataBase buildingDatabase;

    [Inject] private BuildingsList _bases;
    [Inject] private BaseSaveManager _baseSaveManager;
    [Inject] private List<BuildingBehaviour> _buildingBehaviours;

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
    public void BuildBuilding()
    {
        GetBuilding();
    }
    public void GetBuilding()
    {
        SelectBuilding();

        CalculateBuildPosition();

        if (CanBuildHere(_gridPosition))
        {
         
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
            _buildingBehaviours.Remove(buildingBehaviour);
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
        if (Pointer.current == null) return;

        Vector3 inputPosition = Pointer.current.position.ReadValue();
        Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(inputPosition);
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
        _buildingBehaviours.Add(newObject.GetComponent<BuildingBehaviour>());
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
                if (_hasNotBaseLevel)
                {
                    newBuilding.transform.position=new Vector3(0,float.MaxValue,0);
                }

                if (buildingBehaviour != null)
                {
                    buildingBehaviour.Construct(_playerMovement);
                    _buildingBehaviours.Add(buildingBehaviour);
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

