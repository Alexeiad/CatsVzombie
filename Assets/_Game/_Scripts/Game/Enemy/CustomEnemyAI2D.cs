using UnityEngine;
using System.Collections.Generic;
using Zenject;
using UnityEngine.Timeline;
using Unity.VisualScripting;

/// <summary>
/// Кастомный 2D AI без NavMesh и без поворота объекта.
/// Три режима поведения:
/// 1. MeleeAttack — цикл: быстро подходит вплотную (атака), отходит на retreatDistance,
///    потом патрулирует вокруг цели (точно как в RangedPatrol) 1-3 секунды,
///    потом снова бросается в атаку.
/// 2. RangedPatrol — подход на safeDistance + случайный патруль вокруг.
/// 3. Flee — убегает от цели.
/// Обход препятствий — отталкивание от объектов в списке.
/// + Плавная сепарация от других врагов (без тряски и наложения)
/// </summary>
public class CustomEnemyAI2D : MonoBehaviour, IEnemyAIConfig
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

    public float PursuitDistance { get; set; }
    public float Speed { get; set; }
    public float AvoidanceStrength { get; set; }
    public float AvoidanceDistance { get; set; }
    public float AttackDistance { get; set; }
    public float RetreatDistance { get; set; }
    public float MinPauseTime { get; set; }
    public float MaxPauseTime { get; set; }
    public float PatrolVariation { get; set; }
    public float TangentialSpeed { get; set; }
    public float ChangeDirectionMin { get; set; }
    public float ChangeDirectionMax { get; set; }
    public float SafeDistance { get; set; }
    public float FleeSpeedMultiplier { get; set; }

    [Header("Режим поведения")]
    public AIState currentState = AIState.MeleeAttack;

    // Параметры колебания дистанции патруля (делаем поведение более живым)
    [Header("Колебание дистанции патруля")]
    [Range(0f, 0.3f)] public float patrolOscillationAmplitude = 0.15f; // ±15%
    [Range(0.1f, 2f)] public float patrolOscillationFrequency = 0.8f;

    // === Плавная сепарация от других врагов (без тряски) ===
    [Header("Сепарация от других врагов (плавно, без тряски и наложения)")]
    [SerializeField] private float enemySeparationDistance = 2.0f;   // Радиус отталкивания (увеличен — начинают расходиться заранее)
    [SerializeField] private float enemySeparationStrength = 5.0f;   // Сила отталкивания (сбалансировано — нет тряски)
   


    private float patrolPhase; // Рандомная фаза для каждого врага

    private EntitiesDataSO _entityDataSO;
    private EntityType _entityType;

    // Внутренние переменные
    private MeleeSubState meleeSubState = MeleeSubState.Approaching;
    private float patrolTimer;                  // Для смены направления в патруле
    private int tangentialDirection = 1;        // Направление касательного движения
    private float waitTimer;                    // Время патруля в Melee перед новой атакой
    private Enemy _enemy;
    private Transform _target;
    private List<Transform> _obstacles;
    private List<Enemy> _enemis;
    private PlayerMovement _playerMovement;

    public void Initialize(PlayerMovement playerMovement, List<Enemy> enemis)
    {
        _enemy = GetComponent<Enemy>();
        _enemis = enemis;
        _target = playerMovement.transform;
        _obstacles = _enemy.ObstacleList;
        _entityDataSO = _enemy.entitiesDataSO;
        _entityType = _enemy.EntityType;
        _playerMovement = playerMovement;

        patrolTimer = Random.Range(ChangeDirectionMin, ChangeDirectionMax);
        tangentialDirection = Random.value > 0.5f ? 1 : -1;
        meleeSubState = MeleeSubState.Approaching;

        // Рандомная фаза колебания для каждого экземпляра
        patrolPhase = Random.Range(0f, Mathf.PI * 2f);

        DbInit();
        RandomizeParameters();
    }

    private void Update()
    {
        if (_target == null) return;

        Vector2 toTarget = _target.position - transform.position;
        float distToTarget = toTarget.magnitude;

        if (distToTarget >= PursuitDistance)
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

        // Ограничиваем скорость строго Speed (чтобы не было лишних ускорений и тряски)
        if (finalVelocity.sqrMagnitude > Speed * Speed)
        {
            finalVelocity = finalVelocity.normalized * Speed;
        }
        else if (finalVelocity.sqrMagnitude > 0.01f)
        {
            finalVelocity = finalVelocity.normalized * Speed;
        }

        transform.position += (Vector3)finalVelocity * Time.deltaTime;
    }

    private void DbInit()
    {
        foreach (var entity in _entityDataSO.EnemyRows)
        {
            if (_entityType == entity.EntityType)
            {
                PursuitDistance = entity.PursuitDistance;
                Speed = entity.Speed;
                AvoidanceStrength = entity.AvoidanceStrength;
                AvoidanceDistance = entity.AvoidanceDistance;
                AttackDistance = entity.AttackDistance;
                RetreatDistance = entity.RetreatDistance;
                MinPauseTime = entity.MinPauseTime;
                MaxPauseTime = entity.MaxPauseTime;
                PatrolVariation = entity.PatrolVariation;
                TangentialSpeed = entity.TangentialSpeed;
                ChangeDirectionMin = entity.ChangeDirectionMin;
                ChangeDirectionMax = entity.ChangeDirectionMax;
                SafeDistance = entity.SafeDistance;
                FleeSpeedMultiplier = entity.FleeSpeedMultiplier;
            }
        }
    }

    // Рандомизация параметров при создании врага
    private void RandomizeParameters()
    {
        float distVariation = 0.20f; // ±20%
        float pauseVariation = 0.25f; // ±25% для пауз

        AttackDistance *= Random.Range(1f - distVariation, 1f + distVariation);
        RetreatDistance *= Random.Range(1f - distVariation, 1f + distVariation);
        SafeDistance *= Random.Range(1f - distVariation, 1f + distVariation);
        PursuitDistance *= Random.Range(1f - distVariation, 1f + distVariation);

        if (AttackDistance >= RetreatDistance)
        {
            AttackDistance = RetreatDistance * Random.Range(0.5f, 0.8f);
        }

        float baseMin = MinPauseTime;
        float baseMax = MaxPauseTime;
        MinPauseTime = baseMin * Random.Range(1f - pauseVariation, 1f + pauseVariation);
        MaxPauseTime = baseMax * Random.Range(1f - pauseVariation, 1f + pauseVariation);

        if (MinPauseTime > MaxPauseTime)
        {
            float mid = (MinPauseTime + MaxPauseTime) * 0.5f;
            MinPauseTime = mid * 0.8f;
            MaxPauseTime = mid * 1.3f;
        }
    }

    // MeleeAttack с патрулём после отхода
    private Vector2 MeleeAttackDirection(Vector2 toTarget, float dist)
    {
        switch (meleeSubState)
        {
            case MeleeSubState.Approaching:
                if (dist <= AttackDistance)
                {
                    meleeSubState = MeleeSubState.Retreating;
                }
                return toTarget.normalized * Speed;

            case MeleeSubState.Retreating:
                if (dist >= RetreatDistance)
                {
                    meleeSubState = MeleeSubState.Patrolling;
                    waitTimer = Random.Range(MinPauseTime, MaxPauseTime);
                    patrolTimer = Random.Range(ChangeDirectionMin, ChangeDirectionMax);
                    tangentialDirection = Random.value > 0.5f ? 1 : -1;
                }
                return -toTarget.normalized * Speed;

            case MeleeSubState.Patrolling:
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    meleeSubState = MeleeSubState.Approaching;
                    return Vector2.zero;
                }

                return GetPatrolVelocity(toTarget, dist, RetreatDistance);

            default:
                return Vector2.zero;
        }
    }

    private Vector2 RangedPatrolDirection(Vector2 toTarget, float dist)
    {
        return GetPatrolVelocity(toTarget, dist, SafeDistance);
    }

    // Общая логика патруля с колеблющейся дистанцией
    private Vector2 GetPatrolVelocity(Vector2 toTarget, float dist, float desiredDistance)
    {
        float oscillation = Mathf.Sin(Time.time * patrolOscillationFrequency + patrolPhase);
        float currentDesiredDistance = desiredDistance * (1f + patrolOscillationAmplitude * oscillation);

        if (dist > currentDesiredDistance + PatrolVariation)
        {
            return toTarget.normalized * Speed;
        }
        else if (dist < currentDesiredDistance - PatrolVariation)
        {
            return -toTarget.normalized * Speed;
        }

        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0f)
        {
            patrolTimer = Random.Range(ChangeDirectionMin, ChangeDirectionMax);
            tangentialDirection *= -1;
        }

        Vector2 tangential = new Vector2(-toTarget.y, toTarget.x).normalized * tangentialDirection * TangentialSpeed;
        float radialNoise = Random.Range(-0.3f, 0.3f) * Speed;
        Vector2 radial = toTarget.normalized * radialNoise;

        return tangential + radial;
    }

    private Vector2 FleeDirection(Vector2 toTarget)
    {
        return -toTarget.normalized * Speed * FleeSpeedMultiplier;
    }

    // === Плавная сепарация: линейная сила + усреднение направления (нет тряски) ===
    private Vector2 CalculateAvoidance()
    {
        Vector2 avoidance = Vector2.zero;

        // 1. Отталкивание от препятствий (как было — линейное, плавное)
        foreach (Transform obstacle in _obstacles)
        {
            if (obstacle == null) continue;

            Vector2 toObstacle = (Vector2)(transform.position - obstacle.position);
            float dist = toObstacle.magnitude;

            if (dist < AvoidanceDistance && dist > 0.01f)
            {
                float strength = (AvoidanceDistance - dist) / AvoidanceDistance;
                avoidance += toObstacle.normalized * AvoidanceStrength * strength;
            }
        }

        // 2. Плавная сепарация от других врагов (линейный вес + усреднение = нет тряски)
        if (_enemis != null)
        {
            Vector2 separation = Vector2.zero;
            int neighborCount = 0;

            foreach (Enemy other in _enemis)
            {
                if (other == null || other.transform == transform) continue;

                Vector2 toOther = (Vector2)(transform.position - other.transform.position);
                float dist = toOther.magnitude;

                if (dist < enemySeparationDistance && dist > 0.01f)
                {
                    // Линейный вес: чем ближе — тем сильнее, но плавно до 0 на границе радиуса
                    float weight = 1f - (dist / enemySeparationDistance);
                    separation += toOther.normalized * weight;
                    neighborCount++;
                }
            }

            if (neighborCount > 0)
            {
                separation /= neighborCount; // Усредняем направление
                avoidance += separation.normalized * enemySeparationStrength;
            }
        }

        return avoidance;
    }
}