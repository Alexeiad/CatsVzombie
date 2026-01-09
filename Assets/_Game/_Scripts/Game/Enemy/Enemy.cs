// Enemy.cs
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;
using Zenject.SpaceFighter;

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

    private PlayerMovement _playerMovement;


    public void InstantiateConstructor(PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;

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


        StateMachine = new EnemyStateMachine(this, new EnemyIdleState());
    }

    void Update()
    {
        StateMachine?.Update();
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

 
    public void MoveTowardsPlayer()
    {
        transform.DOMove(_playerMovement.transform.position,2f);
    }

    public void FleeFromPlayer()
    {

    }

    public void MeleeAttack()
    {
        
    }

    public void Shoot()
    {
        
    }

    public void TakeDamage(int damage)
    {
        
    }

    private void Die()
    {
        // Логика смерти врага
    }
}