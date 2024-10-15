using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Shell : MonoBehaviour
{
    private const int _distanceUnderground = 140;

    //� ��/�
    [SerializeField] protected float _initialSpeedKmh;
    [SerializeField] protected float _freeFallTime;
    [SerializeField] protected GameObject _shellExplosionEffect;

    protected Vector3 _startPosition;
    protected Vector3 _peakPosition;
    protected Vector3 _endPosition;

    public Action<Vector3, Vector3, Vector3,Vector3,float, float, GameObject> DisplayShellEventHundler;

    public virtual void Shoot(float launchAngle, float azimuth)
    {
        _startPosition = gameObject.transform.position;

        //����������� ���������� ��� �������
        Vector3 hitPosition = TrajectoryСalculation.CalculateLandingPoint(_initialSpeedKmh,launchAngle,azimuth, _startPosition);
        Vector3 peakPosition = TrajectoryСalculation.CalculatePeakPoint(_startPosition,hitPosition,_initialSpeedKmh, launchAngle);
        Vector3 freeFallEndPosition = TrajectoryСalculation.CalculateFreeFallPoint(_initialSpeedKmh, launchAngle , _freeFallTime, azimuth, _endPosition);
        float flyTime = TrajectoryСalculation.CalculateFlyTime(_initialSpeedKmh, launchAngle, _startPosition);

        _endPosition = hitPosition;
        _peakPosition = peakPosition;

        DisplayShellEventHundler?.Invoke(_startPosition, _peakPosition, _endPosition, freeFallEndPosition, flyTime, _freeFallTime, _shellExplosionEffect);       
    }   
}
