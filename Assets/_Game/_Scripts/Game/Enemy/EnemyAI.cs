using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;
using System;
using System.Collections;


public class EnemyAI : MonoBehaviour, IDamageable<float>
{
    public float MaxHealth = 100;
    public FloatReactiveProperty currentHealth = new FloatReactiveProperty();

    [Header("AI Settings")]
    public float detectionRange = 8f;
    public float attackRange = 5f;
    public float retreatRange = 2f;
    public float decisionRate = 0.5f;
    public LayerMask obstacleMask;

    [Header("Behavior Chances")]
    public float dodgeChance = 0.3f;
    public float repositionChance = 0.4f;

    private ReactiveProperty<EnemyState> currentState = new ReactiveProperty<EnemyState>();
    private Transform _playerTransform;
    private EnemyInput enemyInput;
    private EnemyAttack enemyAttack;

    private CompositeDisposable disposables = new CompositeDisposable();

    [Inject] private EntityList _entities;

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
        currentHealth.Value = MaxHealth;
    }

    private void Start()
    {
        // Реактивная подписка на изменения состояния
        currentState.Subscribe(state => OnStateChanged(state)).AddTo(disposables);

        // Реактивная подписка на здоровье
        currentHealth
            .Where(health => health <= 0)
            .Subscribe(_ => SetState(EnemyState.Dead))
            .AddTo(disposables);

        
        Observable.Interval(TimeSpan.FromSeconds(decisionRate))
            .Subscribe(_ => MakeDecision())
            .AddTo(disposables);

        // Непрерывное обновление состояния
        Observable.EveryUpdate()
            .Subscribe(_ => UpdateState())
            .AddTo(disposables);

        SetState(EnemyState.Patrol);
    }

    private void OnDestroy()
    {
        disposables.Dispose();
    }

    private void MakeDecision()
    {
        if (_playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
        bool hasLineOfSight = CheckLineOfSight();

        switch (currentState.Value)
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
                else if (UnityEngine.Random.value < dodgeChance)
                    SetState(EnemyState.Dodge);
                break;

            case EnemyState.Dodge:
                // Автоматический возврат к атаке после уворота
                Observable.Timer(TimeSpan.FromSeconds(1f))
                    .TakeUntilDisable(this)
                    .Subscribe(_ => SetState(EnemyState.Attack));
                break;

            case EnemyState.Retreat:
                if (distanceToPlayer > retreatRange && hasLineOfSight)
                    SetState(EnemyState.Attack);
                break;
        }
    }

    private void UpdateState()
    {
        switch (currentState.Value)
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

    private void OnStateChanged(EnemyState newState)
    {
 
        if (newState == EnemyState.Dodge)
        {
            StartCoroutine(FlashEffect());
        }
    }

    private IEnumerator FlashEffect()
    {
        var renderer = GetComponent<SpriteRenderer>();
        Color originalColor = renderer.color;
        renderer.color = Color.yellow;
        yield return new WaitForSeconds(0.2f);
        renderer.color = originalColor;
    }

    private void PatrolBehavior()
    {
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
        if (UnityEngine.Random.value < repositionChance)
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

        return hit.collider == null;
    }

    private Vector2 GetObstacleAvoidanceDirection(Vector2 desiredDirection)
    {
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
        return new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
    }

    private Vector2 GetRepositionDirection()
    {
        return new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));
    }

    public void SetState(EnemyState newState)
    {
        currentState.Value = newState;
    }

    private void Die()
    {
        // Реактивные события при смерти
        Observable.Timer(TimeSpan.FromSeconds(0.5f))
            .Subscribe(_ => Destroy(gameObject))
            .AddTo(disposables);
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth.Value -= damageAmount;
        Debug.Log("enemy: " + currentHealth.Value);

        // Реакция на получение урона
        if (currentState.Value != EnemyState.Dead)
        {
            StartCoroutine(DamageFlash());
        }
    }

    private IEnumerator DamageFlash()
    {
        var renderer = GetComponent<SpriteRenderer>();
        Color originalColor = renderer.color;
        renderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        renderer.color = originalColor;
    }

    // Реактивное свойство для внешнего доступа к состоянию
    public IReadOnlyReactiveProperty<EnemyState> CurrentState => currentState;
    public IReadOnlyReactiveProperty<float> Health => currentHealth;
}

