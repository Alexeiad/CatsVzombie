// EnemyAttackState.cs

using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    private PlayerMovement _playerMovement;
    private Enemy _enemy;
    public void Enter(Enemy enemy,PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;
        _playerMovement.currentEnemy = enemy;
        _enemy = enemy;
    }

    public void Update(Enemy enemy)
    {
        if (_enemy.Health < 0)
        {
            MonoBehaviour.Destroy(_enemy.gameObject);
        }
        
    }

   
    public void Exit(Enemy enemy)
    {
        
    }
}