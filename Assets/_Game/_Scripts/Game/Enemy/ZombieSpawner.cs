using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class ZombieSpawner : MonoBehaviour
{
    public List<Enemy> enemies  = new List<Enemy>();
    public StatsPreview statsPreview;

    [SerializeField] private EntitiesDataSO _entitiesDataSO;
    [SerializeField] private float _spawnInterval = 1f;
    [SerializeField] private Vector2 _spawnHorizontal;
    [SerializeField] private Vector2 _spawnVertical;
    [SerializeField] private List<Transform> _obstacleList;
    

    [Header("Modes")]
    [SerializeField] private bool _enableRandomMode = true; // Mode 1: Current random spawn
    [SerializeField] private bool _enableSpawnPointsMode = false; // Mode 2: Spawn points based
    [SerializeField] private bool _enableWaveMode = false; // Mode 3: Wave-based spawn

    [Header("Once Spawn Settings")]
    [SerializeField] private bool _randomModeSpawnOnce = false; // Разовый спаун для Random Mode
    [SerializeField] private int _randomOnceCount = 10; // Количество врагов для разового спауна в Random Mode

    [Header("Spawn Points Mode")]
    [SerializeField] private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>(); // List of spawn points (can be populated in editor or found at runtime)
    [SerializeField] private bool _spawnPointsSpawnOnce = false; // Глобальная настройка для всех спаун-поинтов

    [Header("Wave Mode")]
    [SerializeField] private float _waveInterval = 30f; // Time between waves
    [SerializeField] private float _waveDuration = 10f; // Duration of each wave
    [SerializeField] private AnimationCurve _waveSpawnCurve = AnimationCurve.Linear(0, 0, 1, 1); // Curve for spawn rate during wave (0-1 normalized time)
    [SerializeField] private int _waveEnemyCount = 20; // Total enemies to spawn per wave
    [SerializeField] private bool _waveModeSpawnOnce = false; // Разовый спаун для Wave Mode (только одна волна)
    [SerializeField] private int _waveOnceCount = 15; // Количество врагов для разовой волны

    [Header("Enemy Limits")]
    [SerializeField] private int _maxEnemiesOnMap = 30; // Максимальное количество зомби на карте

    [Inject] private PlayerMovement _playerMovement;
    [Inject] private EntityList _entities;

    private List<Coroutine> _activeCoroutines = new List<Coroutine>();
    private Dictionary<string, bool> _hasSpawnedOnce = new Dictionary<string, bool>();// Трекер для разовых спаунов
   

    private void Start()
    {
        _hasSpawnedOnce.Clear();

        if (_enableRandomMode)
        {
            _activeCoroutines.Add(StartCoroutine(RandomSpawnRoutine()));
        }

        if (_enableSpawnPointsMode)
        {
            // Если не заполнены в редакторе, найти все SpawnPoints на сцене
            if (_spawnPoints.Count == 0)
            {
                _spawnPoints.AddRange(FindObjectsOfType<SpawnPoint>());
            }

            // Применить глобальную настройку разового спауна ко всем спаун-поинтам
            ApplySpawnOnceToAllPoints();

            foreach (var spawnPoint in _spawnPoints)
            {
                spawnPoint.Initialize(_entitiesDataSO, _playerMovement, _obstacleList,this);
                _activeCoroutines.Add(StartCoroutine(spawnPoint.SpawnRoutine()));
            }
        }

        if (_enableWaveMode)
        {
            _activeCoroutines.Add(StartCoroutine(WaveSpawnRoutine()));
        }
    }

    // Метод для применения глобальной настройки разового спауна ко всем спаун-поинтам
    private void ApplySpawnOnceToAllPoints()
    {
        if (!_enableSpawnPointsMode) return;

        foreach (var spawnPoint in _spawnPoints)
        {
            // Используем публичный метод SetSpawnOnce если он есть
            var spawnPointComponent = spawnPoint.GetComponent<SpawnPoint>();
            if (spawnPointComponent != null)
            {
                // Пытаемся найти метод SetSpawnOnce
                var method = typeof(SpawnPoint).GetMethod("SetSpawnOnce",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (method != null)
                {
                    method.Invoke(spawnPointComponent, new object[] { _spawnPointsSpawnOnce });
                }
                else
                {
                    // Или устанавливаем через свойство/поле
                    var property = typeof(SpawnPoint).GetProperty("SpawnOnce");
                    if (property != null)
                    {
                        property.SetValue(spawnPointComponent, _spawnPointsSpawnOnce);
                    }
                    else
                    {
                        // Через рефлексию для приватного поля
                        var field = typeof(SpawnPoint).GetField("_spawnOnce",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (field != null)
                        {
                            field.SetValue(spawnPointComponent, _spawnPointsSpawnOnce);
                        }
                    }
                }
            }
        }
    }

    // Метод для сброса флагов разового спауна на всех спаун-поинтах
    public void ResetAllSpawnPoints()
    {
        foreach (var spawnPoint in _spawnPoints)
        {
            var method = spawnPoint.GetComponent<SpawnPoint>()?.GetType().GetMethod("ResetSpawnFlag");
            method?.Invoke(spawnPoint.GetComponent<SpawnPoint>(), null);
        }

        // Сбрасываем флаги разовых спаунов
        _hasSpawnedOnce.Clear();
    }

    // Публичный метод для сброса всех разовых спаунов
    public void ResetAllOnceSpawns()
    {
        ResetAllSpawnPoints();
        _hasSpawnedOnce.Clear();
    }

    private void OnDestroy()
    {
        foreach (var coroutine in _activeCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
    }

    private IEnumerator RandomSpawnRoutine() // Mode 1: Original random spawn
    {
        string modeKey = "RandomMode";
        bool hasSpawned = _hasSpawnedOnce.ContainsKey(modeKey) && _hasSpawnedOnce[modeKey];
        int spawnedCount = 0;

        while (true)
        {
            // Проверяем, нужно ли остановиться после разового спауна
            if (_randomModeSpawnOnce && hasSpawned)
            {
                yield break; // Прерываем корутину
            }

            // Проверяем, достигли ли лимита для разового спауна
            if (_randomModeSpawnOnce && spawnedCount >= _randomOnceCount)
            {
                _hasSpawnedOnce[modeKey] = true;
                hasSpawned = true;
                yield break; // Прерываем корутину
            }

            foreach (var entity in _entitiesDataSO.EnemyRows)
            {
                // Проверяем лимит для разового спауна
                if (_randomModeSpawnOnce && spawnedCount >= _randomOnceCount)
                {
                    _hasSpawnedOnce[modeKey] = true;
                    hasSpawned = true;
                    yield break;
                }

                EntityType[] entityTypes = System.Enum.GetValues(typeof(EntityType))
                    .Cast<EntityType>()
                    .Where(e => e != EntityType.Cat)
                    .ToArray();

                EntityType randomEntityType = entityTypes[Random.Range(0, entityTypes.Length)];

                if (entity.EntityType == randomEntityType)
                {
                    // Проверка лимита перед спавном
                    if (FindObjectsOfType<Enemy>().Length >= _maxEnemiesOnMap)
                    {
                        continue; // Лимит достигнут — пропускаем этот спавн
                    }

                    Vector3 spawnPosition;
                    int attempts = 0;
                    const int maxAttempts = 100;
                    bool validPosition = false;

                    float exclusionRadius = 12f;

                    do
                    {
                        spawnPosition = new Vector3(
                            Random.Range(_spawnHorizontal.x, _spawnHorizontal.y),
                            Random.Range(_spawnVertical.x, _spawnVertical.y),
                            0f
                        );

                        float distanceX = Mathf.Abs(spawnPosition.x - _playerMovement.transform.position.x);
                        float distanceY = Mathf.Abs(spawnPosition.y - _playerMovement.transform.position.y);

                        validPosition = distanceX > exclusionRadius || distanceY > exclusionRadius;

                        attempts++;
                    } while (!validPosition && attempts < maxAttempts);

                    var newEntity = Instantiate(entity.EntityPrefab, spawnPosition, Quaternion.identity);
                    enemies.Add(newEntity.GetComponent<Enemy>());
                    statsPreview.enemys.Add(newEntity.GetComponent<Enemy>());

                    newEntity.GetComponent<Enemy>().InstantiateConstructor(_playerMovement, _obstacleList,this);

                    if (_randomModeSpawnOnce)
                    {
                        spawnedCount++;
                    }
                }
            }

            // Для разового спауна ждем только если еще не достигли лимита
            if (!_randomModeSpawnOnce || spawnedCount < _randomOnceCount)
            {
                yield return new WaitForSeconds(_spawnInterval);
            }
            else
            {
                _hasSpawnedOnce[modeKey] = true;
                hasSpawned = true;
                yield break;
            }
        }
    }

    private IEnumerator WaveSpawnRoutine() // Mode 3: Wave spawn
    {
        string modeKey = "WaveMode";
        bool hasSpawned = _hasSpawnedOnce.ContainsKey(modeKey) && _hasSpawnedOnce[modeKey];
        int waveNumber = 0;

        while (true)
        {
            // Проверяем, нужно ли остановиться после разовой волны
            if (_waveModeSpawnOnce && hasSpawned)
            {
                yield break; // Прерываем корутину
            }

            // Для разового режима - не ждем интервал перед первой волной
            if (!_waveModeSpawnOnce || waveNumber > 0)
            {
                // Wait for wave interval
                yield return new WaitForSeconds(_waveInterval);
            }

            // Start wave
            float waveStartTime = Time.time;
            int spawnedCount = 0;
            int targetCount = _waveModeSpawnOnce ? _waveOnceCount : _waveEnemyCount;

            waveNumber++;

            while (Time.time - waveStartTime < _waveDuration && spawnedCount < targetCount)
            {
                // Для разового режима проверяем флаг
                if (_waveModeSpawnOnce && hasSpawned)
                {
                    yield break;
                }

                // Evaluate curve for spawn intensity
                float normalizedTime = (Time.time - waveStartTime) / _waveDuration;
                float spawnIntensity = _waveSpawnCurve.Evaluate(normalizedTime);

                int spawnThisTick = Mathf.RoundToInt(spawnIntensity * 5f); // Adjustable multiplier

                for (int i = 0; i < spawnThisTick && spawnedCount < targetCount; i++)
                {
                    // Проверка лимита перед каждым спавном в волне
                    if (FindObjectsOfType<Enemy>().Length >= _maxEnemiesOnMap)
                    {
                        break; // Лимит достигнут — прекращаем спавн в этом тике
                    }

                    EntityType[] entityTypes = System.Enum.GetValues(typeof(EntityType))
                        .Cast<EntityType>()
                        .Where(e => e != EntityType.Cat)
                        .ToArray();

                    EntityType randomEntityType = entityTypes[Random.Range(0, entityTypes.Length)];

                    var entityData = _entitiesDataSO.EnemyRows.FirstOrDefault(e => e.EntityType == randomEntityType);
                    if (entityData != null)
                    {
                        Vector3 spawnPosition;
                        int attempts = 0;
                        const int maxAttempts = 100;
                        bool validPosition = false;

                        float exclusionRadius = 12f;

                        do
                        {
                            spawnPosition = new Vector3(
                                Random.Range(_spawnHorizontal.x, _spawnHorizontal.y),
                                Random.Range(_spawnVertical.x, _spawnVertical.y),
                                0f
                            );

                            float distanceX = Mathf.Abs(spawnPosition.x - _playerMovement.transform.position.x);
                            float distanceY = Mathf.Abs(spawnPosition.y - _playerMovement.transform.position.y);

                            validPosition = distanceX > exclusionRadius || distanceY > exclusionRadius;

                            attempts++;
                        } while (!validPosition && attempts < maxAttempts);

                        var newEntity = Instantiate(entityData.EntityPrefab, spawnPosition, Quaternion.identity);
                        enemies.Add(newEntity.GetComponent<Enemy>());
                        statsPreview.enemys.Add(newEntity.GetComponent<Enemy>());
                        newEntity.GetComponent<Enemy>().InstantiateConstructor(_playerMovement, _obstacleList,this);
                        spawnedCount++;
                    }
                }

                yield return null; // Wait one frame
            }

            // После завершения волны в разовом режиме ставим флаг
            if (_waveModeSpawnOnce)
            {
                _hasSpawnedOnce[modeKey] = true;
                hasSpawned = true;
                yield break;
            }
        }
    }

    // Методы для управления спауном из других скриптов
    public void EnableRandomMode(bool enable) => _enableRandomMode = enable;
    public void EnableSpawnPointsMode(bool enable) => _enableSpawnPointsMode = enable;
    public void EnableWaveMode(bool enable) => _enableWaveMode = enable;

    public void SetRandomOnceSpawn(bool once, int count = 10)
    {
        _randomModeSpawnOnce = once;
        _randomOnceCount = count;
    }

    public void SetWaveOnceSpawn(bool once, int count = 15)
    {
        _waveModeSpawnOnce = once;
        _waveOnceCount = count;
    }
}