using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private Vector3 _centrePatrolPoint;
    [SerializeField] private float _range;

    private List<Bounds> _safeZoneBounds;
    private NavMeshAgent _agent;
    private EnemyStateMachine _enemyStateMachine;
    private Transform _currentShelterTransform;

    private void Start()
    {
        _enemyStateMachine = new EnemyStateMachine(this);
        _agent = gameObject.GetComponent<NavMeshAgent>();
        _safeZoneBounds = new List<Bounds>();

        FindAllBounds();
    }

    private void Update()
    {
        _enemyStateMachine.UpdateState();
        UpdateCurrentState();
    }

    private void UpdateCurrentState()
    {
        switch (_enemyStateMachine.GetCurrentState())
        {
            case EnemyState.Rest:
                _agent.isStopped = true;
                _enemyStateMachine.SetState(EnemyState.SearchShelter);
                break;
            case EnemyState.SearchShelter:
                _agent.isStopped = false;
                _enemyStateMachine.SetState(EnemyState.InShelter);
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

    public void EnemyPatroling()
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

    public void EnemySearchShelter()
    {
        Vector3 point;

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            if (FindNearesBoundsPoint(out point))
            {
                _agent.SetDestination(point);
            }
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

    private bool FindNearesBoundsPoint(out Vector3 point)
    {
        Bounds nearestBounds = new Bounds();
        float? distanceForNearestBounds = null;
        Vector3 enemyPosition = transform.position;

        foreach (Bounds bounds in _safeZoneBounds)
        {
            if (nearestBounds == new Bounds() && distanceForNearestBounds == null)
            {
                nearestBounds = bounds;
                distanceForNearestBounds = Vector3.Distance(enemyPosition, bounds.ClosestPoint(enemyPosition));               
            }

            float distance = Vector3.Distance(enemyPosition, bounds.ClosestPoint(enemyPosition));

            if (distance < distanceForNearestBounds)
            {
                distanceForNearestBounds = distance;
                nearestBounds = bounds;
            }
        }

        if (nearestBounds == new Bounds() && distanceForNearestBounds == null)
        {
            point = Vector3.zero;
            return false;
        }

        Vector3 randomPoint = new Vector3(Random.Range(nearestBounds.min.x, nearestBounds.max.x), nearestBounds.center.y, Random.Range(nearestBounds.min.z, nearestBounds.max.z));

        point = randomPoint;
        return true;

    }
}
