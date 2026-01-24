using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; // Для SortingGroup

public class Enemy : MonoBehaviour, IEntity
{
    public int entityID;
    public EntitiesDataSO entitiesDataSO;
    public int MaxHealth { get; private set; }

    // Свойства из интерфейса IEntity
    public int ID {get; set; }
    public string CharacterName { get; set; }
    public EntityType EntityType { get; set; }
    public Vector2 MovementSpeed { get; set; }
    public Vector2 AttackSpeed { get; set; }
    public Vector2 ShootSpeed { get; set; }
    public int ShootDamage { get; set; }
    public int MeleeDamage { get; set; }
    public int Health { get; set; }
    public float DetectionRadius { get; set; } = 6f;
    public float AttackRange { get; set; } = 2f;
    public float ShootRange { get; set; } = 5f;
    public float Armor { get; set; }

    public EnemyStateMachine StateMachine { get; private set; }

    
    public List<Transform> ObstacleList { get; private set; }

    // === НОВАЯ ЧАСТЬ: ползущий зомби ===
    [SerializeField] private bool isCreepingZombie = false; // В инспекторе включите для префаба ползущего зомби

    // === Сортировка по Y ===
    [SerializeField] private float sortingUnitsPerMeter = 100f; // Настройте в инспекторе

    // === НОВАЯ ЧАСТЬ: избегание других врагов (сепарация) ===
    [SerializeField] private float enemySeparationDistance = 1f;   // Минимальная дистанция до других врагов (настраивается в инспекторе)
    [SerializeField] private float enemySeparationStrength = 0.1f;   // Сила отталкивания (настраивается в инспекторе)


    private PlayerMovement _playerMovement;
    private SpriteRenderer mySpriteRenderer;
    private SpriteRenderer playerSpriteRenderer;

    private List<Enemy> _enemies;

    public void InstantiateConstructor(PlayerMovement playerMovement, List<Transform> obstacleList, ZombieSpawner zombieSpawner)
    {
        ObstacleList = obstacleList;

        _playerMovement = playerMovement;

        _playerMovement.currentEnemis.Add(this);

        _enemies = zombieSpawner.enemies;
        
       
        GetComponent<CustomEnemyAI2D>().Initialize(playerMovement,_enemies);

        foreach (var entity in entitiesDataSO.EnemyRows)
        {
            if (entityID == entity.ID)
            {
                CharacterName = entity.CharacterName;
                EntityType = entity.EntityType;
                MovementSpeed = entity.MovementSpeed;
                AttackSpeed = entity.AttackSpeed;
                ShootSpeed = entity.ShootSpeed;
                ShootDamage = entity.ShootDamage;
                MeleeDamage = entity.MeleeDamage;
                Health = entity.Health;
                MaxHealth = Health;
                DetectionRadius = entity.DetectionRadius;
                AttackRange = entity.AttackRange;
                ShootRange = entity.ShootRange;
                Armor = entity.Armor;
            }
        }
       
        //StateMachine = new EnemyStateMachine(this, new EnemyAttackState(), _playerMovement);
        
        mySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerSpriteRenderer = playerMovement.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        StateMachine?.Update();
        UpdateSortingOrder();

        if (Health <= 0)
        {
            Destroy(gameObject);
        }   
    }

    private void LateUpdate()
    {
        if (isCreepingZombie)
        {
            UpdateCreepingRotation();
        }
    }

    private void UpdateCreepingRotation()
    {
        if (_playerMovement == null) return;

        Vector3 direction = _playerMovement.transform.position - transform.position;

        // Проецируем направление на плоскость XY (игнорируем Z)
        Vector3 flatDirection = new Vector3(direction.x, direction.y, 0f);

        float sqrDistance = flatDirection.sqrMagnitude;

        if (sqrDistance > 0.001f) // Избегаем деления на ноль и мелких дрожаний
        {
            flatDirection = flatDirection.normalized;

            // transform.up = -flatDirection → transform.down направлен к игроку
            transform.up = -flatDirection;
        }
    }

    private void UpdateSortingOrder()
    {
        float deltaY = _playerMovement.transform.position.y - transform.position.y;
        int offset = Mathf.RoundToInt(deltaY * sortingUnitsPerMeter);

        mySpriteRenderer.sortingOrder = playerSpriteRenderer.sortingOrder + offset;
    }

    public bool PlayerInDetectionRadius()
    {
        return Vector3.Distance(transform.position, _playerMovement.transform.position) < DetectionRadius;
    }

    public bool PlayerInAttackRange()
    {
        return Vector3.Distance(transform.position, _playerMovement.transform.position) < AttackRange;
    }

    public bool PlayerInShootRange()
    {
        float distance = Vector3.Distance(transform.position, _playerMovement.transform.position);
        return distance > AttackRange && distance < ShootRange;
    }

    public bool PlayerInMeleeRange()
    {
        return Vector3.Distance(transform.position, _playerMovement.transform.position) < AttackRange;
    }

    // === НОВАЯ ЧАСТЬ: расчёт силы сепарации от других врагов ===
    /// <summary>
    /// Возвращает вектор силы отталкивания от близких врагов.
    /// Вызывайте этот метод в состоянии погони/движения (например, в EnemyChaseState или EnemyWalkingState).
    /// </summary>
    public Vector2 GetSeparationForce()
    {
        Vector2 separation = Vector2.zero;
        int neighborCount = 0;

        if (_enemies == null) return Vector2.zero;

        foreach (var other in _enemies)
        {
            if (other == null || other == this) continue;

            Vector2 toOther = (Vector2)(other.transform.position - transform.position);
            float distance = toOther.magnitude;

            if (distance < enemySeparationDistance && distance > 0.01f)
            {
                Vector2 repelDirection = -toOther.normalized;
                float weight = 1f - (distance / enemySeparationDistance); // Чем ближе — тем сильнее
                separation += repelDirection * weight;
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            separation /= neighborCount;
            separation = separation.normalized * enemySeparationStrength;
        }

        return separation;
    }
}