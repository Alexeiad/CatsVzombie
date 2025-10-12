using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Zenject;

public class BuildingsBuilder : MonoBehaviour
{
    public Tilemap tilemap; // Ссылка на ваш тайлмап
   

    [SerializeField] private GameObject aquarium,barrel,workbench,tire,headquarters,medUnit,barrak;


    private GameObject selectedObjectType;
    private GameObject _instantiateObject;
    private Dictionary<Vector3Int, GameObject> builtObjects = new Dictionary<Vector3Int, GameObject>();

    private PlayerMovement _playerMovement;
    private Camera _camera;

    private string 
        _AquariumKey= "AquariumKey",
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
        _bases=BaseLoadSaveData.Bases;

        foreach (BaseElementsSaveData baseData in _bases)
        {
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
            if (_objectMap.TryGetValue(baseData.ID, out _instantiateObject))
            {
                var go = Instantiate(_instantiateObject, baseData.Position, Quaternion.identity);
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

            // Получаем позицию в центре тайла
            Vector3 buildPosition = tilemap.GetCellCenterWorld(tilePosition);

            // Проверяем, можно ли строить
            if (CanBuildHere(tilePosition) && isBuildPanel)
            {
                // Спавним объект
                GameObject newObject = Instantiate(selectedObjectType, buildPosition + new Vector3(0, 0, 10f),
                    Quaternion.identity);

                builtObjects[tilePosition] = newObject;



                if (_baseType == BuildingType.Aquarium)
                    _bases.Add(new BaseElementsSaveData(_AquariumKey, newObject.transform.position));
                if (_baseType == BuildingType.Barrel)
                    _bases.Add(new BaseElementsSaveData(_BarrelKey, newObject.transform.position));
                if (_baseType == BuildingType.Workbench)
                    _bases.Add(new BaseElementsSaveData(_WorkbenchKey, newObject.transform.position));
                if (_baseType == BuildingType.Tire)
                    _bases.Add(new BaseElementsSaveData(_TireKey, newObject.transform.position));
                if (_baseType == BuildingType.Headquarters)
                    _bases.Add(new BaseElementsSaveData(_HeadquartersKey, newObject.transform.position));
                if (_baseType == BuildingType.MedUnit)
                    _bases.Add(new BaseElementsSaveData(_MedUnitKey, newObject.transform.position));
                if (_baseType == BuildingType.Barrak)
                    _bases.Add(new BaseElementsSaveData(_BarrakKey, newObject.transform.position));
            }
        }




        // Отмена выбора по правой кнопке мыши или Escape
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectObject();
        }
    }
    public void BuildedOn()
    {
        isBuildPanel=true;
    }
    public void BuildedOff()
    {
        isBuildPanel=false;
    }

    bool CanBuildHere(Vector3Int tilePosition)
    {
        // Проверяем, нет ли здесь другого объекта
        if (builtObjects.ContainsKey(tilePosition))
            return false;

        // Можно добавить другие проверки (доступность тайла и т.д.)
        return true;
    }

    // Методы для выбора объекта для строительства (вызывайте из UI)
    public void SelectAquarium()
    {
        selectedObjectType = aquarium;
        _baseType= BuildingType.Aquarium;
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
            Vector3 buildPosition = tilemap.GetCellCenterWorld(tilePosition);

            Gizmos.color = CanBuildHere(tilePosition) ? Color.green : Color.red;
            Gizmos.DrawWireCube(buildPosition, Vector3.one * 0.8f);
        }
    }
}