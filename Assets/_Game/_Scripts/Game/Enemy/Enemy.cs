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

    private PlayerMovement _playerMovement;


    public void InstantiateConstructor(PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;

        foreach (var entity in _entitiesDataSO.EnemyRows)
        {
            if (_entityType == entity.EntityType)
            {
                CharacterName=entity.CharacterName;
                MovementSpeed=entity.MovementSpeed;
                AttackSpeed=entity.AttackSpeed;
                ShootSpeed=entity.ShootSpeed;
                ShootDamage=entity.ShootDamage;
                MeleeDamage=entity.MeleeDamage;
                Health=entity.Health;
                DetectionRadius=entity.DetectionRadius;
                AttackRange=entity.AttackRange;
                ShootRange=entity.ShootRange;
                Armor=entity.Armor;
            }
        }

    }

    private enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Flee
    }

    private State currentState;
    

    void Start()
    {
        InstantiateConstructor(_playerMovement);
        currentState = State.Idle;
    }

    void Update()
    {/*
        switch (currentState)
        {
            case State.Idle:
                HandleIdle();
                break;
            case State.Patrol:
                HandlePatrol();
                break;
            case State.Chase:
                HandleChase();
                break;
            case State.Attack:
                HandleAttack();
                break;
            case State.Flee:
                HandleFlee();
                break;
        }
        */
    }

    private void HandleIdle()
    {
        // запуск маршрута

        if (PlayerInDetectionRadius())
        {
            currentState = State.Chase;
        }
    }

    private void HandlePatrol()
    {
        // идти в направлении игрока
        if (PlayerInDetectionRadius())
        {
            currentState = State.Chase;
        }
    }

    private void HandleChase()
    {
        // Ѕежать чтобы ударить или стрельнуть

        if (PlayerInAttackRange())
        {
            currentState = State.Attack;
        }
        else if (!PlayerInDetectionRadius())
        {
            currentState = State.Patrol;
        }
    }

    private void HandleAttack()
    {
        // јтака ударом или выстрелом

        if (Health < 30)
        {
            currentState = State.Flee;
        }
        else if (!PlayerInAttackRange())
        {
            currentState = State.Chase;
        }
    }

    private void HandleFlee()//убежать от игрока
    {
        // Logic for flee state
        // Implement fleeing behavior
    }

    private bool PlayerInDetectionRadius()
    {

        return Vector3.Distance(transform.position, _playerMovement.transform.position) < DetectionRadius;
    }

    private bool PlayerInAttackRange()
    {

        return Vector3.Distance(transform.position, _playerMovement.transform.position) < AttackRange;
    }
}
