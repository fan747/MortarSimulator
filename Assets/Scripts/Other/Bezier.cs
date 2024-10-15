using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier
{
    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        //�������� ����� �� �������� ������� ( ������ ����� �������� � ��������� )
        Vector3 p01 = Vector3.Lerp(p0,p1,t);
        Vector3 p12 = Vector3.Lerp(p1, p2, t);

        //�������� �� �������, ��������� ����� �������.
        Vector3 point = Vector3.Lerp(p01, p12, t);

        return point;
    }
}
