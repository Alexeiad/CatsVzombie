using DG.Tweening;
using UnityEngine;
using Zenject;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int playerDamage = 10;
    [SerializeField] private int enemyDamage = 5;

    private PlayerMovement _playerMovement;
    private Transform _finishPoint;

    [Inject]
    private void Construct(PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;
    }
    public void MoveToContainer(Transform finishPoint)
    {
        _finishPoint = finishPoint;
    }
    private void Update()
    {
        transform.DOMove(_finishPoint.position, 0.5f).OnComplete(()=>SetDamage());
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