
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




    public void SelectType(BuildingType buildingType, BuildingLevelType buildingLevelType)
    {
        _buildingType = buildingType;
        _buildingLevelType = buildingLevelType;
    }
    public void Deselect()
    {
        _selectedBuilding = null;

    }
    public void DeleteBuilding(BuildingType buildingType)
    {
        var building = builtObjects[_gridPosition];

        builtObjects.Remove(_gridPosition);

        SaveBuildingData((Vector2)_gridPosition, true);

        Destroy(building);
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
            var building= builtObjects[_gridPosition];
            var type = building.GetComponent<BuildingBehaviour>().baseType;
            var levelType = building.GetComponent<BuildingBehaviour>().levelType;

            _windowBehaviour.ShowImage(type, levelType, CallbackType.Base);
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

        Vector3Int tilePosition = tilemap.WorldToCell(_mouseWorldPos);

        _gridPosition = new Vector2Int(tilePosition.x, tilePosition.y);
        _buildPosition = tilemap.GetCellCenterWorld(tilePosition);

    }




    private void SaveBuildingData(Vector3 worldPosition,bool remove)
    {
        var buildingList = new List<string>
        { _AquariumKey,_BarrelKey, _WorkbenchKey,_TireKey, _HeadquartersKey, _MedUnitKey, _BarrakKey, 
        _AquariumKey+_Middle,_BarrelKey + _Middle, _WorkbenchKey + _Middle,_TireKey + _Middle, _HeadquartersKey + _Middle, _MedUnitKey + _Middle, _BarrakKey + _Middle,
        _AquariumKey + _High,_BarrelKey + _High, _WorkbenchKey + _High,_TireKey + _High, _HeadquartersKey + _High, _MedUnitKey + _High, _BarrakKey + _High};

        var indexer = new BuildingComparator<BuildingType, BuildingLevelType, string>(buildingList);

        var buildingKey = indexer.GetValue(_buildingType, _buildingLevelType);

        if (!string.IsNullOrEmpty(buildingKey)&&!remove)
        {
            _bases.Add(new BuildingsSaveData(buildingKey, worldPosition));
        }
        if (!string.IsNullOrEmpty(buildingKey) && remove)
        {
            _bases.Remove(new BuildingsSaveData(buildingKey, worldPosition));
            _baseSaveManager.RemoveBaseById(buildingKey);
        }
    }
    

   
    bool CanBuildHere(Vector2Int gridPosition)
    {
        if (builtObjects.ContainsKey(gridPosition))
            return false;

        return true;
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