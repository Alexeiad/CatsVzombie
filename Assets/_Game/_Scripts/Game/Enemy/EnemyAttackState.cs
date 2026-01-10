// EnemyAttackState.cs
using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    private PlayerMovement _playerMovement;

    public void Enter(Enemy enemy,PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;
    }

    public void Update(Enemy enemy)
    {
        
        
    }

    private void PerformAttack(Enemy enemy)
    {
        
    }

    public void Exit(Enemy enemy)
    {
        
    }
}