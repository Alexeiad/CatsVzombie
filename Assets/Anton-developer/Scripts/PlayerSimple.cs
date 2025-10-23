using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSimple : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;   // скорость движения по горизонтали
    public bool canMove = true;

    [Header("Health")]
    public int maxHealth = 10;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (canMove)
            Move();
    }

    void Move()
    {
        // движение по горизонтали (A/D, ←/→)
        float moveX = Input.GetAxisRaw("Horizontal");

        // движение по вертикали (W/S, ↑/↓)
        float moveY = Input.GetAxisRaw("Vertical");

        // общий вектор направления
        Vector2 moveDir = new Vector2(moveX, moveY).normalized;

        // перемещаем игрока
        transform.Translate(moveDir * moveSpeed * Time.deltaTime);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"Player took {amount} damage. HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log("Player died!");
            Destroy(gameObject); // или вызвать экран проигрыша
        }
    }
}
