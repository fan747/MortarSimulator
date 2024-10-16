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
    [SerializeField] protected GameObject _shellExplosionEffect;

    protected Rigidbody _rigidbody;
    protected Renderer _renderer;

    public Action<Vector3, Vector3, Vector3,Vector3,float, float, GameObject> DisplayShellEventHundler;

    private void Awake()
    {
        _renderer = gameObject.GetComponent<Renderer>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        _renderer.enabled = false;
        _rigidbody.isKinematic = true;

        CreateExplosion(_shellExplosionEffect);
    }

    private void Update(){
        ShellRotationDuringFlight();
    }

    public virtual void Shoot(float launchAngle, float azimuth)
    {
        float initialSpeedMs = _initialSpeedKmh; // Переводим км/ч в м/с
    
        // Рассчитываем компоненты скорости
        float horizontalSpeed = initialSpeedMs * Mathf.Cos(launchAngle * Mathf.Deg2Rad);
        float verticalSpeed = initialSpeedMs * Mathf.Sin(launchAngle * Mathf.Deg2Rad);

        // Рассчитываем направление в горизонтальной плоскости
        Vector3 horizontalDirection = new Vector3(Mathf.Sin(azimuth * Mathf.Deg2Rad),  0, Mathf.Cos(azimuth* Mathf.Deg2Rad));

        // Создаем вектор начальной скорости
        Vector3 initialVelocity = horizontalDirection * horizontalSpeed + Vector3.up * verticalSpeed;

        // Применяем начальную скорость к Rigidbody
        _rigidbody.velocity = initialVelocity;
    }   

    private void ShellRotationDuringFlight()
    {
        Vector3 shellVelocity = _rigidbody.velocity;

        if (shellVelocity != Vector3.zero)
        {
            transform.eulerAngles = Quaternion.LookRotation(shellVelocity).eulerAngles + new Vector3(90, 0, 0);
        }
    }


    protected virtual async Task CreateExplosion(GameObject shellExplosionEffect)
    {
        GameObject explosionEffect = Instantiate(shellExplosionEffect, gameObject.transform.position, Quaternion.identity);
        ParticleSystem explosionEffectParticleSystem = explosionEffect.GetComponent<ParticleSystem>();

        while (explosionEffectParticleSystem.isPlaying)
        {
            await Task.Yield();
        }

        Destroy(explosionEffect);
        Destroy(gameObject);
    }
}
