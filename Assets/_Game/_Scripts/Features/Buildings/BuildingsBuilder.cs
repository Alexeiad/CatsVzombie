using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Zenject;

public class BuildingsBuilder : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;

    [SerializeField] private List<EventTrigger> _eventTriggers;

    [SerializeField] private GameObject aquarium, barrel, workbench, tire, headquarters, medUnit, barrak;


    private Dictionary<Vector2Int, GameObject> builtObjects = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<string, GameObject> _objectMap;

    private GameObject _selectedBuilding;
    private Camera _camera;

    private PlayerMovement _playerMovement;
   
    private BuildingsList _bases;
   
    private BuildingType _buildingType;
    private BuildingLevelType _buildingLevelType;

    private bool _isBuildPanel;

    private string
        _AquariumKey = "AquariumKey",
        _BarrelKey = "BarrelKey",
        _WorkbenchKey = "WorkbenchKey",
        _TireKey = "TireKey",
        _HeadquartersKey = "HeadquartersKey",
        _MedUnitKey = "MedUnitKey",
        _BarrakKey = "BarrakKey";

    [Inject]
    private void Construct(PlayerMovement player)
    {
        _playerMovement = player;
        _camera = _playerMovement.GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        _bases = BaseSaveManager.Bases;

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

        foreach (BuildingsSaveData baseData in _bases)
        {
            if (_objectMap.TryGetValue(baseData.ID, out var building))
            {
                var newBuilding = Instantiate(building, baseData.Position, Quaternion.identity);

                Vector3Int tilePos = tilemap.WorldToCell(baseData.Position);
                Vector2Int gridPos = new Vector2Int(tilePos.x, tilePos.y);

                builtObjects[gridPos] = newBuilding;
            }
        }
        _eventTriggers.ForEach(et => AddEventTriggerListener(et, EventTriggerType.PointerDown, OnClick));
    }

    void Update()
    {
        Select(_buildingType);
        Build(_selectedBuilding);
    }

    private void Select(BuildingType baseType)
    {
        BuildingTypeCheck(out _selectedBuilding, new List<GameObject>
        { aquarium, barrel, workbench, tire, headquarters, medUnit, barrak });
    }

    private void BuildingTypeCheck<T>(out T check, List<T> set)
    {
        check = _buildingType switch
        {
            BuildingType.Aquarium => set[0],
            BuildingType.Barrel => set[1],
            BuildingType.Workbench => set[2],
            BuildingType.Tire => set[3],
            BuildingType.Headquarters => set[4],
            BuildingType.MedUnit => set[5],
            BuildingType.Barrak => set[6],
            _ => set[0]
        };
    }
    private void BuildingLevelTypeCheck<T>(out T check, List<T> set)
    {
        check = _buildingLevelType switch
        {
            BuildingLevelType.low => set[0],
            BuildingLevelType.middle => set[1],
            BuildingLevelType.high => set[2],
            _ => set[0]
        };
    }

    private void Build(GameObject selectedBuildings)
    {
        if (selectedBuildings != null)
        {
            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tilePosition = tilemap.WorldToCell(mouseWorldPos);
            Vector2Int gridPosition = new Vector2Int(tilePosition.x, tilePosition.y);

            Vector3 buildPosition = tilemap.GetCellCenterWorld(tilePosition);

            if (CanBuildHere(gridPosition) && _isBuildPanel)
            {
                GameObject newObject = Instantiate(selectedBuildings, buildPosition, Quaternion.identity);
                builtObjects[gridPosition] = newObject;

                SaveBuildingData(gridPosition, newObject.transform.position);
            }
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectObject();
        }
    }

    private void OnClick(BaseEventData data)
    {
        GameObject clickedObject = ((PointerEventData)data).pointerCurrentRaycast.gameObject;
        if (clickedObject != null)
        {
            var buttonForConstruction = clickedObject.GetComponent<ButtonForConstruction>();

            _buildingType = buttonForConstruction.buildingType;
        }
    }

    private void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }

    private void SaveBuildingData(Vector2Int gridPosition, Vector3 worldPosition)
    {
        BuildingTypeCheck(out string buildingKey, new List<string>
        { _AquariumKey,_BarrelKey, _WorkbenchKey,_TireKey, _HeadquartersKey, _MedUnitKey, _BarrakKey });

        if (!string.IsNullOrEmpty(buildingKey))
        {
            _bases.Add(new BuildingsSaveData(buildingKey, worldPosition));
        }
    }

    public void BuildedOn()
    {
        _isBuildPanel = true;
    }

    public void BuildedOff()
    {
        _isBuildPanel = false;
    }

    bool CanBuildHere(Vector2Int gridPosition)
    {
        if (builtObjects.ContainsKey(gridPosition))
            return false;

        return true;
    }

    public void DeselectObject()
    {
        _selectedBuilding = null;
    }

    void OnDrawGizmos()
    {
        if (_camera != null && _selectedBuilding != null)
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