// EnemyIdleState.cs
using Unity.VisualScripting;
using UnityEngine;

public class EnemyWalkingState : IEnemyState
{
    private PlayerMovement _playerMovement;

    public void Enter(Enemy enemy, PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;
    }


    public void Update(Enemy enemy)
    {

        if (enemy.PlayerInDetectionRadius())
        {
            enemy.StateMachine.ChangeState(new EnemyChaseState(), _playerMovement);
        }
        
    }

    public void Exit(Enemy enemy)
    {
        
    }
}