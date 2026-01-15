using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float _spawnRadius = 5f; // Radius around the point to spawn within
    [SerializeField] private float _minInterval = 1f; // Min spawn interval
    [SerializeField] private float _maxInterval = 2f; // Max spawn interval
    [SerializeField] private int _minGroupSize = 1; // Min enemies per spawn
    [SerializeField] private int _maxGroupSize = 3; // Max enemies per spawn
    [SerializeField] private int _maxAlive = 5; // Max alive from this point

    [Header("Entity Percentages")]
    [SerializeField] private List<EntitySpawnChance> _entityChances = new List<EntitySpawnChance>(); // Percentages for each type

    private EntitiesDataSO _entitiesDataSO;
    private PlayerMovement _playerMovement;
    private List<Transform> _obstacleList;
    private List<GameObject> _spawnedEntities = new List<GameObject>(); // Track alive entities

    public void Initialize(EntitiesDataSO entitiesDataSO, PlayerMovement playerMovement, List<Transform> obstacleList)
    {
        _entitiesDataSO = entitiesDataSO;
        _playerMovement = playerMovement;
        _obstacleList = obstacleList;

        // Normalize chances if not set
        if (_entityChances.Count == 0)
        {
            // Default equal chances for non-Cat
            var entityTypes = System.Enum.GetValues(typeof(EntityType))
                .Cast<EntityType>()
                .Where(e => e != EntityType.Cat)
                .ToArray();

            float equalChance = 1f / entityTypes.Length;
            foreach (var type in entityTypes)
            {
                _entityChances.Add(new EntitySpawnChance { Type = type, Chance = equalChance });
            }
        }
        else
        {
            // Normalize to sum to 1
            float total = _entityChances.Sum(c => c.Chance);
            if (total > 0)
            {
                foreach (var chance in _entityChances)
                {
                    chance.Chance /= total;
                }
            }
        }
    }

    public IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Clean up destroyed entities
            _spawnedEntities.RemoveAll(e => e == null);

            // Only spawn if under max
            if (_spawnedEntities.Count < _maxAlive)
            {
                int groupSize = Random.Range(_minGroupSize, _maxGroupSize + 1);
                int toSpawn = Mathf.Min(groupSize, _maxAlive - _spawnedEntities.Count);

                for (int i = 0; i < toSpawn; i++)
                {
                    // Pick type based on chances
                    EntityType selectedType = PickEntityType();

                    var entityData = _entitiesDataSO.EnemyRows.FirstOrDefault(e => e.EntityType == selectedType);
                    if (entityData != null)
                    {
                        // Spawn position: random within radius
                        Vector2 offset = Random.insideUnitCircle * _spawnRadius;
                        Vector3 spawnPosition = transform.position + new Vector3(offset.x, offset.y, 0f);

                        var newEntity = Instantiate(entityData.EntityPrefab, spawnPosition, Quaternion.identity);
                        newEntity.GetComponent<Enemy>().InstantiateConstructor(_playerMovement, _obstacleList);
                        _spawnedEntities.Add(newEntity);
                    }
                }
            }

            // Wait random interval
            yield return new WaitForSeconds(Random.Range(_minInterval, _maxInterval));
        }
    }

    private EntityType PickEntityType()
    {
        float rand = Random.value;
        float cumulative = 0f;
        foreach (var chance in _entityChances)
        {
            cumulative += chance.Chance;
            if (rand <= cumulative)
            {
                return chance.Type;
            }
        }
        // Fallback to first non-Cat
        return _entityChances.FirstOrDefault(c => c.Type != EntityType.Cat)?.Type ?? EntityType.Zombie;
    }
}
[System.Serializable]
public class EntitySpawnChance
{
    public EntityType Type;
    [Range(0f, 1f)] public float Chance;
}
[System.Serializable]
public class EnemyRow
{
    public EntityType EntityType;
    public GameObject EntityPrefab; // Assuming prefab has Enemy component
}
