using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

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
    

    [Header("Spawn Mode")]
    [SerializeField] private bool _spawnOnce = false; // Если true - спавнит разово _maxAlive врагов

    [Header("Entity Percentages")]
    [SerializeField] private List<EntitySpawnChance> _entityChances = new List<EntitySpawnChance>(); // Percentages for each type
    [SerializeField] private GameObject _resultWindow;

    [Inject] private EntityList _entities;

    private EntitiesDataSO _entitiesDataSO;
    private PlayerMovement _playerMovement;
    private Transform _playerTransform; // Кешируем для производительности
    private List<Transform> _obstacleList;
    private List<GameObject> _spawnedEntities = new List<GameObject>(); // Track alive entities
    private bool _hasSpawnedOnce = false; // Флаг для отслеживания разового спауна
    private ZombieSpawner _zombieSpawner;
    private StatsPreview _statsPreview;
    private bool _isCorutineStared,_isShowOnce;

    public void Initialize(EntitiesDataSO entitiesDataSO, PlayerMovement playerMovement, List<Transform> obstacleList,ZombieSpawner zombieSpawner)
    {
        _entitiesDataSO = entitiesDataSO;
        _playerMovement = playerMovement;
        _zombieSpawner = zombieSpawner;
        _playerTransform = playerMovement?.transform; // Кешируем трансформ игрока
        _obstacleList = obstacleList;
        _statsPreview=_zombieSpawner.statsPreview;

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
    private void Update()
    {
        _spawnedEntities.RemoveAll(e => e == null);

        if (_spawnedEntities.Count == 0&&_isCorutineStared&& !_isShowOnce)
        {
            FindObjectsByType<PlayerReward>(FindObjectsSortMode.None)
                .ToList().ForEach(o=>o.gameObject.SetActive(false));
            _resultWindow.SetActive(true);
            _isShowOnce = true;
            Time.timeScale = 0;
        }
        
    }
    public IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Clean up destroyed entities
            

            // Если режим разового спауна и уже заспавнили - выходим из корутины
            if (_spawnOnce && _hasSpawnedOnce)
            {
                yield break; // Прерываем корутину
            }

            // Только если есть место под новых врагов и игрок вне радиуса
            if (_spawnedEntities.Count < _maxAlive && _playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

                // Спавним ТОЛЬКО если игрок дальше радиуса спавна
                if (distanceToPlayer > _spawnRadius)
                {
                    // Для разового спауна спавним сразу всех врагов
                    int toSpawn;
                    if (_spawnOnce && !_hasSpawnedOnce)
                    {
                        // Спавним сразу всех врагов до максимума
                        toSpawn = _maxAlive - _spawnedEntities.Count;
                        _hasSpawnedOnce = true; // Помечаем, что разовый спаун выполнен
                    }
                    else
                    {
                        // Обычный режим - спавним группой
                        int groupSize = Random.Range(_minGroupSize, _maxGroupSize + 1);
                        toSpawn = Mathf.Min(groupSize, _maxAlive - _spawnedEntities.Count);
                    }

                    for (int i = 0; i < toSpawn; i++)
                    {
                        EntityType selectedType = PickEntityType();

                        // Собираем все префабы данного типа
                        var matchingRows = _entitiesDataSO.EnemyRows
                            .Where(e => e.EntityType == selectedType)
                            .ToList();

                        if (matchingRows.Count == 0)
                        {
                            continue; // Нет префабов этого типа — пропускаем
                        }

                        // Выбираем случайный префаб среди подходящих
                        var entityData = matchingRows[Random.Range(0, matchingRows.Count)];

                        // Пытаемся найти подходящую позицию (не слишком близко к игроку)
                        Vector3 spawnPosition = GetValidSpawnPosition(distanceToPlayer);

                        if (spawnPosition != Vector3.zero) // Если нашли позицию
                        {
                            var newEntity = Instantiate(entityData.EntityPrefab, spawnPosition, Quaternion.identity);
                            var enemy = newEntity.GetComponent<Enemy>();
                            if (enemy != null)
                            {
                                _zombieSpawner.enemies.Add(enemy);
                                _statsPreview.enemys.Add(enemy);
                                enemy.InstantiateConstructor(_playerMovement, _obstacleList,_zombieSpawner);

                            }
                            _isCorutineStared = true;
                            _spawnedEntities.Add(newEntity);
                            _entities.Add(newEntity.transform);
                        }
                    }
                }
                
            }

            // Если режим разового спауна и уже заспавнили - выходим после спауна
            if (_spawnOnce && _hasSpawnedOnce)
            {
                yield break; // Прерываем корутину
            }

            // Ждём случайный интервал (для разового спауна это не имеет значения, т.к. выйдем сразу)
            yield return new WaitForSeconds(Random.Range(_minInterval, _maxInterval));
        }
    }

    private Vector3 GetValidSpawnPosition(float distanceToPlayer)
    {
        const int maxAttempts = 10;
        const float minDistanceToPlayer = 1.5f; // Минимальное расстояние до игрока, чтобы не спавнить прямо на нём

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 offset = Random.insideUnitCircle * _spawnRadius;
            Vector3 candidate = transform.position + new Vector3(offset.x, offset.y, 0f);

            float distToPlayer = Vector3.Distance(candidate, _playerTransform.position);
            if (distToPlayer >= minDistanceToPlayer)
            {
                return candidate;
            }
        }

        // Если не нашли подходящую позицию — возвращаем zero (не спавним этого врага)
        return Vector3.zero;
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

    // Метод для принудительного сброса флага разового спауна (если нужно перезапустить)
    public void ResetSpawnFlag()
    {
        _hasSpawnedOnce = false;
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