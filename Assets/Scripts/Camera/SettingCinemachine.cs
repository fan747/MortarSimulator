using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SettingCinemachine : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    [SerializeField, Range(1,100)] private int _sensitivityCamera;
    [SerializeField, Range(60, 120)] private int _fovCamera;

    private CinemachinePOV _cinemachinePOV;

    // синголтона наебни сюда

}
