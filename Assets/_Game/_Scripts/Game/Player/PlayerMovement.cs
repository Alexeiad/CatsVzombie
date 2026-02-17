using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Zenject;

public class PlayerMovement : MonoBehaviour, IDamageable<int>
{
    public Action OnDie;
    public int MaxHealth { get; private set; }

    public List<Enemy> currentEnemis;

   

    
    // Реактивные свойства
    public IReadOnlyReactiveProperty<int> CurrentHealth => _currentHealth;
    public IReadOnlyReactiveProperty<bool> IsDead => _isDead;
    public IReadOnlyReactiveProperty<Vector2> MovementDirection => _movementDirection;

    [SerializeField] private EntitiesDataSO _entitySO;
    [SerializeField] private UltraSensitiveDirectionController _animationControl;
    [SerializeField] private Sprite _leftS, _upLeftS, _upS, _rightUpS,
        _rightS, _rightDownS, _DownS, _leftDownS;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _shootSpeedInterval=0.1f;


    [Header("Параметры эффекта")]
    public Color trailColor = Color.white; // Базовый цвет полоски

    private LineRenderer _lr;
    private Material _trailMaterial;

    private ReactiveProperty<int> _currentHealth = new ReactiveProperty<int>();
    private ReactiveProperty<bool> _isDead = new ReactiveProperty<bool>();
    private ReactiveProperty<Vector2> _movementDirection = new ReactiveProperty<Vector2>();

    private PlayerInput _playerInput;
    private DynamicJoystick _joystick;
    private CompositeDisposable _disposables = new CompositeDisposable();
    private Mouse _mouse;
    private SpriteRenderer _spriteRenderer;

    private float _speed;
    private int _health;
    private float speed = 30f;

    private float _enemyDistance = 15f;

    [Inject]
    public void Construct(DynamicJoystick joystick)
    {
        _joystick = joystick;
    }

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _currentHealth.Value = _entitySO.EnemyRows
            .Where(x => x.ID == 0)
            .Select(x => x.Health)
            .FirstOrDefault();
        MaxHealth = _currentHealth.Value;

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

        _lr = GetComponent<LineRenderer>();
        _trailMaterial = _lr.material; // Копируем материал, чтобы не менять общий
        _lr.material = Instantiate(_trailMaterial); // Инстанс для независимой анимации
        _trailMaterial = _lr.material;

