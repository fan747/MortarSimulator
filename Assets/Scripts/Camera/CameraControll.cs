using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : MonoBehaviour
{
    [SerializeField] private Transform _playerCameraPointTransform;

    private void Update()
    {
        transform.position = _playerCameraPointTransform.position;
    }
}
