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
                _enemyBehavior.Patroling();
                break;              
            case EnemyState.SearchShelter:
                break;
            case EnemyState.InShelter:
                _enemyBehavior.InCoverEventHandler?.Invoke();
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

    public void SetStateSearchShelter()
    {
        SetState(EnemyState.SearchShelter);
    }
    public void SetStateSearchRest()
    {
        SetState(EnemyState.Rest);
    }
}
