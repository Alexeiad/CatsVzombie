// Enemy.cs

using System.Collections.Generic;
using UnityEngine;


public class Enemy : MonoBehaviour, IEntity
{
    [SerializeField] private EntityType _entityType;
    [SerializeField] private EntitiesDataSO _entitiesDataSO;

    // Свойства из интерфейса IEntity
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

    public PlayerMovement PlayerMovement { get; private set; }
    public List<Transform> ObstacleList { get; private set; }


    public void InstantiateConstructor(PlayerMovement playerMovement,List<Transform> obstacleList)
    {
        PlayerMovement = playerMovement;
        ObstacleList = obstacleList;

        foreach (var entity in _entitiesDataSO.EnemyRows)
        {
            if (_entityType == entity.EntityType)
            {
                CharacterName = entity.CharacterName;
                MovementSpeed = entity.MovementSpeed;
                AttackSpeed = entity.AttackSpeed;
                ShootSpeed = entity.ShootSpeed;
                ShootDamage = entity.ShootDamage;
                MeleeDamage = entity.MeleeDamage;
                Health = entity.Health;
                DetectionRadius = entity.DetectionRadius;
                AttackRange = entity.AttackRange;
                ShootRange = entity.ShootRange;
                Armor = entity.Armor;
            }
        }


        StateMachine = new EnemyStateMachine(this, new EnemyWalkingState(),playerMovement);
        
    }

    private void Update()
    {
        StateMachine?.Update();
        
    }
    
    
    public bool PlayerInDetectionRadius()
    {

        return Vector3.Distance(transform.position, PlayerMovement.transform.position) < DetectionRadius;
    }

    public bool PlayerInAttackRange()
    {

        return Vector3.Distance(transform.position, PlayerMovement.transform.position) < AttackRange;
    }

    public bool PlayerInShootRange()
    {

        float distance = Vector3.Distance(transform.position, PlayerMovement.transform.position);
        return distance > AttackRange && distance < ShootRange;
    }

    public bool PlayerInMeleeRange()
    {

        return Vector3.Distance(transform.position, PlayerMovement.transform.position) < AttackRange;
    }

 
  
 
}