using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MortarView : MonoBehaviour
{
    [SerializeField] private Mortar mortar;

    protected Vector3 _rotationMortar;

    private void Start()
    {
        mortar.ChangeViewMortarEventHandler += AimingMortat;
    }

    private void OnDisable()
    {
        mortar.ChangeViewMortarEventHandler -= AimingMortat;
    }

    //Применяет изменения
    protected virtual void AimingMortat(bool isMove, int angleAim, float mouseScroll, float mortarRotateSpeed, float mortarAimSpeed, int minAngleMortar, int maxAngleMortar, Transform cameraTransform)
    {
        _rotationMortar = gameObject.transform.eulerAngles;

        SetMIL(angleAim, mouseScroll, mortarAimSpeed, minAngleMortar, maxAngleMortar);
        HorizonalAim(isMove,mortarRotateSpeed, cameraTransform);

        if (gameObject.transform.eulerAngles != _rotationMortar)
        {
            gameObject.transform.eulerAngles = _rotationMortar;
        }
    }

    //Меняет дальность стрельбы
    protected void SetMIL(int angleAim, float mouseScroll, float mortarAimSpeed, int minAngleMortar, int maxAngleMortar)
    {     
        //Угол подъема ( -45 т.к. моделька имеет поворот 0, а по факту дуло смотрит под 45 градусов )
        int angle = angleAim - 45;

        //Eсли угол подъем отличается от текущего угла, то меняем 
        if (_rotationMortar.x != angle)
            _rotationMortar.x = -angle;
    }

    //Меняет азимут стрельбы
    protected void HorizonalAim(bool isMove, float mortarRotateSpeed, Transform camerTransform)
    {
        if (isMove)
        {
            //Нормализованый угол поворота ( что бы не ускорялась при длинных поворотах )
            float normalizedRotateAngle = Mathf.Abs(gameObject.transform.eulerAngles.y - camerTransform.eulerAngles.y) / 360;
            print($"Номрализованый угол поворота: {normalizedRotateAngle}");
            //Поворот от поворота игрока до поворота камера за _mortarRotateSpeed * Time.deltaTime
            Quaternion rotation = Quaternion.Lerp(gameObject.transform.rotation, camerTransform.rotation, mortarRotateSpeed * Time.deltaTime / normalizedRotateAngle);

            //Поворачиваем по Y
            _rotationMortar.y = rotation.eulerAngles.y;
        }
    }
}
