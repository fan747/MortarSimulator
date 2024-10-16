using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine 
{
    private EnemyState _currentState;
    private EnemyBehavior _enemyBehavior;

    public EnemyStateMachine (EnemyBehavior enemyBehavior)
    {
        _enemyBehavior = enemyBehavior;
        _currentState = EnemyState.Rest;
    }

    public void UpdateState()
    {
        switch (_currentState)
        {
            case EnemyState.Rest:
                _enemyBehavior.EnemyPatroling();
                break;              
            case EnemyState.SearchShelter:
                _enemyBehavior.EnemySearchShelter();
                break;
            case EnemyState.Injury:
                break;
            case EnemyState.Die:
                break;
        }
    }

    public void SetState(EnemyState state)
    {
        _currentState = state;
        UpdateState();
    }

    public EnemyState GetCurrentState() => _currentState;
}
