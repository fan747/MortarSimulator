using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortarYRotation : MonoBehaviour
{
    private GameObject _mortarGameObject;

    private void Start()
    {
        _mortarGameObject = GameObject.FindWithTag("Player");
    }

    private void Update()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x,_mortarGameObject.transform.eulerAngles.y, transform.eulerAngles.z);
    }
}
