using Assets.Scripts.Other;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Logger = Assets.Scripts.Other.Logger;
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
    [SerializeField] protected Text _mortarAimText;

    [Header("Params")]
    [SerializeField] protected float _angleAimSpread;
    [SerializeField] protected float _azimuthSpread;
    [SerializeField] protected float _mortarRotateSpeed;
    [SerializeField] protected float _mortarAimSpeed;
    [SerializeField] protected int _maxAngleMortar;
    [SerializeField] protected int _minAngleMortar;
    [SerializeField] protected float _maxDistanceShot;
    [SerializeField] protected float _minDistanceShot;
    [SerializeField] private bool _isCreatingTable;

    protected float _mILAim => AangleToAim(_angleAim);
    protected float _azimuth;
    protected float _angleAim = 0;
    protected float _normalizeAngleAim => (_angleAim - _minAngleMortar ) /(_maxAngleMortar - _minAngleMortar);
    protected Vector3 _rotationMortar;
    protected Dictionary<float, int>  _distanceTable;
    protected Logger _logger;
    protected string _mortarAimTextString => $"Azimuth: {Mathf.RoundToInt(_azimuth)} \n MIL: {Mathf.RoundToInt(_mILAim)} \n Distance Shot: {GetDistanceTable()}";
    protected string _mortarDistanceTablePath;
    protected float _angleAimSpreadTemp;
    protected float _azimuthSpreadTemp;
    protected float AangleToAim(float angle) => Mathf.Round(angle * 17.453f);

    //int angleAim, float mouseScroll, float mortarRotateSpeed, float mortarAimSpeed, int minAngleMortar, int maxAngleMortar, Transform cameraTransform
    public Action<bool, int, float, float, float, int, int, Transform> ChangeViewMortarEventHandler;

    private void Start()
    {
        _inputController.ShootEventHandler += Shot;
        _mortarDistanceTablePath = Path.Combine(Application.dataPath + "/Source/DistanceShotTables", $"{this.ToString()}.txt");

        if (_isCreatingTable)
        {
            _azimuthSpreadTemp = _azimuthSpread;
            _angleAimSpreadTemp = _azimuthSpread;
            _angleAimSpread = 0;
            _azimuthSpread = 0;
            _logger = new Logger(_mortarDistanceTablePath);
            Application.logMessageReceived += _logger.LogMessageDistance;
            Time.timeScale = 50f;
            CreateDistanceTable();
        }
    }

    private void OnDisable()
    {
        if (_isCreatingTable)
            Application.logMessageReceived -= _logger.LogMessageDistance;

        _inputController.ShootEventHandler -= Shot;
    }

    private void Update()
    {
        ChangeView();
        SetAngleAim();
    }

    protected void ChangeView()
    {
        if (_inputController.GetIsMoving() || _inputController.GetMouseScroll() != 0 || _angleAim == 0 || _isCreatingTable)
        {
            _azimuth = gameObject.transform.rotation.eulerAngles.y;

            if (_angleAim == 0)
            {
                _angleAim = _minAngleMortar;
            }

            AimingMortat(_inputController.GetIsMoving(),
                _angleAim,
                _inputController.GetMouseScroll(),
                _mortarRotateSpeed,
                _mortarAimSpeed,
                _minAngleMortar,
                _maxAngleMortar,
                _camera.transform
                );

            _mortarAimText.text = _mortarAimTextString;
            return;
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

    protected virtual string GetDistanceTable()
    {
        if (!_isCreatingTable)
        {
            string distanceTable = "\n";
            string[] table = File.ReadAllLines(_mortarDistanceTablePath);

            string maxDistance = $"{_maxDistanceShot} m: {AangleToAim(_maxAngleMortar)} MIL";
            string minDistance = $"{_minDistanceShot} m: {AangleToAim(_minAngleMortar)} MIL";

            distanceTable += maxDistance + "\n";

            foreach (var line in table)
            {
                string[] lineSplit = line.Split(" m: ");
                if (lineSplit[0] != _maxDistanceShot.ToString() && lineSplit[0] != _minDistanceShot.ToString())
                    distanceTable += $"{line} \n";
            }

            distanceTable += minDistance + "\n";

            return distanceTable;
        }
        return "";
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
        shell.Shoot(spreadAngleAim, spreadAzimuth, _isCreatingTable);

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

    protected async Task CreateDistanceTable()
    {
        while(_angleAim <= _maxAngleMortar + 0.05f)
        {
            ChangeView();
            Shot();
            _angleAim += 0.05f;            
            await Task.Delay(100);
        }
        print($"Таблица создана, путь: {_mortarDistanceTablePath}");
        _angleAimSpread = _angleAimSpreadTemp;
        _azimuthSpread = _azimuthSpreadTemp;
        Time.timeScale = 1f;
    }
}
