using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using Random = UnityEngine.Random;

public class EnemiesManager : MonoBehaviour
{
    [SerializeField] private int _maxDistanceForCover;
    [SerializeField] private float _timeUnderCover;

    private List<EnemyBehavior> _enemyBehaviors = new List<EnemyBehavior>();
    private List<Bounds> _safeZoneBounds = new List<Bounds>();
    private List<Vector3> _nearestSafeZonePositions;
    public static Action FindCoverEventHandler;
    public static Action BombingOverEventHandler;

    private void Awake()
    {
        FindCoverEventHandler += FindCover;
    }

    private void OnDestroy()
    {
       FindCoverEventHandler -= FindCover;
    }

    private void FindCover()
    {
        GetAllBounds();
        GetAllEnemies();
        _nearestSafeZonePositions = new List<Vector3>();

        foreach (EnemyBehavior enemy in _enemyBehaviors)
        {
            NavMeshAgent enemyAgent = enemy.GetNavMeshAgent();
            Vector3 agentNearestCoverPosition;
            float enemyRadius = enemyAgent.radius + 0.3f;
            bool isCoverSingle;
            Bounds nearestBounds;

            if (GetNearesBoundsPoint(enemyAgent,out agentNearestCoverPosition, out isCoverSingle, out nearestBounds) && Vector3.Distance(enemyAgent.transform.position, nearestBounds.center) < _maxDistanceForCover)
            {
                if (_nearestSafeZonePositions.Count != 0)
                {
                    bool isPositionOccupied = false;
                    List<Vector3> occupiedPosition = new List<Vector3>();

                    isPositionOccupied = IsPositionOccupied(agentNearestCoverPosition, enemyRadius, ref occupiedPosition);

                    if (isPositionOccupied)
                    {
                        isPositionOccupied = FindNoOccupiedPosition(enemyRadius, nearestBounds, occupiedPosition, ref agentNearestCoverPosition);
                        agentNearestCoverPosition = StayPut(enemy, enemyAgent);
                    }
                    else
                    {
                        GoForCover(enemy, agentNearestCoverPosition);
                    }

                }
                else
                {

                    GoForCover(enemy, agentNearestCoverPosition);
                }

                _nearestSafeZonePositions.Add(agentNearestCoverPosition);
            }
            else
            {
                StayPut(enemy, enemyAgent);
            }
        }
        StartTimerAfterExplosionTask();
    }

    private static void GoForCover(EnemyBehavior enemy, Vector3 agentNearestCoverPosition)
    {
        enemy.SetNearestCoverPoint(agentNearestCoverPosition);
        enemy.GoToCover(agentNearestCoverPosition);
    }

    private static Vector3 StayPut(EnemyBehavior enemy, NavMeshAgent enemyAgent)
    {
        Vector3 agentNearestCoverPosition = enemyAgent.transform.position;
        enemy.SetNearestCoverPoint(enemyAgent.transform.position);
        enemy.GoToCover(enemyAgent.transform.position);
        return agentNearestCoverPosition;
    }

    private bool FindNoOccupiedPosition(float enemyRadius, Bounds nearestBounds, List<Vector3> occupiedPosition, ref Vector3 agentNearestCoverPosition)
    {
        float enemyRadiusMultiplie = 1f;

        foreach (Vector3 pos in occupiedPosition)
        {
            Vector3[] directions = new Vector3[8] {
                        new Vector3 (pos.x + enemyRadius * enemyRadiusMultiplie, pos.y, pos.z),
                        new Vector3(pos.x - enemyRadius * enemyRadiusMultiplie, pos.y, pos.z),
                        new Vector3(pos.x, pos.y, pos.z + enemyRadius * enemyRadiusMultiplie),
                        new Vector3(pos.x, pos.y, pos.z - enemyRadius * enemyRadiusMultiplie),

                        new Vector3 (pos.x + enemyRadius * enemyRadiusMultiplie, pos.y,  pos.z + enemyRadius * enemyRadiusMultiplie),
                        new Vector3(pos.x - enemyRadius * enemyRadiusMultiplie, pos.y,  pos.z - enemyRadius * enemyRadiusMultiplie),
                        new Vector3(pos.x + enemyRadius * enemyRadiusMultiplie, pos.y, pos.z - enemyRadius * enemyRadiusMultiplie),
                        new Vector3(pos.x - enemyRadius * enemyRadiusMultiplie, pos.y, pos.z + enemyRadius * enemyRadiusMultiplie),
            };

            foreach (Vector3 dir in directions)
            {

                bool inSafeZone = dir.x >= nearestBounds.min.x + enemyRadius && dir.x <= nearestBounds.max.x - enemyRadius && dir.z >= nearestBounds.min.z + enemyRadius && dir.z <= nearestBounds.max.z - enemyRadius;
                bool isNewPositionOccupied = IsPositionOccupied(dir, enemyRadius);

                if (inSafeZone && !isNewPositionOccupied)
                {
                    agentNearestCoverPosition = dir;
                    return false;
                }
            }
        }

        return true;
    }

