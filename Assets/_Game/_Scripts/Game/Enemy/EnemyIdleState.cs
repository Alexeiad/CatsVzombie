// EnemyIdleState.cs
using UnityEngine;

public class EnemyIdleState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        
    }

    public void Update(Enemy enemy)
    {
        
        if (enemy.PlayerInDetectionRadius())
        {
            enemy.StateMachine.ChangeState(new EnemyChaseState());
        }
        
    }

    public void Exit(Enemy enemy)
    {
        
    }
}