using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private Vector3 _centrePatrolPoint;
    [SerializeField] private float _range;
    [SerializeField] private float _layCenterYOffset;
    [SerializeField] private float _layTime;
    [SerializeField] private GameObject _childrenCapsule;

    private NavMeshAgent _agent;
    private EnemyStateMachine _enemyStateMachine;
    private Vector3 _nearestCoverPoint;
    private bool _isLayDown = false;
    private Vector3 _startPosition;
    public Action InCoverEventHandler;
    private Vector3 _startRotation;

    private void Start()
    {
        _enemyStateMachine = new EnemyStateMachine(this);
        _agent = gameObject.GetComponent<NavMeshAgent>();

        EnemiesManager.FindCoverEventHandler += _enemyStateMachine.SetStateSearchShelter;
        EnemiesManager.BombingOverEventHandler += _enemyStateMachine.SetStateSearchRest;
        EnemiesManager.BombingOverEventHandler += StandUp;
        InCoverEventHandler += LayDown;
    }

    private void OnDestroy()
    {
        EnemiesManager.FindCoverEventHandler -= _enemyStateMachine.SetStateSearchShelter;
        EnemiesManager.BombingOverEventHandler -= StandUp;
        EnemiesManager.BombingOverEventHandler -= _enemyStateMachine.SetStateSearchRest;
        InCoverEventHandler -= LayDown;
    }        


    private void Update()
    {
        UpdateCurrentState();
        _enemyStateMachine.UpdateState();
    }

    private void UpdateCurrentState()
    {
        switch (_enemyStateMachine.GetCurrentState())
        {
            case EnemyState.Rest:
                break;
            case EnemyState.SearchShelter:
                _agent.isStopped = false;

                if (_agent.transform.position == new Vector3(_nearestCoverPoint.x, _agent.transform.position.y, _nearestCoverPoint.z) && !_agent.hasPath)
                {
                    _enemyStateMachine.SetState(EnemyState.InShelter);
                }
                break;
            case EnemyState.InShelter:
                
                break;
        }      
    }

    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit navMeshHit;

        if (NavMesh.SamplePosition(randomPoint, out navMeshHit, 1f, NavMesh.AllAreas))
        {
            result = navMeshHit.position;
            return true;
        }
        
        result = Vector3.zero;
        return false;
    }

    public void Patroling()
    {
        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            Vector3 point;
            if (RandomPoint(_centrePatrolPoint, _range, out point))
            {
                _agent.SetDestination(point);
            }
        }
    }

    public void GoToCover(Vector3 coverPoint)
    {  
       _agent.SetDestination(coverPoint);                 
    }

    public void LayDown()
    {
        if (!_isLayDown)
        {
            LayDownTask();
        }
    }

    private async Task LayDownTask()
    {
        if (_agent != null)
        {
            _isLayDown = true;
            _agent.isStopped = true;
            _agent.updateUpAxis = false;
            _agent.updatePosition = false;

            _startPosition = _childrenCapsule.transform.position;
            Vector3 layPosition = _startPosition - new Vector3(0, 1, 0);


            _startRotation = _childrenCapsule.transform.eulerAngles;
            Vector3 rotatelDirection = new Vector3(Mathf.Sin(_startRotation.y * Mathf.Deg2Rad), 0, Mathf.Cos(_startRotation.y * Mathf.Deg2Rad));
            Vector3 layRotation = rotatelDirection * 90;


            float timer = 0;

            while (timer < _layTime)
            {
                timer += Time.deltaTime;
                float normalizedTimer = timer / _layTime;

                _childrenCapsule.transform.position = Vector3.Lerp(_startPosition, layPosition, normalizedTimer);
                _childrenCapsule.transform.eulerAngles = Vector3.Lerp(_startRotation, layRotation, normalizedTimer);

                await Task.Yield();
            }
        }
    }

    public void StandUp()
    {
        if (_isLayDown)
        {
            StandUpTask();
        }
    }

    private async Task StandUpTask()
    {
        if (_agent != null)
        {
            _isLayDown = false;

            Vector3 startPosition = _childrenCapsule.transform.position;
            Vector3 startRotation = _childrenCapsule.transform.eulerAngles;

            float timer = 0;

            while (timer < _layTime)
            {
                timer += Time.deltaTime;
                float normalizedTimer = timer / _layTime;

                _childrenCapsule.transform.position = Vector3.Lerp(startPosition, _startPosition, normalizedTimer);
                _childrenCapsule.transform.eulerAngles = Vector3.Lerp(startRotation, _startRotation, normalizedTimer);

                await Task.Yield();
            }

            _agent.isStopped = false;
            _agent.updateUpAxis = true;
            _agent.updatePosition = true;
        }
    }

    public NavMeshAgent GetNavMeshAgent() => _agent;

    public Vector3 SetNearestCoverPoint(Vector3 nearestCoverPoint) => _nearestCoverPoint = nearestCoverPoint; 
}
