// EnemyStateMachine.cs
using UnityEngine;

public class EnemyStateMachine
{
    private IEnemyState _currentState;
    private Enemy _enemy;

    public IEnemyState CurrentState => _currentState;

    public EnemyStateMachine(Enemy enemy, IEnemyState initialState,PlayerMovement playerMovement)
    {
        _enemy = enemy;
        ChangeState(initialState, playerMovement);
    }

    public void ChangeState(IEnemyState newState,PlayerMovement playerMovement)
    {
        _currentState?.Exit(_enemy);
        _currentState = newState;
        _currentState?.Enter(_enemy,playerMovement);
    }

    public void Update()
    {
        _currentState?.Update(_enemy);
    }
}