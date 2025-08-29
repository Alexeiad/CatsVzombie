using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Components")]
    public EnemyAI enemyAI;
    public EnemyInput enemyInput;
    public EnemyAttack enemyAttack;
    public Animator animator;

    private void Awake()
    {
        // Автоматическое получение компонентов
        enemyAI = GetComponent<EnemyAI>();
        enemyInput = GetComponent<EnemyInput>();
        enemyAttack = GetComponent<EnemyAttack>();
        animator = GetComponent<Animator>();

        SetupEventListeners();
    }

    private void SetupEventListeners()
    {
        if (enemyInput != null)
        {
            enemyInput.OnMovementChanged += HandleMovement;
            enemyInput.OnShootingChanged += HandleShooting;
        }
    }

    private void HandleMovement(Vector2 movement)
    {
        // Обновление анимации
        if (animator != null)
        {
            animator.SetFloat("MoveX", movement.x);
            animator.SetFloat("MoveY", movement.y);
            animator.SetBool("IsMoving", movement != Vector2.zero);
        }
    }

    private void HandleShooting(bool isShooting)
    {
        if (animator != null)
        {
            animator.SetBool("IsAttacking", isShooting);
        }
    }

    private void OnDestroy()
    {
        // Отписка от событий
        if (enemyInput != null)
        {
            enemyInput.OnMovementChanged -= HandleMovement;
            enemyInput.OnShootingChanged -= HandleShooting;
        }
    }
}