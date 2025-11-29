using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

public class BuildingsBuilder : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private WindowBehaviour _windowBehaviour;

    [SerializeField] private GameObject aquarium, barrel, workbench, tire, headquarters, medUnit, barrak;
    [SerializeField] private GameObject aquariumTwo, barrelTwo, workbenchTwo, tireTwo, headquartersTwo, medUnitTwo, barrakTwo;
    [SerializeField] private GameObject aquariumThree, barrelThree, workbenchThree, tireThree, headquartersThree, medUnitThree, barrakThree;

    private Dictionary<Vector2Int, GameObject> builtObjects = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<string, GameObject> _objectMap;

    private GameObject _selectedBuilding;

    private Camera _camera;
    private Vector3 _mouseWorldPos;

    private PlayerMovement _playerMovement;

    private Vector2Int _gridPosition;
    private Vector3 _buildPosition;

    [Inject] private BuildingsList _bases;
    [Inject] private BaseSaveManager _baseSaveManager;

    private BuildingType _buildingType;
    private BuildingLevelType _buildingLevelType;

    private string
        _AquariumKey = "AquariumKey",
        _BarrelKey = "BarrelKey",
        _WorkbenchKey = "WorkbenchKey",
        _TireKey = "TireKey",
        _HeadquartersKey = "HeadquartersKey",
        _MedUnitKey = "MedUnitKey",
        _BarrakKey = "BarrakKey";
    private string
        _Middle = "Middle",
        _High = "High";

    public void BuildForLevelUp()
    {
        if ((int)_buildingLevelType < Enum.GetValues(typeof(BuildingLevelType)).Length - 1)
            _buildingLevelType++;

        Select();

        _mouseWorldPos = _buildPosition;

        Vector3Int tilePosition = tilemap.WorldToCell(_mouseWorldPos);

        _gridPosition = new Vector2Int(tilePosition.x, tilePosition.y);
        _buildPosition = tilemap.GetCellCenterWorld(tilePosition);

        GameObject newObject = Instantiate(_selectedBuilding, _buildPosition, Quaternion.identity);
        newObject.GetComponent<BuildingBehaviour>().Construct(_playerMovement);

        builtObjects[_gridPosition] = newObject;
        SaveBuildingData(newObject.transform.position, false);
    }

    public void SelectType(BuildingType buildingType, BuildingLevelType buildingLevelType)
    {
        _buildingType = buildingType;
        _buildingLevelType = buildingLevelType;
    }

    public void Deselect()
    {
        _selectedBuilding = null;
    }

    public void DeleteBuilding()
    {
        Vector3Int tilePosition = tilemap.WorldToCell(_buildPosition);
        Vector2Int gridPosition = new Vector2Int(tilePosition.x, tilePosition.y);

        if (builtObjects.TryGetValue(gridPosition, out GameObject building))
        {
            // Получаем тип и уровень здания перед удалением
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

    public void GetBuilding()
    {
        Select();

        if (_selectedBuilding == null)
        {
            return;
        }

        Build();

        if (CanBuildHere(_gridPosition))
        {
            GameObject newObject = Instantiate(_selectedBuilding, _buildPosition, Quaternion.identity);
            newObject.GetComponent<BuildingBehaviour>().Construct(_playerMovement);

            builtObjects[_gridPosition] = newObject;
            SaveBuildingData(newObject.transform.position, false);
        }
        else
        {
            // Проверяем, есть ли здание на этой позиции
            if (builtObjects.TryGetValue(_gridPosition, out var building))
            {
                var buildingBehaviour = building.GetComponent<BuildingBehaviour>();
                if (buildingBehaviour != null)
                {
                    var type = buildingBehaviour.baseType;
                    var levelType = buildingBehaviour.levelType;
                    _windowBehaviour.ShowImage(type, levelType, CallbackType.Base);
                }
            }
        }
    }

    [Inject]
    private void Construct(PlayerMovement player)
    {
        _playerMovement = player;
        _camera = _playerMovement.GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        _bases = _baseSaveManager.GetBases();

        _objectMap = new Dictionary<string, GameObject>
        {
            { _AquariumKey, aquarium },
            { _BarrelKey, barrel },
            { _WorkbenchKey, workbench },
            { _TireKey, tire },
            { _HeadquartersKey, headquarters },
            { _MedUnitKey, medUnit },
            { _BarrakKey, barrak },

            { _AquariumKey+_Middle, aquariumTwo },
            { _BarrelKey+_Middle, barrelTwo },
            { _WorkbenchKey+_Middle, workbenchTwo },
            { _TireKey+_Middle, tireTwo },
            { _HeadquartersKey+_Middle, headquartersTwo },
            { _MedUnitKey + _Middle, medUnitTwo },
            { _BarrakKey + _Middle, barrakTwo },

            { _AquariumKey+_High, aquariumThree },
            { _BarrelKey+_High, barrelThree },
            { _WorkbenchKey+_High, workbenchThree },
            { _TireKey+_High, tireThree },
            { _HeadquartersKey+_High, headquartersThree },
            { _MedUnitKey + _High, medUnitThree },
            { _BarrakKey + _High, barrakThree }
        };

        LoadSavedBuildings();
    }

    private void LoadSavedBuildings()
    {
        // Очищаем существующие постройки перед загрузкой
        foreach (var builtObject in builtObjects.Values)
        {
            if (builtObject != null)
                Destroy(builtObject);
        }
        builtObjects.Clear();

        // Загружаем сохраненные здания
        foreach (BuildingsSaveData baseData in _bases)
        {
            if (_objectMap.TryGetValue(baseData.ID, out var buildingPrefab))
            {
                Vector3Int tilePos = tilemap.WorldToCell(baseData.Position);
                Vector2Int gridPos = new Vector2Int(tilePos.x, tilePos.y);

                // Проверяем, не занята ли уже эта позиция
                if (!builtObjects.ContainsKey(gridPos))
                {
                    var newBuilding = Instantiate(buildingPrefab, baseData.Position, Quaternion.identity, null);
                    newBuilding.GetComponent<BuildingBehaviour>().Construct(_playerMovement);
                    builtObjects[gridPos] = newBuilding;
                }
                else
                {
                    Debug.LogWarning($"Position {gridPos} already occupied, skipping building: {baseData.ID}");
                }
            }
            else
            {
                Debug.LogWarning($"No prefab found for building ID: {baseData.ID}");
            }
        }
    }

    private void Select()
    {
        var buildings = new List<GameObject>
        {
            aquarium, barrel, workbench, tire, headquarters, medUnit, barrak,
            aquariumTwo, barrelTwo, workbenchTwo, tireTwo, headquartersTwo, medUnitTwo, barrakTwo,
            aquariumThree, barrelThree, workbenchThree, tireThree, headquartersThree, medUnitThree, barrakThree
        };

        var indexer = new BuildingComparator<BuildingType, BuildingLevelType, GameObject>(buildings);
        _selectedBuilding = indexer.GetValue(_buildingType, _buildingLevelType);
    }

    private void Build()
    {
        _mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
        _mouseWorldPos.z = 0; // Важно: обнуляем Z-координату

        Vector3Int tilePosition = tilemap.WorldToCell(_mouseWorldPos);
        _gridPosition = new Vector2Int(tilePosition.x, tilePosition.y);
        _buildPosition = tilemap.GetCellCenterWorld(tilePosition);
    }

    private void SaveBuildingData(Vector3 worldPosition, bool remove)
    {
        var buildingList = new List<string>
        {
            _AquariumKey, _BarrelKey, _WorkbenchKey, _TireKey, _HeadquartersKey, _MedUnitKey, _BarrakKey,
            _AquariumKey+_Middle, _BarrelKey + _Middle, _WorkbenchKey + _Middle, _TireKey + _Middle, _HeadquartersKey + _Middle, _MedUnitKey + _Middle, _BarrakKey + _Middle,
            _AquariumKey + _High, _BarrelKey + _High, _WorkbenchKey + _High, _TireKey + _High, _HeadquartersKey + _High, _MedUnitKey + _High, _BarrakKey + _High
        };

        var indexer = new BuildingComparator<BuildingType, BuildingLevelType, string>(buildingList);
        var buildingKey = indexer.GetValue(_buildingType, _buildingLevelType);

        if (!string.IsNullOrEmpty(buildingKey))
        {
            if (remove)
            {
                // Удаляем по точной позиции
                _baseSaveManager.RemoveBase(buildingKey, worldPosition);
            }
            else
            {
                // Добавляем новое здание
                var newData = new BuildingsSaveData(buildingKey, worldPosition);
                _baseSaveManager.AddBase(newData);
            }

            // Сохраняем изменения
            _baseSaveManager.SaveNow();
        }
    }

    private bool CanBuildHere(Vector2Int gridPosition)
    {
        // Проверяем, что позиция не занята другим зданием
        if (builtObjects.ContainsKey(gridPosition))
            return false;

        // Проверяем, что в этой клетке tilemap есть тайл (текстура)
        Vector3Int tilePosition = new Vector3Int(gridPosition.x, gridPosition.y, 0);
        return tilemap.HasTile(tilePosition);
    }

    void OnDrawGizmos()
    {
        if (_camera != null && _selectedBuilding != null)
        {
            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0; // Обнуляем Z-координату
            Vector3Int tilePosition = tilemap.WorldToCell(mouseWorldPos);
            Vector2Int gridPosition = new Vector2Int(tilePosition.x, tilePosition.y);
            Vector3 buildPosition = tilemap.GetCellCenterWorld(tilePosition);

            Gizmos.color = CanBuildHere(gridPosition) ? Color.green : Color.red;
            Gizmos.DrawWireCube(buildPosition, Vector3.one * 0.8f);
        }
    }

    // Метод для отладки - показывает все занятые позиции
    private void DebugBuiltObjects()
    {
        Debug.Log($"Total built objects: {builtObjects.Count}");
        foreach (var kvp in builtObjects)
        {
            Debug.Log($"Position: {kvp.Key}, Object: {kvp.Value.name}");
        }
    }
}