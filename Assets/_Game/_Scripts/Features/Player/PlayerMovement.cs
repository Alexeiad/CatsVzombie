
using UnityEngine;
using Zenject;


public class PlayerMovement : MonoBehaviour,IDamageable<float>
{
    public float Speed;
    public float MaxHealth = 100;
    public float CurrentHealth => _currentHealth;
    public bool IsDead => _currentHealth <= 0;


    private float _currentHealth;

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
        OnPlayerInput();
    }
    private void OnPlayerInput()
    {
        Vector2 dir = _playerInput.GetMovement();
        if (dir != Vector2.zero)
        {
            Vector3 move = new Vector3(dir.x, dir.y, 0) * Speed * Time.deltaTime;
            transform.position += move;
        }
    }

    public void TakeDamage(float damage)
    {
        _currentHealth-=damage;
        Debug.Log("player: "+ _currentHealth);
    }
}
