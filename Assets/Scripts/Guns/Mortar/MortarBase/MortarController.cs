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

public abstract class MortarController : MonoBehaviour
{
    private const float _creatingTableTimeScale = 10f;

    [Header("Objects")]
    [SerializeField] protected GameObject _cameraGameObject;
    [SerializeField] protected InputController _inputController;
    [SerializeField] protected GameObject _shellPrefab;
    [SerializeField] protected Transform _shellSpawnPointTransform;
    [SerializeField] protected AudioSource _mortarAudioSource;
    [SerializeField] protected ParticleSystem _shotParticleSystem;
    [SerializeField] protected Text _mortarAimText;
    [SerializeField] protected MortarPointingController _mortarPointingController;
    [SerializeField] protected MortarShootingController _mortarShootingController;

    [Header("Params")]
    [SerializeField] protected float _mortarReloadTimer;
    [SerializeField] protected float _verticalSpread;
    [SerializeField] protected float _horizontalSpread;
    [SerializeField] protected float _mortarHorizontalPointingSpeed;
    [SerializeField] protected float _mortarVerticalPointingSpeed;
    [SerializeField] protected int _maxMortarVerticalPointingAngle;
    [SerializeField] protected int _minMortarVerticalPointingAngle;
    [SerializeField] protected float _maxDistanceShot;
    [SerializeField] protected float _minDistanceShot;
    [SerializeField] private bool _isCreatingTable;

    protected float MILAim => AangleToAim(_pointingVerticalAngle);
    protected float _azimuth;
    protected float _pointingVerticalAngle;
    protected Vector3 _rotationMortar;
    protected Dictionary<float, int>  _distanceTable;
    protected Logger _logger;
    protected string MortarAimTextString => $"Azimuth: {Mathf.RoundToInt(_azimuth)} \n MIL: {Mathf.RoundToInt(MILAim)} \n Distance Shot: {GetDistanceTable()}";
    protected string _mortarDistanceTablePath;
    protected float _angleAimSpreadTemp;
    protected float _azimuthSpreadTemp;
    protected float AangleToAim(float angle) => Mathf.Round(angle * 17.453f);

    private async void Start()
    {
        _azimuth = gameObject.transform.localRotation.eulerAngles.y;
        _pointingVerticalAngle = _minMortarVerticalPointingAngle;

        _inputController.ShootEventHandler += Shot;
        _mortarDistanceTablePath = Path.Combine(Application.dataPath + "/Source/DistanceShotTables", $"{this.ToString()}.txt");

        if (_isCreatingTable)
        {
            _azimuthSpreadTemp = _horizontalSpread;
            _angleAimSpreadTemp = _horizontalSpread;
            _verticalSpread = 0;
            _horizontalSpread = 0;
            _logger = new Logger(_mortarDistanceTablePath);
            Application.logMessageReceived += _logger.LogMessageDistance;
            Time.timeScale = _creatingTableTimeScale;
            await CreateDistanceTable();
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
        _azimuth = gameObject.transform.rotation.eulerAngles.y;
        _mortarAimText.text = MortarAimTextString;
        PointingMortar();
    }

    protected virtual void PointingMortar()
    {
        float mouseScroll = _inputController.GetMouseScroll();

        _azimuth = gameObject.transform.rotation.eulerAngles.y;

        _pointingVerticalAngle = _mortarPointingController.GetPointingAngle(mouseScroll,_pointingVerticalAngle,_mortarVerticalPointingSpeed,_minMortarVerticalPointingAngle,_maxMortarVerticalPointingAngle);
        _mortarPointingController.SetPointingMortar(_pointingVerticalAngle, _mortarHorizontalPointingSpeed, _cameraGameObject.transform, mouseScroll != 0, _inputController.GetIsMoving());          
    }

    private string GetDistanceTable()
    {
        if (!_isCreatingTable)
        {
            try
            {
                string distanceTable = "\n";
                string[] table = File.ReadAllLines(_mortarDistanceTablePath);

                string maxDistance = $"{_maxDistanceShot} m: {AangleToAim(_minMortarVerticalPointingAngle)} MIL";
                string minDistance = $"{_minDistanceShot} m: {AangleToAim(_maxMortarVerticalPointingAngle)} MIL";

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
            catch
            {
                throw new NullReferenceException($"Create distance table for the {this.name} (turn on isCreatingTable)");
            }
        }
        return "";
    }

    protected virtual void Shot()
    {
        _mortarShootingController.Shoot(
            _mortarAudioSource,
            _shellPrefab,
            _shellSpawnPointTransform.position,
            _shotParticleSystem,
            _verticalSpread,
            _horizontalSpread,
            _pointingVerticalAngle,
            _azimuth,
            _mortarReloadTimer,
            _isCreatingTable
            );
    }

    private async Task CreateDistanceTable()
    {

        while(_pointingVerticalAngle <= _maxMortarVerticalPointingAngle + 0.05f)
        {
            //ChangeView();
            PointingMortar();
            Shot();
            _pointingVerticalAngle += 0.05f;            
            await Task.Yield();
        }
        print($"Таблица создана, путь: {_mortarDistanceTablePath}");
        _verticalSpread = _angleAimSpreadTemp;
        _horizontalSpread = _azimuthSpreadTemp;
        Time.timeScale = 1f;
    }
}
