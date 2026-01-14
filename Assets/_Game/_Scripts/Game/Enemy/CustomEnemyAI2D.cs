using UnityEngine;
using System.Collections.Generic;
using Zenject;

/// <summary>
/// Кастомный 2D AI без NavMesh и без поворота объекта.
/// Три режима поведения:
/// 1. MeleeAttack — цикл: быстро подходит вплотную (атака), отходит на retreatDistance,
///    потом патрулирует вокруг цели (точно как в RangedPatrol) 1-3 секунды,
///    потом снова бросается в атаку.
/// 2. RangedPatrol — подход на safeDistance + случайный патруль вокруг.
/// 3. Flee — убегает от цели.
/// Обход препятствий — отталкивание от объектов в списке.
/// </summary>
public class CustomEnemyAI2D : MonoBehaviour
{
    public enum AIState
    {
        MeleeAttack,
        RangedPatrol,
        Flee
    }

    private enum MeleeSubState
    {
        Approaching,    // Подходит к цели
        Retreating,     // Отходит назад
        Patrolling      // Патрулирует вокруг после отхода (вместо стояния)
    }

    [Header("Основные настройки")]
    
    public float pursuitDistance = 10f;
    public float speed = 4f;
    public float avoidanceStrength = 15f;
    public float avoidanceDistance = 3f;

    [Header("Препятствия для обхода")]
    public List<Transform> obstacles = new List<Transform>();

    [Header("Режим поведения")]
    public AIState currentState = AIState.MeleeAttack;

    [Header("Параметры MeleeAttack")]
    public float attackDistance = 1.5f;
    public float retreatDistance = 4f;
    public float minPauseTime = 1f;
    public float maxPauseTime = 3f;

    [Header("Общие параметры патруля (используются в RangedPatrol и в Melee после отхода)")]
    public float patrolVariation = 1.5f;
    public float tangentialSpeed = 2f;
    public float changeDirectionMin = 1.5f;
    public float changeDirectionMax = 4f;

    [Header("Параметры RangedPatrol")]
    public float safeDistance = 3f;

    [Header("Параметры Flee")]
    public float fleeSpeedMultiplier = 1.5f;

    // Внутренние переменные
    private MeleeSubState meleeSubState = MeleeSubState.Approaching;
    private float patrolTimer;                  // Для смены направления в патруле (общий для обоих режимов)
    private int tangentialDirection = 1;        // Общий для обоих режимов патруля
    private float waitTimer;                    // Время патруля в Melee перед новой атакой
    private Enemy _enemy;
    private Transform target;
    [Inject] private PlayerMovement _playerMovement;

    private void Start()
    {
        _enemy = GetComponent<Enemy>();

       // target = _enemy?.PlayerMovement.transform;
        
        target = _playerMovement.transform;
        
        patrolTimer = Random.Range(changeDirectionMin, changeDirectionMax);
        tangentialDirection = Random.value > 0.5f ? 1 : -1;
        meleeSubState = MeleeSubState.Approaching;
    }

    private void Update()
    {
        if (target == null) return;

        Vector2 toTarget = target.position - transform.position;
        float distToTarget = toTarget.magnitude;

        if (distToTarget >= pursuitDistance)
        {
            meleeSubState = MeleeSubState.Approaching;
            return;
        }

        Vector2 desiredVelocity = Vector2.zero;

        switch (currentState)
        {
            case AIState.MeleeAttack:
                desiredVelocity = MeleeAttackDirection(toTarget, distToTarget);
                break;

            case AIState.RangedPatrol:
                desiredVelocity = RangedPatrolDirection(toTarget, distToTarget);
                break;

            case AIState.Flee:
                desiredVelocity = FleeDirection(toTarget);
                break;
        }

        Vector2 avoidanceVelocity = CalculateAvoidance();

        Vector2 finalVelocity = desiredVelocity + avoidanceVelocity;

        if (finalVelocity.sqrMagnitude > 0.01f)
        {
            finalVelocity = finalVelocity.normalized * speed;
        }

        transform.position += (Vector3)finalVelocity * Time.deltaTime;
    }

    // MeleeAttack с патрулём после отхода
    private Vector2 MeleeAttackDirection(Vector2 toTarget, float dist)
    {
        switch (meleeSubState)
        {
            case MeleeSubState.Approaching:
                if (dist <= attackDistance)
                {
                    meleeSubState = MeleeSubState.Retreating;
                }
                return toTarget.normalized * speed;

            case MeleeSubState.Retreating:
                if (dist >= retreatDistance)
                {
                    // Переходим в патруль
                    meleeSubState = MeleeSubState.Patrolling;
                    waitTimer = Random.Range(minPauseTime, maxPauseTime);
                    // Сбрасываем параметры патруля для разнообразия
                    patrolTimer = Random.Range(changeDirectionMin, changeDirectionMax);
                    tangentialDirection = Random.value > 0.5f ? 1 : -1;
                }
                return -toTarget.normalized * speed;

            case MeleeSubState.Patrolling:
                // Отсчитываем общее время патруля
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    meleeSubState = MeleeSubState.Approaching;
                    return Vector2.zero;
                }

                // Точно такая же логика патруля, как в RangedPatrol, но с desiredDistance = retreatDistance
                return GetPatrolVelocity(toTarget, dist, retreatDistance);

            default:
                return Vector2.zero;
        }
    }

    // RangedPatrol (без изменений, но вынесена логика в общий метод)
    private Vector2 RangedPatrolDirection(Vector2 toTarget, float dist)
    {
        return GetPatrolVelocity(toTarget, dist, safeDistance);
    }

    // Общая логика патруля (используется и в Melee, и в Ranged)
    private Vector2 GetPatrolVelocity(Vector2 toTarget, float dist, float desiredDistance)
    {
        // Радиальная коррекция, если слишком далеко/близко
        if (dist > desiredDistance + patrolVariation)
        {
            return toTarget.normalized * speed;
        }
        else if (dist < desiredDistance - patrolVariation)
        {
            return -toTarget.normalized * speed;
        }

        // Смена направления по таймеру
        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0f)
        {
            patrolTimer = Random.Range(changeDirectionMin, changeDirectionMax);
            tangentialDirection *= -1;
        }

        // Касательное движение + небольшой шум по радиусу
        Vector2 tangential = new Vector2(-toTarget.y, toTarget.x).normalized * tangentialDirection * tangentialSpeed;
        float radialNoise = Random.Range(-0.3f, 0.3f) * speed;
        Vector2 radial = toTarget.normalized * radialNoise;

        return tangential + radial;
    }

    // Flee
    private Vector2 FleeDirection(Vector2 toTarget)
    {
        return -toTarget.normalized * speed * fleeSpeedMultiplier;
    }

    // Обход препятствий
    private Vector2 CalculateAvoidance()
    {
        Vector2 avoidance = Vector2.zero;

        foreach (Transform obstacle in obstacles)
        {
            if (obstacle == null) continue;

            Vector2 toObstacle = transform.position - obstacle.position;
            float dist = toObstacle.magnitude;

            if (dist < avoidanceDistance)
            {
                float strength = (avoidanceDistance - dist) / avoidanceDistance;
                avoidance += toObstacle.normalized * avoidanceStrength * strength;
            }
        }

        return avoidance;
    }
}