using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Shell : MonoBehaviour
{
    //В км/ч
    private float _initialSpeedKmh = 72;

    private Vector3 _startPoint;
    private Vector3 _peakPoint;
    private Vector3 _endPoint;



    public async Task Shoot(float launchAngle, float azimuth)
    {
        Vector3 startPosition = gameObject.transform.position;
        Vector3 loacalStartPosition = gameObject.transform.localPosition;

        //Высчитываем все основные позиция для снаряда
        Vector3 hitPosition = TrajectoryСalculation.CalculateLandingPoint(_initialSpeedKmh,launchAngle,azimuth, loacalStartPosition);
        Vector3 peakPosition = TrajectoryСalculation.CalculatePeakPoint(startPosition,hitPosition,_initialSpeedKmh, launchAngle);
        float flyTime = TrajectoryСalculation.CalculateFlyTime(_initialSpeedKmh, launchAngle, startPosition);

        _startPoint = startPosition;
        _endPoint = hitPosition;
        _peakPoint = peakPosition;

        //Запускаем снаряд
        await Fly(startPosition, peakPosition, hitPosition, flyTime);

        print("Снаряд приземлился");
    }

    private async Task Fly(Vector3 startPosition, Vector3 peakPosition, Vector3 hitPosition, float flyTime)
    {
        Quaternion startRotation = gameObject.transform.rotation;
        Quaternion endRotation = Quaternion.Euler(startRotation.eulerAngles + new Vector3 (90,0));

        float timer = 0f;             

        while (timer < flyTime)
        {
            float timerNormalized = timer / flyTime;

            timer += Time.deltaTime;

            print($"Таймер: {timer}, Нормализованный таймер: {timerNormalized}");

            gameObject.transform.rotation = Quaternion.Lerp(startRotation, endRotation, timerNormalized);

            gameObject.transform.position = -Bezier.GetPoint(startPosition, peakPosition - new Vector3(0,140), hitPosition, timerNormalized);

            await Task.Yield();
        }
    }

    private void OnDrawGizmos()
    {
        float segmentsNumber = 20;
        Vector3 previousPoint = _startPoint;
        Gizmos.color = Color.red;

        for (float i = 0; i < segmentsNumber + 1; i++)
        {
            float parametr = i / segmentsNumber;
            Vector3 point = -Bezier.GetPoint(_startPoint, _peakPoint - new Vector3(0, 140), _endPoint, parametr);
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

    }
}
