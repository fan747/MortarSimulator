using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class MortarModel : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] protected GameObject _camera;
    [SerializeField] protected InputController _inputController;
    [SerializeField] protected GameObject _shellPrefab;
    [SerializeField] protected Transform _shellSpawnPointTransform;
    [SerializeField] protected AudioSource _audioSource;
    [SerializeField] protected ParticleSystem _shotPaticleSystem;
    [SerializeField] protected ParticleSystem _shotUnderPaticleSystem;

    [Header("Params")]
    [SerializeField] protected float _angleAimSpread;
    [SerializeField] protected float _azimuthSpread;
    [SerializeField] protected float _mortarRotateSpeed;
    [SerializeField] protected float _mortarAimSpeed;
    [SerializeField] protected int _maxAngleMortar;
    [SerializeField] protected int _minAngleMortar;

    protected float _mILAim => _angleAim * 17.453f;
    protected float _azimuth;
    protected float _angleAim;
    protected Vector3 _rotationMortar;

    //int angleAim, float mouseScroll, float mortarRotateSpeed, float mortarAimSpeed, int minAngleMortar, int maxAngleMortar, Transform cameraTransform
    public Action<bool, int, float, float, float, int, int, Transform> ChangeViewMortarEventHandler;

    private void Start()
    {
        _azimuth = gameObject.transform.rotation.eulerAngles.y;
        _angleAim = _minAngleMortar;

        AimingMortat(_inputController.GetIsMoving(),
                _angleAim,
                _inputController.GetMouseScroll(),
                _mortarRotateSpeed,
                _mortarAimSpeed,
                _minAngleMortar,
                _maxAngleMortar,
                _camera.transform
                );

        _inputController.ShootEventHandler += Shot;
    }

    private void OnDisable()
    {
        _inputController.ShootEventHandler -= Shot;
    }

    private void Update()
    {
        ChangeView();
        SetAngleAim();
    }

    protected void ChangeView()
    {
        if (_inputController.GetIsMoving() || _inputController.GetMouseScroll() != 0)
        {
            _azimuth = gameObject.transform.rotation.eulerAngles.y;

            AimingMortat(_inputController.GetIsMoving(),
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
            float _mILChange = mouseScroll * Time.deltaTime * _mortarAimSpeed;

            if (_angleAim >= _minAngleMortar && _angleAim <= _maxAngleMortar)
            {
                if (_angleAim + _mILChange > _minAngleMortar && _angleAim + _mILChange < _maxAngleMortar)
                    _angleAim += mouseScroll * Time.deltaTime * _mortarAimSpeed;
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
        _audioSource.PlayOneShot(_audioSource.clip);

        GameObject shellPrefab = Instantiate(
            _shellPrefab,
            _shellSpawnPointTransform.position,
            Quaternion.identity
            );

        _shotPaticleSystem.Play(true);
        //_shotUnderPaticleSystem.Play();

        Shell shell = shellPrefab.GetComponent<Shell>();

        float spreadAngleAim = _angleAim + Random.Range(-_angleAimSpread, _angleAimSpread);
        float spreadAzimuth = _azimuth + Random.Range(-_azimuthSpread, _azimuthSpread);
        shell.Shoot(spreadAngleAim, spreadAzimuth);

        shell = null;
        shellPrefab = null;
    }

    protected virtual void AimingMortat(bool isMove, float angleAim, float mouseScroll, float mortarRotateSpeed, float mortarAimSpeed, int minAngleMortar, int maxAngleMortar, Transform cameraTransform)
    {
        _rotationMortar = gameObject.transform.eulerAngles;

        SetMIL(angleAim);
        HorizonalAim(isMove, mortarRotateSpeed, cameraTransform);

        if (gameObject.transform.eulerAngles != _rotationMortar)
        {
            gameObject.transform.eulerAngles = _rotationMortar;
        }
    }
    protected void SetMIL(float angleAim)
    {
        float angle = angleAim - 45;

        if (_rotationMortar.x != angle)
            _rotationMortar.x = -angle;
    }

    protected void HorizonalAim(bool isMove, float mortarRotateSpeed, Transform camerTransform)
    {
        if (isMove)
        {
            float normalizedRotateAngle = Mathf.Abs(gameObject.transform.eulerAngles.y - camerTransform.eulerAngles.y) / 360;

            Quaternion rotation = Quaternion.Lerp(gameObject.transform.rotation, camerTransform.rotation, mortarRotateSpeed * Time.deltaTime / normalizedRotateAngle);

            _rotationMortar.y = rotation.eulerAngles.y;
        }
    }
}