    private bool IsPositionOccupied(Vector3 agentNearestCoverPosition, float enemyRadius, ref List<Vector3> occupiedPosition)
    {
        bool isPositionOccupied = false;

        foreach (Vector3 pos in _nearestSafeZonePositions)
        {
            (float minPosX, float maxPosX, float minPosZ, float maxPosZ) = FindOccupiedPositionExtremums(enemyRadius, pos);

            if ((agentNearestCoverPosition.x >= minPosX && agentNearestCoverPosition.x <= maxPosX) && (agentNearestCoverPosition.z >= minPosZ && agentNearestCoverPosition.z <= maxPosZ))
            {
                isPositionOccupied = true;

                occupiedPosition.Add(pos);
            }

        }

        return isPositionOccupied;
    }

    private bool IsPositionOccupied(Vector3 agentNearestCoverPosition, float enemyRadius)
    {
        bool isPositionOccupied = false;
        foreach (Vector3 pos in _nearestSafeZonePositions)
        {
            (float minPosX, float maxPosX, float minPosZ, float maxPosZ) = FindOccupiedPositionExtremums(enemyRadius, pos);

            if ((agentNearestCoverPosition.x >= minPosX && agentNearestCoverPosition.x <= maxPosX) && (agentNearestCoverPosition.z >= minPosZ && agentNearestCoverPosition.z <= maxPosZ))
            {
                isPositionOccupied = true;
            }
        }

        return isPositionOccupied;
    }

    private (float minPosX, float maxPosX, float minPosZ, float maxPosZ) FindOccupiedPositionExtremums(float enemyRadius, Vector3 pos)
    {
        float multiplie = 1f;

        float minPosX = pos.x - enemyRadius * multiplie;
        float maxPosX = pos.x + enemyRadius * multiplie;
        float minPosZ = pos.z - enemyRadius * multiplie;
        float maxPosZ = pos.z + enemyRadius * multiplie;

        return (minPosX, maxPosX, minPosZ, maxPosZ);
    }

    private void GetAllEnemies()
    {
        GameObject[] enemiesGameObjects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject gameObject in enemiesGameObjects)
        {
            EnemyBehavior enemyBehavior = gameObject.GetComponent<EnemyBehavior>();
            _enemyBehaviors.Add(enemyBehavior);
        }
    }

    private void GetAllBounds()
    {
        GameObject[] boundGameObjects = GameObject.FindGameObjectsWithTag("Cover");
        foreach (GameObject gameObject in boundGameObjects)
        {
            Bounds bounds = gameObject.GetComponent<Collider>().bounds;
            _safeZoneBounds.Add(bounds);
        }
    }

    private bool GetNearesBoundsPoint(NavMeshAgent agent,out Vector3 point, out bool isCoverSingle, out Bounds nearestBounds)
    {
        nearestBounds = new Bounds();
        float? distanceForNearestBounds = null;
        Vector3 enemyPosition = agent.transform.position;

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

            float offset = agent.radius + 0.5f;

            float boundsXDistance = nearestBounds.max.x - nearestBounds.min.x;
            float boundsZDistance = nearestBounds.max.z - nearestBounds.min.z;

            point = new Vector3(Random.Range(nearestBounds.min.x + offset, nearestBounds.max.x - offset), nearestBounds.center.y, Random.Range(nearestBounds.min.z + offset, nearestBounds.max.z - offset));
            isCoverSingle = boundsXDistance / offset <= 1 && boundsZDistance / offset <= 1;

            return true;
        }

        point = Vector3.zero;
        isCoverSingle = true;
        return false;
    }

    private async Task StartTimerAfterExplosionTask()
    {
        FindCoverEventHandler -= FindCover;

        float timer = 0f;

        while (timer < _timeUnderCover)
        {
            timer += Time.deltaTime;
            await Task.Yield();
        }

        BombingOverEventHandler?.Invoke();
        FindCoverEventHandler += FindCover;
    }
}
