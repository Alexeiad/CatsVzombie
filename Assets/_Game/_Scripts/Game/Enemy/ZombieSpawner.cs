using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private EntitiesDataSO _entitiesDataSO;
    [SerializeField] private float _spawnInterval = 1f;
    [SerializeField] private Vector2 _spawnHorizontal;
    [SerializeField] private Vector2 _spawnVertical;
    [SerializeField] private List<Transform> _obstacleList;

    [Header("Modes")]
    [SerializeField] private bool _enableRandomMode = true; // Mode 1: Current random spawn
    [SerializeField] private bool _enableSpawnPointsMode = false; // Mode 2: Spawn points based
    [SerializeField] private bool _enableWaveMode = false; // Mode 3: Wave-based spawn

    [Header("Spawn Points Mode")]
    [SerializeField] private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>(); // List of spawn points (can be populated in editor or found at runtime)

    [Header("Wave Mode")]
    [SerializeField] private float _waveInterval = 30f; // Time between waves
    [SerializeField] private float _waveDuration = 10f; // Duration of each wave
    [SerializeField] private AnimationCurve _waveSpawnCurve = AnimationCurve.Linear(0, 0, 1, 1); // Curve for spawn rate during wave (0-1 normalized time)
    [SerializeField] private int _waveEnemyCount = 20; // Total enemies to spawn per wave

    [Inject] private PlayerMovement _playerMovement;

    private List<Coroutine> _activeCoroutines = new List<Coroutine>();

    private void Start()
    {
        if (_enableRandomMode)
        {
            _activeCoroutines.Add(StartCoroutine(RandomSpawnRoutine()));
        }

        if (_enableSpawnPointsMode)
        {
            // If not populated, find all SpawnPoints in scene
            if (_spawnPoints.Count == 0)
            {
                _spawnPoints.AddRange(FindObjectsOfType<SpawnPoint>());
            }
            foreach (var spawnPoint in _spawnPoints)
            {
                spawnPoint.Initialize(_entitiesDataSO, _playerMovement, _obstacleList);
                _activeCoroutines.Add(StartCoroutine(spawnPoint.SpawnRoutine()));
            }
        }

        if (_enableWaveMode)
        {
            _activeCoroutines.Add(StartCoroutine(WaveSpawnRoutine()));
        }
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
        while (true)
        {
            foreach (var entity in _entitiesDataSO.EnemyRows)
            {
                EntityType[] entityTypes = System.Enum.GetValues(typeof(EntityType))
                    .Cast<EntityType>()
                    .Where(e => e != EntityType.Cat)
                    .ToArray();

                EntityType randomEntityType = entityTypes[Random.Range(0, entityTypes.Length)];

                if (entity.EntityType == randomEntityType)
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

                    var newEntity = Instantiate(entity.EntityPrefab, spawnPosition, Quaternion.identity);
                    newEntity.GetComponent<Enemy>().InstantiateConstructor(_playerMovement, _obstacleList);
                }
            }
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private IEnumerator WaveSpawnRoutine() // Mode 3: Wave spawn
    {
        while (true)
        {
            // Wait for wave interval
            yield return new WaitForSeconds(_waveInterval);

            // Start wave
            float waveStartTime = Time.time;
            int spawnedCount = 0;

            while (Time.time - waveStartTime < _waveDuration && spawnedCount < _waveEnemyCount)
            {
                // Evaluate curve for spawn intensity (e.g., higher value = more likely to spawn)
                float normalizedTime = (Time.time - waveStartTime) / _waveDuration;
                float spawnIntensity = _waveSpawnCurve.Evaluate(normalizedTime);

                // Decide how many to spawn this frame based on intensity (e.g., 0-1 curve -> 0-5 enemies)
                int spawnThisTick = Mathf.RoundToInt(spawnIntensity * 5f); // Adjustable multiplier

                for (int i = 0; i < spawnThisTick && spawnedCount < _waveEnemyCount; i++)
                {
                    // Similar to random spawn, but without per-entity loop; pick random type
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
                        newEntity.GetComponent<Enemy>().InstantiateConstructor(_playerMovement, _obstacleList);
                        spawnedCount++;
                    }
                }

                yield return null; // Wait one frame
            }
        }
    }
}