using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Zenject;

public class PlayerMovement : MonoBehaviour, IDamageable<float>
{
    public List<Enemy> currentEnemis;

    
    // Реактивные свойства
    public IReadOnlyReactiveProperty<float> CurrentHealth => _currentHealth;
    public IReadOnlyReactiveProperty<bool> IsDead => _isDead;
    public IReadOnlyReactiveProperty<Vector2> MovementDirection => _movementDirection;

    [SerializeField] private EntitiesDataSO _entitySO;

    private ReactiveProperty<float> _currentHealth = new ReactiveProperty<float>();
    private ReactiveProperty<bool> _isDead = new ReactiveProperty<bool>();
    private ReactiveProperty<Vector2> _movementDirection = new ReactiveProperty<Vector2>();

    private PlayerInput _playerInput;
    private DynamicJoystick _joystick;
    private CompositeDisposable _disposables = new CompositeDisposable();
    private Mouse _mouse;

    private float _speed;
    private float _health;

    [Inject]
    public void Construct(DynamicJoystick joystick)
    {
        _joystick = joystick;
    }

    void Awake()
    {
        _currentHealth.Value = _entitySO.EnemyRows
            .Where(x => x.ID == 0)
            .Select(x => x.Health)
            .FirstOrDefault();

        _health = _currentHealth.Value;

        _speed = _entitySO.EnemyRows
            .Where(x => x.ID == 0)
            .Select(x => x.Speed)
            .FirstOrDefault();

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

        _mouse = Mouse.current;
    }

    void Start()
    {
        _playerInput = new PlayerInput(_joystick);

        // Реактивное обновление движения каждый кадр
        Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                Vector2 dir = _playerInput.GetMovement();
                _movementDirection.Value = dir;

                // Движение игрока
                if (dir != Vector2.zero && !_isDead.Value)
                {
                    Vector3 move = new Vector3(dir.x, dir.y, 0) * _speed * Time.deltaTime;
                    transform.position += move;
                }
            })
            .AddTo(_disposables);

        

        // Можно добавить визуальные эффекты при движении
        _movementDirection
            .Where(dir => dir != Vector2.zero)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Subscribe(_ => OnMovement())
            .AddTo(_disposables);
    }
    private void Update()
    {
        AttackZombie();
    }
    private void AttackZombie()
    {
        if(currentEnemis==null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            var playerData = _entitySO.EnemyRows.Where(x => x.ID == 0).FirstOrDefault();

            var nearestEnemy = currentEnemis.Where(x=>x!=null)
                .OrderBy(x => Vector3.Distance(transform.position, x.transform.position))
                .FirstOrDefault();

            if (nearestEnemy != null)
            {
                nearestEnemy.Health -= playerData.ShootDamage;
            }
        }
    }
    private void OnDestroy()
    {
        _disposables.Dispose();
    }

    public void TakeDamage(float damage)
    {
        if (_isDead.Value) return;

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

        // Визуальные эффекты смерти
        Observable.Timer(TimeSpan.FromSeconds(2))
            .Subscribe(_ => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex))
            .AddTo(_disposables);
    }

    private void OnMovement()
    {
        // Здесь можно добавить эффекты движения (частицы, звуки и т.д.)
        // Debug.Log("Player is moving");
    }

    // Реактивный метод для лечения
    public void Heal(float amount)
    {
        Observable.NextFrame()
            .Subscribe(_ =>
            {
                _currentHealth.Value = Mathf.Min(_currentHealth.Value + amount, _health);
            })
            .AddTo(_disposables);
    }

    // Реактивный метод для сброса здоровья
    public void ResetHealth()
    {
        Observable.NextFrame()
            .Subscribe(_ =>
            {
                _currentHealth.Value = _health;
                _isDead.Value = false;
                enabled = true;
                var collider = GetComponent<Collider2D>();
                if (collider != null) collider.enabled = true;
            })
            .AddTo(_disposables);
    }
}