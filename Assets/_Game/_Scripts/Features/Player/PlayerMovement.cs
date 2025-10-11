using UnityEngine;
using Zenject;
using UniRx;
using System;
using UnityEngine.SceneManagement;


public class PlayerMovement : MonoBehaviour, IDamageable<float>
{
    public float Speed;
    public float MaxHealth = 100;

    // Реактивные свойства
    public IReadOnlyReactiveProperty<float> CurrentHealth => _currentHealth;
    public IReadOnlyReactiveProperty<bool> IsDead => _isDead;
    public IReadOnlyReactiveProperty<Vector2> MovementDirection => _movementDirection;

    private ReactiveProperty<float> _currentHealth = new ReactiveProperty<float>();
    private ReactiveProperty<bool> _isDead = new ReactiveProperty<bool>();
    private ReactiveProperty<Vector2> _movementDirection = new ReactiveProperty<Vector2>();

    private PlayerInput _playerInput;
    private DynamicJoystick _joystick;
    private CompositeDisposable _disposables = new CompositeDisposable();

    [Inject]
    public void Construct(DynamicJoystick joystick)
    {
        _joystick = joystick;
    }

    void Awake()
    {
        _currentHealth.Value = MaxHealth;

        // Реактивная подписка на смерть
        _currentHealth
            .Select(health => health <= 0)
            .DistinctUntilChanged()
            .Subscribe(isDead => _isDead.Value = isDead)
            .AddTo(_disposables);

        // Реакция на смерть
        _isDead
            .Where(isDead => isDead)
            .Subscribe(_ => OnDeath())
            .AddTo(_disposables);
    }

    void Start()
    {
        _playerInput = new PlayerInput(_joystick);

        // Реактивное обновление движения каждый кадр
        Observable.EveryUpdate()
            .Subscribe(_ => UpdateMovement())
            .AddTo(_disposables);

        // Можно добавить визуальные эффекты при движении
        _movementDirection
            .Where(dir => dir != Vector2.zero)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Subscribe(_ => OnMovement())
            .AddTo(_disposables);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }

    private void UpdateMovement()
    {
        Vector2 dir = _playerInput.GetMovement();
        _movementDirection.Value = dir;

        if (dir != Vector2.zero)
        {
            Vector3 move = new Vector3(dir.x, dir.y, 0) * Speed * Time.deltaTime;
            transform.position += move;
        }
    }

    public void TakeDamage(float damage)
    {
        // Реактивное применение урона
        Observable.NextFrame()
            .Subscribe(_ =>
            {
                _currentHealth.Value -= damage;


                // Визуальный эффект при получении урона
                OnDamageTaken(damage);
            })
            .AddTo(_disposables);
    }

    private void OnDamageTaken(float damage)
    {
        // Можно добавить визуальные эффекты, звуки и т.д.


        // Мигание спрайта при получении урона
        StartCoroutine(DamageFlashCoroutine());
    }

    private System.Collections.IEnumerator DamageFlashCoroutine()
    {
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.color;
            renderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            renderer.color = originalColor;
        }
    }

    private void OnDeath()
    {

        // Отключаем управление и коллайдер
        enabled = false;
        var collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        // Визуальные эффекты смерти
        Observable.Timer(TimeSpan.FromSeconds(2))
            .Subscribe(_ => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex))
            .AddTo(_disposables);
    }

    private void OnMovement()
    {
      
    }

    // Реактивный метод для лечения
    public void Heal(float amount)
    {
        Observable.NextFrame()
            .Subscribe(_ =>
            {
                _currentHealth.Value = Mathf.Min(_currentHealth.Value + amount, MaxHealth);
            })
            .AddTo(_disposables);
    }

    // Реактивный метод для сброса здоровья
    public void ResetHealth()
    {
        Observable.NextFrame()
            .Subscribe(_ =>
            {
                _currentHealth.Value = MaxHealth;
                enabled = true;
                var collider = GetComponent<Collider2D>();
                if (collider != null) collider.enabled = true;
            })
            .AddTo(_disposables);
    }
}