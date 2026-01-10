
using DG.Tweening;

public class EnemyChaseState : IEnemyState
{
    private PlayerMovement _playerMovement;

    public void Enter(Enemy enemy, PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;
    }

    public void Update(Enemy enemy)
    {

        enemy.transform.DOMove(_playerMovement.transform.position, 2f);


    }

    public void Exit(Enemy enemy)
    {
        
    }
}