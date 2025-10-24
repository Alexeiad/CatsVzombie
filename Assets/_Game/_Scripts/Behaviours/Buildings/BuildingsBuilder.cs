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
    [SerializeField] private GameObject aquariumTwo, barrelTwo, workbenchTwo, tireTwo, headquartersTwo, medUnitTwo, barrakTwo;
    [SerializeField] private GameObject aquariumThree, barrelThree, workbenchThree, tireThree, headquartersThree, medUnitThree, barrakThree;

    private Dictionary<Vector2Int, GameObject> builtObjects = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<string, GameObject> _objectMap;

    private GameObject _selectedBuilding;
    private Camera _camera;

    private PlayerMovement _playerMovement;
   
    [Inject] private BuildingsList _bases;
    [Inject] private BaseSaveManager _baseSaveManager;

    private DiContainer _diContainer;
   
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
    private string

        _Middle = "Middle",
        _High= "High";
        

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

        foreach (BuildingsSaveData baseData in _bases)
        {
            if (_objectMap.TryGetValue(baseData.ID, out var building))
            {
                var newBuilding = Instantiate(building, baseData.Position, Quaternion.identity,null);

                newBuilding.GetComponent<BuildingBehaviour>().Construct(_playerMovement);

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
        var buildingList = new List<GameObject>
        {
            aquarium, barrel, workbench, tire, headquarters, medUnit, barrak,
            aquariumTwo, barrelTwo, workbenchTwo, tireTwo, headquartersTwo, medUnitTwo, barrakTwo,
            aquariumThree, barrelThree, workbenchThree, tireThree, headquartersThree, medUnitThree, barrakThree
        };

        var indexer = new EnumDoubleIndexer<BuildingType, BuildingLevelType, GameObject>(buildingList);
        _selectedBuilding = indexer.GetValue(_buildingType, _buildingLevelType);
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
                newObject.GetComponent<BuildingBehaviour>().Construct(_playerMovement);

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
        var buildingList = new List<string>
        { _AquariumKey,_BarrelKey, _WorkbenchKey,_TireKey, _HeadquartersKey, _MedUnitKey, _BarrakKey, 
        _AquariumKey+_Middle,_BarrelKey + _Middle, _WorkbenchKey + _Middle,_TireKey + _Middle, _HeadquartersKey + _Middle, _MedUnitKey + _Middle, _BarrakKey + _Middle,
        _AquariumKey + _High,_BarrelKey + _High, _WorkbenchKey + _High,_TireKey + _High, _HeadquartersKey + _High, _MedUnitKey + _High, _BarrakKey + _High};

        var indexer = new EnumDoubleIndexer<BuildingType, BuildingLevelType, string>(buildingList);

        var buildingKey = indexer.GetValue(_buildingType, _buildingLevelType);

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