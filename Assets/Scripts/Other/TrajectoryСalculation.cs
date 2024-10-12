using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class TrajectoryСalculation
{
    public static Vector3 CalculatePeakPoint(Vector3 startPosition, Vector3 hitPosition, float initialSpeed, float angle)
    {
        // Ускорение свободного падения
        float g = Physics.gravity.y;

        // Перевод угла из градусов в радианы
        float radianAngle = angle * Mathf.Deg2Rad;

        // Вычисление вертикальной и горизонтальной компонент начальной скорости  // Горизонтальная составляющая
        float initialVelocityY = initialSpeed * Mathf.Sin(radianAngle);  // Вертикальная составляющая

        // Время, за которое снаряд достигнет максимальной высоты
        float timeToPeak = initialVelocityY / g;

        // Координата по оси Y (высота) в момент максимальной высоты
        float yAtPeak = (initialVelocityY * timeToPeak) - (0.5f * g * Mathf.Pow(timeToPeak, 2));
        float xAtPeak = (startPosition.x + hitPosition.x)/2;
        float zAtPeak = (startPosition.z + hitPosition.z) / 2;

        // Возвращаем Vector3 точки (X, Y, Z)
        return new Vector3(xAtPeak, yAtPeak, zAtPeak); 
    }

    public static Vector3 CalculateLandingPoint(float initialSpeed, float launchAngle,float azimuth, Vector3 startPosition)
    {
        // Константы
        float gravity = Physics.gravity.y; // Берем гравитацию из настроек Unity (по умолчанию -9.81 м/с^2)

        // Преобразуем угол в радианы
        float launchAngleRad = Mathf.Deg2Rad * launchAngle;

        // Компоненты начальной скорости
        float v0z = initialSpeed * Mathf.Cos(launchAngleRad); // Горизонтальная скорость
        float v0y = initialSpeed * Mathf.Sin(launchAngleRad); // Вертикальная скорость

        // Время подъема (время, когда вертикальная скорость становится равна 0)
        float timeToMaxHeight = v0y / -gravity;

        // Общее время полета: время подъема + время падения (симметрично для движения по параболе)
        float totalTime = 2 * timeToMaxHeight;

        // Горизонтальная дистанция, пройденная за это время
        float distance = v0z * totalTime;

        float azimuthRad = Mathf.Deg2Rad * azimuth;

        Vector3 direction = new Vector3(Mathf.Sin(azimuthRad), 0, Mathf.Cos(azimuthRad));

        Vector3 landingPoint = startPosition - direction * distance;

        return landingPoint;
    }

    public static float CalculateFlyTime(float initialSpeed, float launchAngle, Vector3 startPosition)
    {
        // Константы
        float gravity = Physics.gravity.y; // Берем гравитацию из настроек Unity (по умолчанию -9.81 м/с^2)

        // Преобразуем угол в радианы
        float launchAngleRad = Mathf.Deg2Rad * launchAngle;

        // Компоненты начальной скорости
        float v0x = initialSpeed * Mathf.Cos(launchAngleRad); // Горизонтальная скорость
        float v0y = initialSpeed * Mathf.Sin(launchAngleRad); // Вертикальная скорость

        // Время подъема (время, когда вертикальная скорость становится равна 0)
        float timeToMaxHeight = v0y / -gravity;

        // Общее время полета: время подъема + время падения (симметрично для движения по параболе)
        float totalTime = 2 * timeToMaxHeight;

        // Горизонтальная дистанция, пройденная за это время      

        return totalTime;
    }
}
