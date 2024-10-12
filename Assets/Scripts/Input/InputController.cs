using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private bool _isMoving;
    private float _mouseScroll;

    public Action ShootEventHandler;

    private void Update()
    {
        InputProcessing();
    }

    private void InputProcessing()
    {
        _mouseScroll = Input.GetAxis("Mouse ScrollWheel");

        _isMoving = Input.GetMouseButton(1);

        if (Input.GetMouseButtonDown(0))
        {
            ShootEventHandler?.Invoke();
        }
    }

    public bool GetIsMoving() => _isMoving;

    public float GetMouseScroll() => _mouseScroll;
}
