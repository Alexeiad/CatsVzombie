
using System.Collections;
using System.Linq;
using UnityEngine;
using Zenject;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private EntitiesDataSO _entitiesDataSO;
    [SerializeField] private float _spawnInterval=1;
    [SerializeField] private Vector2 _spawnHorizontal;
    [SerializeField] private Vector2 _spawnVertical;

    [Inject] private PlayerMovement _playerMovement;

    private Coroutine _coroutine;

    private void Start()
    {
        _coroutine = StartCoroutine(SpawnZombies());
    }
    private IEnumerator SpawnZombies()
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
                    newEntity.GetComponent<Enemy>().InstantiateConstructor(_playerMovement);
                }
                
            }
            yield return new WaitForSeconds(_spawnInterval);
        }
        
    }
}
