
public interface IEnemyState
{
    void Enter(Enemy enemy, PlayerMovement playerMovement);
    void Update(Enemy enemy);
    void Exit(Enemy enemy);
}
