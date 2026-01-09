// EnemyStateMachine.cs
using UnityEngine;

public class EnemyStateMachine
{
    private IEnemyState _currentState;
    private Enemy _enemy;

    public IEnemyState CurrentState => _currentState;

    public EnemyStateMachine(Enemy enemy, IEnemyState initialState)
    {
        _enemy = enemy;
        ChangeState(initialState);
    }

    public void ChangeState(IEnemyState newState)
    {
        _currentState?.Exit(_enemy);
        _currentState = newState;
        _currentState?.Enter(_enemy);
    }

    public void Update()
    {
        _currentState?.Update(_enemy);
    }
}