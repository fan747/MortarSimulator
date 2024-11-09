using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class DroneCameraControll : MonoBehaviour
{
    [SerializeField] private float _cameraOffsetMultiplie;
    [SerializeField] private float _cameraHeight;
    private GameObject _enemyBasePoint;

    private void Start()
    {
        _enemyBasePoint = GameObject.FindWithTag("EnemyBase");
        Bounds enemyBaseBounds = _enemyBasePoint.GetComponent<Collider>().bounds;

        Vector3[] directions = new Vector3[8] {
            new Vector3 (enemyBaseBounds.min.x -_cameraOffsetMultiplie, enemyBaseBounds.max.y + _cameraHeight, enemyBaseBounds.center.z),
            new Vector3(enemyBaseBounds.max.x + _cameraOffsetMultiplie,  enemyBaseBounds.max.y + _cameraHeight, enemyBaseBounds.center.z),
            new Vector3(enemyBaseBounds.center.x, enemyBaseBounds.max.y + _cameraHeight, enemyBaseBounds.min.z - _cameraOffsetMultiplie),
            new Vector3(enemyBaseBounds.center.x, enemyBaseBounds.max.y + _cameraHeight, enemyBaseBounds.max.z + _cameraOffsetMultiplie),

            new Vector3 (enemyBaseBounds.max.x, enemyBaseBounds.max.y + _cameraHeight,  enemyBaseBounds.max.z + _cameraOffsetMultiplie),
            new Vector3(enemyBaseBounds.min.x -_cameraOffsetMultiplie, enemyBaseBounds.max.y + _cameraHeight,  enemyBaseBounds.min.z - _cameraOffsetMultiplie),
            new Vector3(enemyBaseBounds.max.x + _cameraOffsetMultiplie, enemyBaseBounds.max.y + _cameraHeight, enemyBaseBounds.min.z - _cameraOffsetMultiplie),
            new Vector3(enemyBaseBounds.min.x - _cameraOffsetMultiplie, enemyBaseBounds.max.y + _cameraHeight, enemyBaseBounds.max.z + _cameraOffsetMultiplie),
        };

        transform.position = directions[Random.Range(0,directions.Length)];
        transform.LookAt(enemyBaseBounds.center);
    }
}