        StartCoroutine(AutomaticShoot());
    }
    private void OnEnable()
    {
        //StatsPreview.OnClick += AttackZombie;
    }
    private void OnDisable()
    {
        //StatsPreview.OnClick -= AttackZombie;
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
        if (currentEnemis == null) return;
        if (Mouse.current.leftButton.wasPressedThisFrame)
            AttackZombie();
    }
    private void AttackZombie()
    {

        EnemyTableRow playerData = _entitySO.EnemyRows.Where(x => x.ID == 0).FirstOrDefault();

        Vector3 distance = Vector3.zero;

        Enemy nearestEnemy = currentEnemis.Where(x => x != null)
            .OrderBy(x => EnemyDistance(transform.position, x.transform.position, out distance))
            .FirstOrDefault();

        if (nearestEnemy != null)
        {

            StartCoroutine(Shoot(nearestEnemy, distance, playerData));
            Fire(transform.position, nearestEnemy.transform.position);

        }

    }
    private IEnumerator Shoot(Enemy enemy,Vector3 direction,EnemyTableRow playerData)
    {
        if (Vector3.Distance(transform.position, enemy.transform.position) < _enemyDistance*0.5f)
           
            enemy.Health -= playerData.ShootDamage;
        /*
        if (enemy!=null&&IsEnemyCloseToLineSegment(transform.position, enemy.transform.position, direction))
        {

            enemy.Health -= playerData.ShootDamage;

        }*/
        yield return new WaitForSeconds(0.5f);
        _lr.enabled = false;
    }
    public void Fire(Vector3 startPosition, Vector3 endPosition)
    {
        if (Vector3.Distance(startPosition, endPosition) > _enemyDistance)
            return;

        _lr.enabled = true;
        _animator.enabled = false;
        //_animationControl.ResetAllDirectionBools();
        //_animationControl.enabled = false;

        // Определяем направление
        Vector3 direction = (endPosition - startPosition).normalized;
        SetSpriteByDirection(direction);

        float segmentLength = 0.2f;
        float duration = Vector3.Distance(startPosition, endPosition) / speed;
        float currentTime = 0f;

        DOTween.To(() => currentTime, x => currentTime = x, duration, duration)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                float progress = currentTime / duration;
                float totalDistance = Vector3.Distance(startPosition, endPosition);
                float segmentStartPos = Mathf.Clamp(progress * totalDistance - segmentLength, 0f, totalDistance);
                float segmentEndPos = Mathf.Clamp(progress * totalDistance, segmentLength, totalDistance);

                Vector3 segmentStart = Vector3.Lerp(startPosition, endPosition, segmentStartPos / totalDistance);
                Vector3 segmentEnd = Vector3.Lerp(startPosition, endPosition, segmentEndPos / totalDistance);

                _lr.SetPosition(0, segmentStart);
                _lr.SetPosition(1, segmentEnd);
            })
            .OnComplete(() => {
                _lr.enabled = false;
                StartCoroutine(StartAnimation());
            });
    }
    private IEnumerator AutomaticShoot()
    {
        while (true)
        {
            AttackZombie();
            yield return new WaitForSeconds(_shootSpeedInterval);
            
            
        }
    }
    private IEnumerator StartAnimation()
    {
        yield return new WaitForSeconds(0.3f);
        
        _animationControl.enabled = true;
        _animator.enabled = true;

    }
    private void SetSpriteByDirection(Vector3 direction)
    {

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle < 0) angle += 360;

        if (angle >= 337.5f || angle < 22.5f)
        {
            _spriteRenderer.sprite = _rightS;
        }
        else if (angle >= 22.5f && angle < 67.5f)
        {
            _spriteRenderer.sprite = _rightUpS;
        }
        else if (angle >= 67.5f && angle < 112.5f)
        {
            _spriteRenderer.sprite = _upS;
        }
        else if (angle >= 112.5f && angle < 157.5f)
        {
            _spriteRenderer.sprite = _upLeftS;
        }
        else if (angle >= 157.5f && angle < 202.5f)
        {
            _spriteRenderer.sprite = _leftS;
        }
        else if (angle >= 202.5f && angle < 247.5f)
        {
            _spriteRenderer.sprite = _leftDownS;
        }
        else if (angle >= 247.5f && angle < 292.5f)
        {
            _spriteRenderer.sprite = _DownS;
        }
        else if (angle >= 292.5f && angle < 337.5f)
        {
            _spriteRenderer.sprite = _rightDownS;
        }
    }
 
    private void OnDestroy()
    {
        _disposables.Dispose();
    }
    public float EnemyDistance(Vector3 player, Vector3 enemy, out Vector3 distance)
    {

        distance = enemy - player;

        return Vector3.Distance(player, enemy);
    }
    public static bool IsEnemyCloseToLineSegment(
       Vector3 playerPosition,
       Vector3 enemyPosition,
       Vector3 direction,
       float lineLength = 20f,
       float maxDistance = 2f)
    {
        Vector3 start = playerPosition;

        // Ранний выход: если враг слишком далеко в принципе
        float totalDistSqr = (enemyPosition - start).sqrMagnitude;
        float maxPossibleSqr = (lineLength + maxDistance) * (lineLength + maxDistance);
        if (totalDistSqr > maxPossibleSqr)
            return false;

        // Вырожденный случай: отрезок выродился в точку
        if (direction.sqrMagnitude == 0f || lineLength <= 0f)
        {
            return totalDistSqr <= maxDistance * maxDistance;
        }

        Vector3 normalizedDir = direction.normalized;
        Vector3 end = start + normalizedDir * lineLength;
        Vector3 segment = end - start;

        float segSqrLen = segment.sqrMagnitude;

        // Проекция врага на линию
        float t = Vector3.Dot(enemyPosition - start, segment) / segSqrLen;
        t = Mathf.Clamp01(t); // ближайшая точка строго на отрезке

        Vector3 closestPoint = start + t * segment;

        float sqrDistToLine = (enemyPosition - closestPoint).sqrMagnitude;
        float sqrMaxDist = maxDistance * maxDistance;

        return sqrDistToLine <= sqrMaxDist;
    }

    public void TakeDamage(int damage)
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

        renderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        renderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        renderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        renderer.color = Color.white;

    }

    // Создаем Action через лямбду
    private void OnDeath()
    {
        
        GetComponent<SpriteRenderer>().enabled = false;
        OnDie?.Invoke();

    }
    private void OnMovement()
    {
        // Здесь можно добавить эффекты движения (частицы, звуки и т.д.)
        // Debug.Log("Player is moving");
    }

    // Реактивный метод для лечения
    public void Heal(int amount)
    {
        Observable.NextFrame()
            .Subscribe(_ =>
            {
                _currentHealth.Value = Mathf.Min(_currentHealth.Value + amount, (int)_health);
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