using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Enemy2D : MonoBehaviour
{
    [Header("References")]
    public Transform player; // перетащите игрока в инспектор или найдите по тегу в Start()
    public Animator animator; // опционально

    [Header("Movement")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3f;
    public Transform pointA;
    public Transform pointB;
    public bool usePatrol = true;

    [Header("Detection & Attack")]
    public float detectionRadius = 6f;
    public float attackRadius = 1f;
    public int damage = 1;
    public float attackCooldown = 1.2f;

    [Header("Stats")]
    public int maxHealth = 5;

    // internal
    int currentHealth;
    Vector2 targetPosition;
    bool goingToB = true;
    float lastAttackTime = -999f;
    Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        // если нет точек патрул€ Ч не патрулируем
        if (pointA == null || pointB == null) usePatrol = false;
        if (usePatrol)
            targetPosition = pointB.position;
    }

    void Update()
    {
        if (player != null)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);

            if (distToPlayer <= attackRadius)
            {
                // атака
                TryAttack();
                SetAnimator("isMoving", false);
            }
            else if (distToPlayer <= detectionRadius)
            {
                // преследование игрока
                MoveTowards(player.position, chaseSpeed);
                SetAnimator("isMoving", true);
            }
            else
            {
                // патруль или стоим
                if (usePatrol)
                    Patrol();
                else
                    SetAnimator("isMoving", false);
            }
        }
        else
        {
            // если нет игрока Ч патруль / стоим
            if (usePatrol)
                Patrol();
            else
                SetAnimator("isMoving", false);
        }
    }

    void Patrol()
    {
        if (pointA == null || pointB == null) return;

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            goingToB = !goingToB;
            targetPosition = goingToB ? pointB.position : pointA.position;
        }

        MoveTowards(targetPosition, patrolSpeed);
        SetAnimator("isMoving", true);
    }

    void MoveTowards(Vector2 dest, float speed)
    {
        Vector2 direction = ((Vector2)dest - (Vector2)transform.position).normalized;
        rb.velocity = direction * speed;

        // ѕоворачивать спрайт по оси X (если нужно)
        if (direction.x > 0.01f) transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < -0.01f) transform.localScale = new Vector3(-1, 1, 1);
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        SetAnimatorTrigger("Attack");

        // тут простой способ нанести урон Ч провер€ем игрока по радиусу
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius);
        foreach (var c in hits)
        {
            if (c.transform == player)
            {
                // ожидаем что у игрока есть компонент с методом TakeDamage(int)
                var playerHealth = c.GetComponent<PlayerHealth>(); // пример
                if (playerHealth != null)
                    playerHealth.TakeDamage(damage);
                else
                {
                    // если у игрока другой скрипт Ч можно вызывать IMessage или интерфейс
                    var receiver = c.GetComponent<IDamageable>();
                    if (receiver != null) receiver.TakeDamage(damage);
                }
            }
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        SetAnimatorTrigger("Hurt");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        SetAnimatorTrigger("Die");
        // отключаем коллайдер и движение
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
        rb.velocity = Vector2.zero;
        this.enabled = false; // либо Destroy(gameObject, 1f) после анимации
        Destroy(gameObject, 1.5f);
    }

    // вспомогательные методы дл€ аниматора
    void SetAnimator(string param, bool value)
    {
        if (animator) animator.SetBool(param, value);
    }

    void SetAnimatorTrigger(string trigger)
    {
        if (animator) animator.SetTrigger(trigger);
    }

    // визуализаци€ радиусов в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        if (pointA) { Gizmos.color = Color.cyan; Gizmos.DrawSphere(pointA.position, 0.08f); }
        if (pointB) { Gizmos.color = Color.cyan; Gizmos.DrawSphere(pointB.position, 0.08f); }
    }
}

// ѕримеры интерфейсов/скриптов, которые может ожидать враг:
public interface IDamageable { void TakeDamage(int amount); }

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public int hp = 10;
    public void TakeDamage(int amount)
    {
        hp -= amount;
        Debug.Log("Player took " + amount + " damage. HP left: " + hp);
        if (hp <= 0) Debug.Log("Player dead");
    }
}

