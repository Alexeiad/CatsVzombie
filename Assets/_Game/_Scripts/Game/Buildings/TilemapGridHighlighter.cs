using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGridHighlighter : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tilemap highlightTilemap;

    [Header("Highlight Tiles")]
    [SerializeField] private TileBase buildableTile;
    [SerializeField] private TileBase occupiedTile;

    [Header("Building Detection")]
    [SerializeField] private LayerMask buildingLayerMask = -1; // По умолчанию все слои
    [SerializeField] private float checkRadius = 0.3f; // Радиус для проверки зданий

    private HashSet<Vector3Int> tilePositions = new HashSet<Vector3Int>();
    private Dictionary<Vector3Int, Collider2D[]> occupiedTilesCache = new Dictionary<Vector3Int, Collider2D[]>();

    private float updateTimer = 0f;
    private const float UPDATE_INTERVAL = 0.1f;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (highlightTilemap == null)
        {
            highlightTilemap = CreateHighlightTilemap();
        }

        FindAllTilePositions();
        RefreshAllTiles();
    }

    private Tilemap CreateHighlightTilemap()
    {
        GameObject highlightGO = new GameObject("HighlightTilemap");
        highlightGO.transform.SetParent(tilemap.transform.parent);
        highlightGO.transform.position = tilemap.transform.position + new Vector3(0, 0, -0.1f);

        Tilemap newTilemap = highlightGO.AddComponent<Tilemap>();
        TilemapRenderer renderer = highlightGO.AddComponent<TilemapRenderer>();

        TilemapRenderer baseRenderer = tilemap.GetComponent<TilemapRenderer>();
        if (baseRenderer != null)
        {
            renderer.sortingOrder = baseRenderer.sortingOrder + 1;
            renderer.sortingLayerName = baseRenderer.sortingLayerName;
            renderer.material = baseRenderer.material;
        }

        return newTilemap;
    }

    private void FindAllTilePositions()
    {
        tilePositions.Clear();

        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(cellPosition))
                {
                    tilePositions.Add(cellPosition);
                }
            }
        }
    }

    private void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= UPDATE_INTERVAL)
        {
            updateTimer = 0f;
            RefreshAllTiles();
        }
    }

    private void RefreshAllTiles()
    {
        // Очищаем кэш
        occupiedTilesCache.Clear();

        // Сначала устанавливаем buildableTile на все позиции
        foreach (Vector3Int cellPos in tilePositions)
        {
            highlightTilemap.SetTile(cellPos, buildableTile);
        }

        // Ищем все здания на сцене
        FindAndMarkOccupiedTiles();
    }

    private void FindAndMarkOccupiedTiles()
    {
        // Вариант 1: Ищем все объекты с SpriteRenderer
        BuildingBehaviour[] allBuildings = FindObjectsOfType<BuildingBehaviour>();

        foreach (BuildingBehaviour building in allBuildings)
        {
            if (building == null || !building.enabled || building.gameObject == null)
                continue;

            // Пропускаем сам tilemap и highlightTilemap
            if (building.GetComponent<TilemapRenderer>() != null)
                continue;

            // Проверяем, находится ли здание над тайлом
            MarkTileUnderBuilding(building.transform.position, Vector3Int.one);
        }

        // Вариант 2: Ищем все объекты с определенным тегом или компонентом
        // GameObject[] taggedBuildings = GameObject.FindGameObjectsWithTag("Building");
        // foreach (GameObject building in taggedBuildings)
        // {
        //     if (building.activeInHierarchy)
        //     {
        //         MarkTileUnderBuilding(building.transform.position, GetObjectSize(building));
        //     }
        // }
    }

    private void MarkTileUnderBuilding(Vector3 worldPosition, Vector3 buildingSize)
    {
        // Преобразуем мировую позицию в позицию тайла
        Vector3Int cellPosition = highlightTilemap.WorldToCell(worldPosition);

        // Проверяем, есть ли тайл в этой позиции
        if (tilePositions.Contains(cellPosition))
        {
            // Ставим occupiedTile под зданием
            highlightTilemap.SetTile(cellPosition, occupiedTile);

            // Если здание большое, отмечаем соседние тайлы
            if (buildingSize.x > 1f || buildingSize.y > 1f)
            {
                int cellsX = Mathf.CeilToInt(buildingSize.x);
                int cellsY = Mathf.CeilToInt(buildingSize.y);

                for (int x = 0; x < cellsX; x++)
                {
                    for (int y = 0; y < cellsY; y++)
                    {
                        Vector3Int offsetCell = cellPosition + new Vector3Int(x, y, 0);
                        if (tilePositions.Contains(offsetCell))
                        {
                            highlightTilemap.SetTile(offsetCell, occupiedTile);
                        }
                    }
                }
            }
        }
    }

    private Vector3 GetObjectSize(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size;
        }

        Collider2D collider2D = obj.GetComponent<Collider2D>();
        if (collider2D != null)
        {
            return collider2D.bounds.size;
        }

        // Если нет коллайдера или рендерера, используем размер по умолчанию
        return Vector3.one;
    }

    // Метод для проверки конкретной позиции
    public bool IsTileOccupied(Vector3Int cellPosition)
    {
        // Проверяем коллайдеры в центре тайла
        Vector3 worldPos = highlightTilemap.GetCellCenterWorld(cellPosition);

        // Ищем SpriteRenderer в этой позиции
        SpriteRenderer[] renderers = Physics2D.OverlapCircleAll(worldPos, checkRadius)
            .Select(collider => collider.GetComponent<SpriteRenderer>())
            .Where(renderer => renderer != null && renderer.enabled)
            .ToArray();

        return renderers.Length > 0;
    }

    // Метод для принудительного обновления
    public void ForceRefresh()
    {
        RefreshAllTiles();
    }

    private void OnDisable()
    {
        if (highlightTilemap != null)
        {
            highlightTilemap.ClearAllTiles();
        }
    }

    private void OnEnable()
    {
        if (highlightTilemap != null && tilePositions.Count > 0)
        {
            RefreshAllTiles();
        }
    }

    // Визуализация в редакторе (для отладки)
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || highlightTilemap == null) return;

        Gizmos.color = Color.red;
        foreach (Vector3Int cellPos in tilePositions)
        {
            if (IsTileOccupied(cellPos))
            {
                Vector3 worldPos = highlightTilemap.GetCellCenterWorld(cellPos);
                Gizmos.DrawWireCube(worldPos, Vector3.one * 0.8f);
            }
        }
    }
}