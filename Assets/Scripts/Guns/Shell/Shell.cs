using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Shell : MonoBehaviour
{
    //� ��/�
    [SerializeField] protected float _initialSpeedKmh;

    protected Vector3 _startPosition;
    protected Vector3 _peakPosition;
    protected Vector3 _endPosition;

    public virtual async Task Shoot(float launchAngle, float azimuth)
    {
        _startPosition = gameObject.transform.position;

        //����������� ���������� ��� �������
        Vector3 hitPosition = Trajectory�alculation.CalculateLandingPoint(_initialSpeedKmh,launchAngle,azimuth, _startPosition);
        Vector3 peakPosition = Trajectory�alculation.CalculatePeakPoint(_startPosition,hitPosition,_initialSpeedKmh, launchAngle);
        float flyTime = Trajectory�alculation.CalculateFlyTime(_initialSpeedKmh, launchAngle, _startPosition);

        _endPosition = hitPosition;
        _peakPosition = peakPosition;

        //��������� ������ �� ����������
        await Fly(_startPosition, peakPosition, hitPosition, flyTime);

        print("������ �����������");

        //��������� ��������� �������
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    protected virtual async Task Fly(Vector3 startPosition, Vector3 peakPosition, Vector3 hitPosition, float flyTime)
    {
        Quaternion startRotation = gameObject.transform.rotation;
        Quaternion endRotation = Quaternion.Euler(180 - startRotation.eulerAngles.x, startRotation.eulerAngles.y, startRotation.eulerAngles.z);

        print($"��������� �������: {startRotation.eulerAngles.x}, �������� �������: {endRotation.eulerAngles.x}");

        float timer = 0f;             

        while (timer < flyTime)
        {
            float timerNormalized = timer / flyTime;

            timer += Time.deltaTime;

            gameObject.transform.rotation = Quaternion.Lerp(startRotation, endRotation, timerNormalized);

            gameObject.transform.position = -Bezier.GetPoint(startPosition, peakPosition - new Vector3(0,140), hitPosition, timerNormalized);

            await Task.Yield();
        }
    }

    private void OnDrawGizmos()
    {
        //����������� ���������� ������
        float segmentsNumber = 20;
        Vector3 previousPoint = _startPosition;
        Gizmos.color = Color.red;

        for (float i = 0; i < segmentsNumber + 1; i++)
        {
            float parametr = i / segmentsNumber;
            Vector3 point = -Bezier.GetPoint(_startPosition, _peakPosition - new Vector3(0, 140), _endPosition, parametr);
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }
}
