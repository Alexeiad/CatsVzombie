// EnemyFleeState.cs
using UnityEngine;

public class EnemyFleeState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        
    }

    public void Update(Enemy enemy)
    {

        enemy.FleeFromPlayer();

       
    }

    public void Exit(Enemy enemy)
    {
        
    }
}