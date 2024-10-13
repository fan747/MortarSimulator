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

    //��������� ���������
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

    //������ ��������� ��������
    protected void SetMIL(int angleAim, float mouseScroll, float mortarAimSpeed, int minAngleMortar, int maxAngleMortar)
    {     
        //���� ������� ( -45 �.�. �������� ����� ������� 0, � �� ����� ���� ������� ��� 45 �������� )
        int angle = angleAim - 45;

        //E��� ���� ������ ���������� �� �������� ����, �� ������ 
        if (_rotationMortar.x != angle)
            _rotationMortar.x = -angle;
    }

    //������ ������ ��������
    protected void HorizonalAim(bool isMove, float mortarRotateSpeed, Transform camerTransform)
    {
        if (isMove)
        {
            //�������������� ���� �������� ( ��� �� �� ���������� ��� ������� ��������� )
            float normalizedRotateAngle = Mathf.Abs(gameObject.transform.eulerAngles.y - camerTransform.eulerAngles.y) / 360;
            print($"�������������� ���� ��������: {normalizedRotateAngle}");
            //������� �� �������� ������ �� �������� ������ �� _mortarRotateSpeed * Time.deltaTime
            Quaternion rotation = Quaternion.Lerp(gameObject.transform.rotation, camerTransform.rotation, mortarRotateSpeed * Time.deltaTime / normalizedRotateAngle);

            //������������ �� Y
            _rotationMortar.y = rotation.eulerAngles.y;
        }
    }
}
