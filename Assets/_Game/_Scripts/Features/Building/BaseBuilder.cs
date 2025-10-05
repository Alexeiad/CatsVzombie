using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

public class BaseBuilder : MonoBehaviour
{
    public Tilemap tilemap; // Ссылка на ваш тайлмап
    public GameObject[] buildableObjects; // Массив префабов: [аквариум, вода, верстак]

    [SerializeField] private GameObject fishBase,waterBase;

    public int Bases;//

    private GameObject selectedObjectType; // Выбранный тип объекта для строительства
    private Dictionary<Vector3Int, GameObject> builtObjects = new Dictionary<Vector3Int, GameObject>(); // Словарь построенных объектов

    private PlayerMovement _playerMovement;
    private Camera _camera;

    private string _FishKey="FishKey", _WaterKey = "WaterKey";

    private BaseList _bases;

    
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
            if (baseData.ID == _FishKey)
            {
                Instantiate(fishBase, baseData.Position, Quaternion.identity);
            }
            else if (baseData.ID == _WaterKey)
            {
                Instantiate(waterBase, baseData.Position, Quaternion.identity);
            }


        }
        
    }



    void Update()
    {
        Bases = _bases.Count;//
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
                if (CanBuildHere(tilePosition))
                {
                    // Спавним объект
                    GameObject newObject = Instantiate(selectedObjectType, buildPosition+new Vector3(0,0,10f),
                        Quaternion.identity);
                    builtObjects[tilePosition] = newObject;

                    _bases.Add(new BaseElementsSaveData(_FishKey, newObject.transform.position));
                    
                }
            }
        }

        // Отмена выбора по правой кнопке мыши или Escape
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectObject();
        }
    }

    Vector3 CalculateBuildPosition(Vector3Int tilePos)
    {
        // Для изометрического тайлмапа используем GetCellCenterWorld
        Vector3 worldPos = tilemap.GetCellCenterWorld(tilePos);

        return worldPos;
    }

    bool CanBuildHere(Vector3Int tilePosition)
    {
        // Проверяем, нет ли здесь другого объекта
        if (builtObjects.ContainsKey(tilePosition))
            return false;

        // Можно добавить другие проверки (доступность тайла и т.д.)
        return true;
    }

    bool IsPointerOverUI()
    {
        // Простая проверка на UI (можно заменить на более продвинутую)
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    // Методы для выбора объекта для строительства (вызывайте из UI)
    public void SelectAquarium()
    {
        selectedObjectType = buildableObjects[0];
        Debug.Log("Выбран аквариум. Кликните по тайлу для размещения.");
    }

    public void SelectWater()
    {
        selectedObjectType = buildableObjects[1];
        Debug.Log("Выбрана вода. Кликните по тайлу для размещения.");
    }

    public void SelectWorkbench()
    {
        selectedObjectType = buildableObjects[2];
        Debug.Log("Выбран верстак. Кликните по тайлу для размещения.");
    }

    public void DeselectObject()
    {
        selectedObjectType = null;
        Debug.Log("Объект deselected");
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