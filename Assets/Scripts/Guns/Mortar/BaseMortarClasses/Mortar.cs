using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Mortar : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] protected GameObject _camera;
    [SerializeField] protected InputController _inputController;
    [SerializeField] protected GameObject _shellPrefab;
    [SerializeField] protected Transform _shellSpawnPointTransform;

    [Header("Params")]
    [SerializeField] protected float _mortarRotateSpeed;
    [SerializeField] protected float _mortarAimSpeed;
    [SerializeField] protected int _maxAngleMortar;
    [SerializeField] protected int _minAngleMortar;

    protected int _mILAim => Convert.ToInt32(_angleAim * 17.453f);
    protected float _azimuth;
    protected int _angleAim;

    //int angleAim, float mouseScroll, float mortarRotateSpeed, float mortarAimSpeed, int minAngleMortar, int maxAngleMortar, Transform cameraTransform
    public Action<bool ,int, float, float, float, int , int, Transform> ChangeViewMortarEventHandler;

    private void Start()
    {
        _angleAim = _minAngleMortar;
        _inputController.ShootEventHandler += Shot;
    }

    private void OnDisable()
    {
        _inputController.ShootEventHandler -= Shot;
    }

    private void Update()
    {
        _azimuth = gameObject.transform.rotation.eulerAngles.y;
        ChangeView();
        SetAngleAim();
    }

    //ѕередаем изменени€ во View
    protected void ChangeView()
    {
        if (_inputController.GetIsMoving() || _inputController.GetMouseScroll() != 0)
        {
            ChangeViewMortarEventHandler?.Invoke(_inputController.GetIsMoving(),
                _angleAim,
                _inputController.GetMouseScroll(),
                _mortarRotateSpeed,
                _mortarAimSpeed,
                _minAngleMortar,
                _maxAngleMortar,
                _camera.transform
                );
        }
    }

    protected void SetAngleAim()
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

    protected virtual void Reload()
    {

    }

    protected virtual void Shot()
    {
        GameObject shellPrefab = Instantiate(
            _shellPrefab,
            -_shellSpawnPointTransform.position,
            Quaternion.Euler(gameObject.transform.eulerAngles.x + 45, gameObject.transform.eulerAngles.y, gameObject.transform.eulerAngles.z)
            );

        Shell shell = shellPrefab.GetComponent<Shell>();

        shell.Shoot(_angleAim, _azimuth);

        shell = null;
        shellPrefab = null;
    }
}
