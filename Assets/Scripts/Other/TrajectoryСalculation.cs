using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class TrajectoryСalculation
{
    private static readonly float _gravity = Physics.gravity.y;

    private static float AngleInRadians(float angle) => angle * Mathf.Deg2Rad;

    private static (float horizontalV0, float verticalV0) CalculateInitialVelocity(float initialSpeed, float radian)
    {
        float radianAngle = AngleInRadians(radian);
        return (initialSpeed * Mathf.Cos(radianAngle), initialSpeed * Mathf.Sin(radianAngle));
    }

    private static float CalculateTimeToPeak(float verticalV0) => verticalV0 / _gravity;

    private static float CalculateTimeOfFly(float verticalV0) => CalculateTimeToPeak(verticalV0) * 2;

    private static float CalculateDistance(float v0, float time) => v0 * time;

    private static Vector3 CalculateDirection(float azimuth)
    {
        float azimuthRadian = AngleInRadians(azimuth);
        return new Vector3(Mathf.Sin(azimuthRadian), 0, Mathf.Cos(azimuthRadian));
    }

    public static Vector3 CalculatePeakPoint(Vector3 startPosition, Vector3 hitPosition, float initialSpeed, float angle)
    {
        float verticalV0 = CalculateInitialVelocity(initialSpeed, angle).verticalV0;
        float timeToPeak = CalculateTimeToPeak(verticalV0);

        // Координата по оси Y (высота) в момент максимальной высоты
        float yAtPeak = (verticalV0 * timeToPeak) - (0.5f * _gravity
         * Mathf.Pow(timeToPeak, 2));
        float xAtPeak = (startPosition.x + hitPosition.x) / 2;
        float zAtPeak = (startPosition.z + hitPosition.z) / 2;

        // Возвращаем Vector3 точки (X, Y, Z)
        return new Vector3(xAtPeak, yAtPeak, zAtPeak);
    }

    public static Vector3 CalculateLandingPoint(float initialSpeed, float launchAngle, float azimuth, Vector3 startPosition)
    {
        (float horizontalV0, float verticalV0) = CalculateInitialVelocity(initialSpeed, launchAngle); // Вертикальная скорость

        // Общее время полета: время подъема + время падения (симметрично для движения по параболе)
        float flyTime = -CalculateTimeOfFly(verticalV0);

        // Горизонтальная дистанция, пройденная за это время
        float distance = CalculateDistance(horizontalV0, flyTime);

        Vector3 direction = CalculateDirection(azimuth);

        return startPosition - direction * distance;
    }

    public static float CalculateFlyTime(float initialSpeed, float launchAngle, Vector3 startPosition)
    {
        float v0 = CalculateInitialVelocity(initialSpeed, launchAngle).verticalV0; // Вертикальная скорость

        return -CalculateTimeOfFly(v0);
    }

    public static Vector3 CalculateFreeFallPoint(float initialSpeed, float launchAngle, float freeFallTime, float azimuth, Vector3 startPosition)
    {
        // Рассчитываем начальные скорости
        (float horizontalV0, float verticalV0) = CalculateInitialVelocity(initialSpeed, launchAngle);

        // Рассчитываем время полета до достижения начальной высоты
        float timeToInitialHeight = -CalculateTimeOfFly(verticalV0);

        // Рассчитываем горизонтальное расстояние, пройденное за время полета до начальной высоты
        float distanceToInitialHeight = horizontalV0 * timeToInitialHeight;

        // Рассчитываем дополнительное горизонтальное расстояние во время свободного падения
        float additionalHorizontalDistance = horizontalV0 * freeFallTime;

        // Рассчитываем вертикальное расстояние во время свободного падения
        float freeFallVerticalDistance = 0.5f * -_gravity * Mathf.Pow(freeFallTime, 2);

        // Суммарное горизонтальное расстояние
        float totalHorizontalDistance = distanceToInitialHeight + additionalHorizontalDistance;

        // Направление движения
        Vector3 direction = CalculateDirection(azimuth);

        // Вектор смещения
        Vector3 displacementVector = new Vector3(
            direction.x * totalHorizontalDistance,
            -freeFallVerticalDistance,  // Отрицательное значение, так как снаряд движется вниз
            direction.z * totalHorizontalDistance
        );

        // Конечная точка
        return startPosition + displacementVector;
    }

}

