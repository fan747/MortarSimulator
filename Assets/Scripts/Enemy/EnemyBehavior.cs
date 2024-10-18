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

    private float _originalCenterY;
    private List<Bounds> _safeZoneBounds;
    private NavMeshAgent _agent;
    private EnemyStateMachine _enemyStateMachine;
    private Vector3 _nearestCoverRandomPoint;

    public Action FindCoverEventHandler;
    public Action InCoverEventHandler;

    private void Start()
    {
        _enemyStateMachine = new EnemyStateMachine(this);
        _agent = gameObject.GetComponent<NavMeshAgent>();
        _safeZoneBounds = new List<Bounds>();

        FindCoverEventHandler += GoToCover;
        InCoverEventHandler += LayDown;

        FindAllBounds();
    }

    private void OnDestroy()
    {
        FindCoverEventHandler -= GoToCover;
        InCoverEventHandler -= LayDown;
    }

    private void Update()
    {
        UpdateCurrentState();
    }

    private void UpdateCurrentState()
    {
        switch (_enemyStateMachine.GetCurrentState())
        {
            case EnemyState.Rest:
                _enemyStateMachine.SetState(EnemyState.SearchShelter);
                break;
            case EnemyState.SearchShelter:
                _agent.isStopped = false;

                if (_agent.transform.position == new Vector3(_nearestCoverRandomPoint.x, _agent.transform.position.y, _nearestCoverRandomPoint.z) && !_agent.hasPath)
                {
                    _agent.isStopped = true;
                    _agent.updateUpAxis = false;
                    _agent.updatePosition = false;
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

    private void GoToCover()
    {
       if (FindNearesBoundsRandomPoint(out _nearestCoverRandomPoint))
       {
            _agent.SetDestination(_nearestCoverRandomPoint);            
        } 
    }

    private void FindAllBounds()
    {
        GameObject[] boundGameObjects = GameObject.FindGameObjectsWithTag("Cover");
        foreach (GameObject gameObject in boundGameObjects)
        {
            Bounds bounds = gameObject.GetComponent<Collider>().bounds;
            _safeZoneBounds.Add(bounds);
        }
    }

    private bool FindNearesBoundsRandomPoint(out Vector3 point)
    {
        Bounds nearestBounds = new Bounds();
        float? distanceForNearestBounds = null;
        Vector3 enemyPosition = transform.position;

        if (_safeZoneBounds.Count != 0)
        {
            foreach (Bounds safeZoneBounds in _safeZoneBounds)
            {
                if (nearestBounds == new Bounds() && distanceForNearestBounds == null)
                {
                    nearestBounds = safeZoneBounds;
                    distanceForNearestBounds = Vector3.Distance(enemyPosition, safeZoneBounds.ClosestPoint(enemyPosition));
                }

                float distance = Vector3.Distance(enemyPosition, safeZoneBounds.ClosestPoint(enemyPosition));

                if (distance < distanceForNearestBounds)
                {
                    distanceForNearestBounds = distance;
                    nearestBounds = safeZoneBounds;
                }
            }

            Vector3 randomPoint = new Vector3(Random.Range(nearestBounds.min.x + _agent.height/2, nearestBounds.max.x - _agent.height / 2), nearestBounds.center.y, Random.Range(nearestBounds.min.z + _agent.height / 2, nearestBounds.max.z - _agent.height / 2));

            point = randomPoint;
            return true;
        }
      
        point = Vector3.zero; 
        return false;
    }

    private void LayDown()
    {
        LayDownTask();
    }

    private async Task LayDownTask()
    {

        if (_agent != null)
        {
            Vector3 startPosition = _childrenCapsule.transform.position;
            Vector3 layPosition = startPosition - new Vector3(0, 1, 0);

            Vector3 startRotation = _childrenCapsule.transform.eulerAngles;
            Vector3 layRotation = startRotation + new Vector3(90, 0, 0);

            float timer = 0;

            while (timer < _layTime)
            {
                timer += Time.deltaTime;
                float normalizedTimer = timer / _layTime;

                _childrenCapsule.transform.position = Vector3.Lerp(startPosition, layPosition, normalizedTimer);
                _childrenCapsule.transform.eulerAngles = Vector3.Lerp(startRotation, layRotation, normalizedTimer);

                await Task.Yield();
            }
        }
    }
    private void StandUp()
    {
        StandUpTask();
    }

    private async Task StandUpTask()
    {
        
    }
}
