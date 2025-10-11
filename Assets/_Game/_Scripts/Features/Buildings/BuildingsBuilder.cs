using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Zenject;

public class BuildingsBuilder : MonoBehaviour
{
    public Tilemap tilemap; // Ссылка на ваш тайлмап


    [SerializeField] private GameObject aquarium,barrel,workbench;


    private GameObject selectedObjectType;
    private GameObject _instantiateObject;
    private Dictionary<Vector3Int, GameObject> builtObjects = new Dictionary<Vector3Int, GameObject>();

    private PlayerMovement _playerMovement;
    private Camera _camera;

    private string _AquariumKey= "AquariumKey",
        _BarrelKey = "BarrelKey",
        _WorkbenchKey = "WorkbenchKey";

    private BaseList _bases;

    private BuildingType _baseType;


    private DiContainer _diContainer;

    private bool isBuildPanel;


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
            if (baseData.ID == _AquariumKey)
            {
                _instantiateObject = aquarium;
            }
            else if (baseData.ID == _BarrelKey)
            {
                _instantiateObject = barrel;
            }
            else if (baseData.ID == _WorkbenchKey)
            {
                _instantiateObject = workbench;
            }

            var go = Instantiate(_instantiateObject, baseData.Position, Quaternion.identity);
   
        }
        
    }



    void Update()
    {
    
        if (Input.GetMouseButtonDown(0)) // Левая кнопка мыши
        {
            // Проверяем, что кликнули не по UI элементу
            if (selectedObjectType != null)
            {
                // Получаем позицию клика и преобразуем в позицию на тайлмапе
                Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int tilePosition = tilemap.WorldToCell(mouseWorldPos);

                // Получаем позицию в центре тайла
                Vector3 buildPosition = tilemap.GetCellCenterWorld(tilePosition);

                // Проверяем, можно ли строить
                if (CanBuildHere(tilePosition)&& isBuildPanel)
                {
                    // Спавним объект
                    GameObject newObject = Instantiate(selectedObjectType, buildPosition+new Vector3(0,0,10f),
                        Quaternion.identity);

                    builtObjects[tilePosition] = newObject;


                   
                    if (_baseType == BuildingType.Aquarium)
                        _bases.Add(new BaseElementsSaveData(_AquariumKey, newObject.transform.position));
                    if (_baseType == BuildingType.Barrel)
                        _bases.Add(new BaseElementsSaveData(_BarrelKey, newObject.transform.position));
                    if (_baseType == BuildingType.Workbench)
                        _bases.Add(new BaseElementsSaveData(_WorkbenchKey, newObject.transform.position));
                }
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