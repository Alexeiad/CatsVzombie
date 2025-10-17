using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Zenject;

public class BuildingsBuilder : MonoBehaviour
{
    public Tilemap tilemap; // Ссылка на ваш тайлмап

    [SerializeField] private GameObject aquarium, barrel, workbench, tire, headquarters, medUnit, barrak;

    private Dictionary<Vector2Int, GameObject> builtObjects = new Dictionary<Vector2Int, GameObject>();
    private GameObject selectedObjectType;
    private GameObject _instantiateObject;

    private PlayerMovement _playerMovement;
    private Camera _camera;

    private string
        _AquariumKey = "AquariumKey",
        _BarrelKey = "BarrelKey",
        _WorkbenchKey = "WorkbenchKey",
        _TireKey = "TireKey",
        _HeadquartersKey = "HeadquartersKey",
        _MedUnitKey = "MedUnitKey",
        _BarrakKey = "BarrakKey";

    private BaseList _bases;
    private BuildingType _baseType;
    private DiContainer _diContainer;
    private bool isBuildPanel;
    private Dictionary<string, GameObject> _objectMap;

    [Inject]
    private void Construct(PlayerMovement player)
    {
        _playerMovement = player;
        _camera = _playerMovement.GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        _bases = BaseLoadSaveData.Bases;

        _objectMap = new Dictionary<string, GameObject>
        {
            { _AquariumKey, aquarium },
            { _BarrelKey, barrel },
            { _WorkbenchKey, workbench },
            { _TireKey, tire },
            { _HeadquartersKey, headquarters },
            { _MedUnitKey, medUnit },
            { _BarrakKey, barrak }
        };

        foreach (BaseElementsSaveData baseData in _bases)
        {
            if (_objectMap.TryGetValue(baseData.ID, out _instantiateObject))
            {
                var go = Instantiate(_instantiateObject, baseData.Position, Quaternion.identity);

                Vector3Int tilePos = tilemap.WorldToCell(baseData.Position);
                Vector2Int gridPos = new Vector2Int(tilePos.x, tilePos.y);

                builtObjects[gridPos] = go;
                Debug.Log(builtObjects.Count);
            }
        }
    }

    void Update()
    {
        if (selectedObjectType != null)
        {
            // Получаем позицию клика и преобразуем в позицию на тайлмапе
            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tilePosition = tilemap.WorldToCell(mouseWorldPos);
            Vector2Int gridPosition = new Vector2Int(tilePosition.x, tilePosition.y);

            // Получаем позицию в центре тайла
            Vector3 buildPosition = tilemap.GetCellCenterWorld(tilePosition);

            // Проверяем, можно ли строить
            if (CanBuildHere(gridPosition) && isBuildPanel)
            {
                // Спавним объект
                GameObject newObject = Instantiate(selectedObjectType, buildPosition, Quaternion.identity);
                builtObjects[gridPosition] = newObject;

                // Сохраняем данные о постройке
                SaveBuildingData(gridPosition, newObject.transform.position);
            }
        }

        // Отмена выбора по правой кнопке мыши или Escape
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectObject();
        }
    }

    private void SaveBuildingData(Vector2Int gridPosition, Vector3 worldPosition)
    {
        string buildingKey = _baseType switch
        {
            BuildingType.Aquarium => _AquariumKey,
            BuildingType.Barrel => _BarrelKey,
            BuildingType.Workbench => _WorkbenchKey,
            BuildingType.Tire => _TireKey,
            BuildingType.Headquarters => _HeadquartersKey,
            BuildingType.MedUnit => _MedUnitKey,
            BuildingType.Barrak => _BarrakKey,
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(buildingKey))
        {
            _bases.Add(new BaseElementsSaveData(buildingKey, worldPosition));
        }
    }

    public void BuildedOn()
    {
        isBuildPanel = true;
    }

    public void BuildedOff()
    {
        isBuildPanel = false;
    }

    bool CanBuildHere(Vector2Int gridPosition)
    {
        // Проверяем, нет ли здесь другого объекта
        if (builtObjects.ContainsKey(gridPosition))
            return false;

        // Можно добавить другие проверки (доступность тайла и т.д.)
        return true;
    }

    // Методы для выбора объекта для строительства (вызывайте из UI)
    public void SelectAquarium()
    {
        selectedObjectType = aquarium;
        _baseType = BuildingType.Aquarium;
    }

    public void SelectBarrel()
    {
        selectedObjectType = barrel;
        _baseType = BuildingType.Barrel;
    }

    public void SelectWorkbench()
    {
        selectedObjectType = workbench;
        _baseType = BuildingType.Workbench;
    }

    public void SelectTire()
    {
        selectedObjectType = tire;
        _baseType = BuildingType.Tire;
    }

    public void SelectHeadquarters()
    {
        selectedObjectType = headquarters;
        _baseType = BuildingType.Headquarters;
    }

    public void SelectMedicalUnit()
    {
        selectedObjectType = medUnit;
        _baseType = BuildingType.MedUnit;
    }

    public void SelectBarrak()
    {
        selectedObjectType = barrak;
        _baseType = BuildingType.Barrak;
    }

    public void DeselectObject()
    {
        selectedObjectType = null;
    }

    // Визуализация выбранной позиции (опционально)
    void OnDrawGizmos()
    {
        if (_camera != null && selectedObjectType != null)
        {
            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tilePosition = tilemap.WorldToCell(mouseWorldPos);
            Vector2Int gridPosition = new Vector2Int(tilePosition.x, tilePosition.y);
            Vector3 buildPosition = tilemap.GetCellCenterWorld(tilePosition);

            Gizmos.color = CanBuildHere(gridPosition) ? Color.green : Color.red;
            Gizmos.DrawWireCube(buildPosition, Vector3.one * 0.8f);
        }
    }
}