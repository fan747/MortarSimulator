using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Mortar : MonoBehaviour
{
    [SerializeField] private GameObject _camera;
    [SerializeField] private InputController _inputController;
    [SerializeField] private GameObject _shellPrefab;
    [SerializeField] private Transform _shellSpawnPointTransform;
    [SerializeField] private float _mortarRotateSpeed;
    [SerializeField] private float _mortarAimSpeed;
    [SerializeField] private int _maxAngleMortar;
    [SerializeField] private int _minAngleMortar;

    private float _mILAim => _angleAim * 17.453f;
    private float _azimuth;

    private float _angleAim;

    private void Start()
    {
        _angleAim = _minAngleMortar;
        _inputController.ShootEventHandler += Shoot;
    }

    private void Update()
    {
        _azimuth = gameObject.transform.rotation.eulerAngles.y;
        RotateMortat(_angleAim);
        SetMIL();
    }

    private void RotateMortat(float mIL)
    {
        bool isMove = _inputController.GetIsMoving();

        Vector3 rotationMortar = gameObject.transform.eulerAngles;

        if (isMove)
        {
            //ѕоворот от поворота игрока до поворота камера за _mortarRotateSpeed * Time.deltaTime
            Quaternion rotation = Quaternion.Lerp(gameObject.transform.rotation, _camera.transform.rotation, _mortarRotateSpeed * Time.deltaTime);

            //«адаем поворот только по оси Y
            rotationMortar.y = rotation.eulerAngles.y;
        }

        //ѕеревод из MIL в углы
        float angle = _angleAim - 45;

        if (gameObject.transform.eulerAngles.x != angle)
            rotationMortar.x = -angle;

        if (gameObject.transform.eulerAngles != rotationMortar)
        {
            gameObject.transform.eulerAngles = rotationMortar;
        }
    }

    private void SetMIL()
    {
        float mouseScroll = _inputController.GetMouseScroll();

        if (mouseScroll != 0)
        {
            //¬еличина на которую нужно изменить MIL
            int _mILChange = Convert.ToInt32(mouseScroll * Time.deltaTime * _mortarAimSpeed);

            //≈сли MIL в целевом диапазоне, то провер€ем текущую иттерацию на то, что бы при прибавлении MIL, они не ушли за целевой диапазон, -
            // - иначе устанавливаем значение MIL на крайние значени€
            if (_angleAim >= _minAngleMortar && _angleAim <= _maxAngleMortar)
            {
                if (_angleAim + _mILChange > _minAngleMortar && _angleAim + _mILChange < _maxAngleMortar)
                    _angleAim += Convert.ToInt32(mouseScroll * Time.deltaTime * _mortarAimSpeed);
                else if (_angleAim - _minAngleMortar < _maxAngleMortar - _angleAim)
                {
                    _angleAim = _minAngleMortar;
                }
                else
                    _angleAim = _maxAngleMortar;
            }
        }
    }

    private void Shoot()
    {
        GameObject shellPrefab = Instantiate(
            _shellPrefab, 
            -_shellSpawnPointTransform.position, 
            Quaternion.Euler(gameObject.transform.eulerAngles.x + 45, gameObject.transform.eulerAngles.y, gameObject.transform.eulerAngles.z)
            );

        Shell shell = shellPrefab.GetComponent<Shell>();

        shell.Shoot(_angleAim,_azimuth);

        shell = null;
        shellPrefab = null;
    }   
}
