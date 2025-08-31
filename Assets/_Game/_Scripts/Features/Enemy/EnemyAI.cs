using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemyAI : MonoBehaviour,IDamageable<float>
{
    public float MaxHealth = 100;
    public float currentHealth;

    [Header("AI Settings")]
    public float detectionRange = 8f;
    public float attackRange = 5f;
    public float retreatRange = 2f;
    public float decisionRate = 0.5f;
    public LayerMask obstacleMask;

    [Header("Behavior Chances")]
    public float dodgeChance = 0.3f;
    public float repositionChance = 0.4f;

    private EnemyState currentState;
    private Transform _playerTransform;
    private EnemyInput enemyInput;
    private EnemyAttack enemyAttack;

    private float nextDecisionTime;

    [Inject] private List<Transform> _entities;

    [Inject]
    private void Construct(PlayerMovement player)
    {
        _playerTransform = player.transform;
    }


    private void Awake()
    {
        enemyInput = GetComponent<EnemyInput>();
        enemyAttack = GetComponent<EnemyAttack>();

        _entities.Add(transform);
        currentHealth = MaxHealth;
        
    }

    private void Start()
    {
        SetState(EnemyState.Patrol);
    }

    private void Update()
    {
        if (Time.time >= nextDecisionTime)
        {
            MakeDecision();
            nextDecisionTime = Time.time + decisionRate;
        }

        UpdateState();
       
    }
    
    private void MakeDecision()
    {
        if (_playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
        bool hasLineOfSight = CheckLineOfSight();

        switch (currentState)
        {
            case EnemyState.Patrol:
                if (distanceToPlayer <= detectionRange && hasLineOfSight)
                    SetState(EnemyState.Chase);
                break;

            case EnemyState.Chase:
                if (distanceToPlayer <= attackRange && hasLineOfSight)
                    SetState(EnemyState.Attack);
                else if (distanceToPlayer > detectionRange || !hasLineOfSight)
                    SetState(EnemyState.Patrol);
                break;

            case EnemyState.Attack:
                if (distanceToPlayer > attackRange || !hasLineOfSight)
                    SetState(EnemyState.Chase);
                else if (distanceToPlayer <= retreatRange)
                    SetState(EnemyState.Retreat);
                else if (Random.value < dodgeChance)
                    SetState(EnemyState.Dodge);
                break;

            case EnemyState.Dodge:
                
                    SetState(EnemyState.Attack);
                break;

            case EnemyState.Retreat:
                if (distanceToPlayer > retreatRange && hasLineOfSight)
                    SetState(EnemyState.Attack);
                break;
        }
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolBehavior();
                break;

            case EnemyState.Chase:
                ChaseBehavior();
                break;

            case EnemyState.Attack:
                AttackBehavior();
                break;

            case EnemyState.Dodge:
                DodgeBehavior();
                break;

            case EnemyState.Retreat:
                RetreatBehavior();
                break;
            case EnemyState.Dead:
                Die();
                break;
        }
    }

    private void PatrolBehavior()
    {
        // Простое патрулирование или ожидание
        enemyInput.SetMovementInput(Vector2.zero);
        enemyAttack.SetShoot(_playerTransform, false);
    }

    private void ChaseBehavior()
    {
        Vector2 direction = (_playerTransform.position - transform.position).normalized;
        Vector2 avoidanceDirection = GetObstacleAvoidanceDirection(direction);
        enemyInput.SetMovementInput(avoidanceDirection);
        enemyAttack.SetShoot(_playerTransform, false);
    }

    private void AttackBehavior()
    {
        // Маневрирование во время атаки
        if (Random.value < repositionChance)
        {
            Vector2 repositionDirection = GetRepositionDirection();
            enemyInput.SetMovementInput(repositionDirection);
        }
        else
        {
            enemyInput.SetMovementInput(Vector2.zero);
        }

        enemyAttack.SetShoot(_playerTransform, true);
    }

    private void DodgeBehavior()
    {
        // Уворот от снарядов или игрока
        Vector2 dodgeDir = GetDodgeDirection();
        enemyInput.SetMovementInput(dodgeDir);
        enemyAttack.SetShoot(_playerTransform, false);


    }

    private void RetreatBehavior()
    {
        Vector2 retreatDir = (transform.position - _playerTransform.position).normalized;
        Vector2 avoidanceDir = GetObstacleAvoidanceDirection(retreatDir);
        enemyInput.SetMovementInput(avoidanceDir);
        enemyAttack.SetShoot(_playerTransform, false);
    }

    private bool CheckLineOfSight()
    {
        if (_playerTransform == null) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            _playerTransform.position - transform.position,
            detectionRange,
            obstacleMask
        );

        return hit.collider == null || hit.collider.CompareTag("Player");
    }

    private Vector2 GetObstacleAvoidanceDirection(Vector2 desiredDirection)
    {
        // Упрощенное избегание препятствий
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            desiredDirection,
            2f,
            obstacleMask
        );

        return hit.collider != null ? Vector2.Perpendicular(desiredDirection) : desiredDirection;
    }

    private Vector2 GetDodgeDirection()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    private Vector2 GetRepositionDirection()
    {
        return new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
    }

    public void SetState(EnemyState newState)
    {
        currentState = newState;
        
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth-=damageAmount;

        if (currentHealth < 0)
        {
            SetState(EnemyState.Dead);
        }
        Debug.Log("enemy: "+currentHealth);
    }

}