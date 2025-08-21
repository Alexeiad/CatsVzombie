
using UnityEngine;
using Zenject;

public class PlayerBehaviour : MonoBehaviour
{
    public float Speed;
    public float MaxHealth = 100;
    public float CurrentHealth => _currentHealth;
    public bool IsDead => _currentHealth <= 0;


    private float _currentHealth;
    private float _damageMultiplier = 1f;
    private float _damageReduction = 0f; 

    private PlayerInput _playerInput;
    private DynamicJoystick _joystick;

    [Inject]
    public void Construct(DynamicJoystick joystick)
    {
        _joystick = joystick;
    }

    void Awake()
    {
        _currentHealth = MaxHealth;
    }
    void Start()
    {
        _playerInput = new PlayerInput(_joystick);
  
    }

    void Update()
    {
        Vector2 dir = _playerInput.GetMovement();
        if (dir != Vector2.zero)
        {
            Vector3 move = new Vector3(dir.x, dir.y, 0) * Speed * Time.deltaTime;
            transform.position += move;
        }
    }


    public void ApplyDamageUpgrade(float multiplier)
    {
        _damageMultiplier += multiplier;  // накопительно увеличивает урон
    }

    public void ApplyDefenseUpgrade(float reduction)
    {
        _damageReduction += reduction;   // например, 0.1 - 10%
        _damageReduction = Mathf.Clamp01(_damageReduction); // чтобы не больше 100%
    }
    public void TakeDamage(GameObject source, float damage)
    {
        // ≈сли источник - враг, то примен€ем защиту
        if (source.CompareTag("Enemy"))
        {
            float reducedDamage = damage * (1 - _damageReduction);
            _currentHealth -= reducedDamage;
        }
        else if (source.CompareTag("Player"))
        {
            // ќбычное повреждение (если есть случаи)
            _currentHealth -= damage;
        }
        else
        {
            // ≈сли источник не определЄн, просто наносим урон
            _currentHealth -= damage;
        }

        _currentHealth = Mathf.Max(_currentHealth, 0);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    public float GetDamage(float baseDamage)
    {
        return baseDamage * _damageMultiplier;
    }

    private void Die()
    {
        Debug.Log("Player Died");
        // “ут логика смерти игрока
    }



}
