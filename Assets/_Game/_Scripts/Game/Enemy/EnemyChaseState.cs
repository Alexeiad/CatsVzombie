

public class EnemyChaseState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        
    }

    public void Update(Enemy enemy)
    {
      
        enemy.MoveTowardsPlayer();

        
    }

    public void Exit(Enemy enemy)
    {
        
    }
}