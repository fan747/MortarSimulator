using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MortarPointingController : MonoBehaviour
{
    private const int modelRotationOffset = 45;
    private const float _minSpeedLimiterMultiplie = 0.1f;

    public void SetPointingMortar(float pointingAngle, float mortarHorizontalPointingSpeed, Transform cameraTransform, bool isVerticalPointing, bool isHorizontalPointing)
    {
        Vector3 mortarRotation = gameObject.transform.eulerAngles;

        if (isVerticalPointing)
        {
            GetVerticalPointingAngle(pointingAngle, ref mortarRotation);
        }

        if (isHorizontalPointing)
        {
            GetHorizontalPointingAngle(mortarHorizontalPointingSpeed, cameraTransform, ref mortarRotation);
        }

        ApplyPointing(mortarRotation);
    }

    public float GetPointingAngle(float mouseScroll,float pointingAngle, float mortarVerticalPointingSpeed, float minMortarVerticalPointingAngle, float maxMortarVerticalPointingAngle)
    {
        if (mouseScroll != 0)
        {
            float angleChange = mouseScroll * Time.deltaTime * mortarVerticalPointingSpeed;

            if (pointingAngle >= minMortarVerticalPointingAngle && pointingAngle <= maxMortarVerticalPointingAngle)
            {
                if (pointingAngle + angleChange > minMortarVerticalPointingAngle && pointingAngle + angleChange < maxMortarVerticalPointingAngle)
                {
                    pointingAngle += mouseScroll * Time.deltaTime * mortarVerticalPointingSpeed;
                }
                else if (pointingAngle - minMortarVerticalPointingAngle < maxMortarVerticalPointingAngle - pointingAngle)
                {
                    pointingAngle = minMortarVerticalPointingAngle;
                }
                else
                {
                    pointingAngle = maxMortarVerticalPointingAngle;
                }
            }
        }

        return pointingAngle;
    }

    private void ApplyPointing(Vector3 mortarRotation)
    {
        if (gameObject.transform.eulerAngles != mortarRotation)
        {
            gameObject.transform.eulerAngles = mortarRotation;
        }
    }

    private void GetVerticalPointingAngle(float pointingAngle, ref Vector3 mortarRotation)
    {
        float angle = pointingAngle - modelRotationOffset;
        
        mortarRotation.x = -angle;  
    }

    private void GetHorizontalPointingAngle( float mortarRotateSpeed, Transform cameraTransform, ref Vector3 mortarRotation)
    {
        float speedLimiterMultiplie = Mathf.Clamp(Quaternion.Angle(gameObject.transform.rotation, cameraTransform.rotation) / 180, _minSpeedLimiterMultiplie, 1);

        Quaternion rotation = Quaternion.Lerp(gameObject.transform.rotation, cameraTransform.rotation, mortarRotateSpeed * Time.deltaTime / speedLimiterMultiplie);

        mortarRotation.y = rotation.eulerAngles.y;
    }
}
