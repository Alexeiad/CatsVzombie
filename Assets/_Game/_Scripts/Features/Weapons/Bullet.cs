using DG.Tweening;
using UnityEngine;
using Zenject;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int playerDamage = 10;
    [SerializeField] private int enemyDamage = 5;

    private PlayerMovement _playerMovement;
    private Transform _finishPoint;
    private Vector3 _position;
    private Vector3 _startPoint;

    [Inject]
    private void Construct(PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;

    }
    private void Start()
    {
        _startPoint = transform.position;
    }
    public void MoveToContainer(Transform finishPoint)
    {
        _finishPoint = finishPoint;
        _position = _finishPoint.position;
    }
    private void Update()
    {
        if (_finishPoint != null)
        {
            Vector3 direction = (_finishPoint.position - _startPoint).normalized;
            float distanceFromStart = Vector3.Distance(_startPoint, transform.position);

            transform.position += direction * 30f * Time.deltaTime;

            if (Vector3.Distance(_finishPoint.position, transform.position) < 0.25f)
            {
                SetDamage();
            }

            if (distanceFromStart > 100f)
            {
                Destroy(gameObject);
            }
        }
        else
            Destroy(gameObject);
        

    }

    private void SetDamage()
    {
        
        
            if (_finishPoint.TryGetComponent<PlayerMovement>(out var playerMovement))
            {
                playerMovement.TakeDamage(playerDamage);
            }
            else if (_finishPoint.TryGetComponent<EnemyAI>(out var enemyAI))
            {
                enemyAI.TakeDamage(enemyDamage);

            }

            Destroy(gameObject);
        

        
    }
}