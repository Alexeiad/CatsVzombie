using System.Collections.Generic;
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

    [SerializeField] private BuildingsBuilder buildingsBuilder;

    private HashSet<Vector3Int> tilePositions = new HashSet<Vector3Int>();
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
        foreach (Vector3Int cellPos in tilePositions)
        {
            Vector2Int gridPos = new Vector2Int(cellPos.x, cellPos.y);
            bool isOccupied = IsPositionOccupied(gridPos);
            highlightTilemap.SetTile(cellPos, isOccupied ? occupiedTile : buildableTile);
        }
    }

    private bool IsPositionOccupied(Vector2Int gridPosition)
    {
        if (buildingsBuilder == null) return false;

        System.Reflection.FieldInfo field = typeof(BuildingsBuilder).GetField("builtObjects",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            var builtObjects = field.GetValue(buildingsBuilder) as Dictionary<Vector2Int, GameObject>;
            return builtObjects != null && builtObjects.ContainsKey(gridPosition);
        }

        return false;
    }

    private void OnDisable()
    {
        // Очищаем тайлы при отключении компонента
        if (highlightTilemap != null)
        {
            highlightTilemap.ClearAllTiles();
        }
    }

    private void OnEnable()
    {
        // Восстанавливаем подсветку при включении компонента
        if (highlightTilemap != null && buildingsBuilder != null)
        {
            RefreshAllTiles();
        }
    }
}