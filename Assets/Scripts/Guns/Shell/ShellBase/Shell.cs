using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class Shell : MonoBehaviour
{ 
    [SerializeField] protected float _initialSpeedKmh;
    [SerializeField] protected GameObject _shellExplosionEffect;
    [SerializeField] protected AudioSource _audioSource;
    [SerializeField] protected float _radiusOfScatteringShards;
    [SerializeField] protected int _shardsCount;
    [SerializeField] protected int _posYWhenDestroy;

    protected Rigidbody _rigidbody;
    protected Renderer _renderer;
    private float _launchAngle;
    private bool _isShowDistanceHit;

    public Action<Vector3, Vector3, Vector3,Vector3,float, float, GameObject> DisplayShellEventHundler;
    public float InitialSpeed => _initialSpeedKmh;

    private void Awake()
    {
        _renderer = gameObject.GetComponent<Renderer>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        _renderer.enabled = false;
        _rigidbody.isKinematic = true;
        Vector3 startPosition = transform.position;

        for (int i = 0; i < _shardsCount; i++)
        {
            RaycastHit raycastHit;
            float xDir = Random.Range(-_radiusOfScatteringShards, _radiusOfScatteringShards);
            float yDir = Random.Range(-_radiusOfScatteringShards / 4, _radiusOfScatteringShards);
            float zDir = Random.Range(- _radiusOfScatteringShards, _radiusOfScatteringShards);
            Vector3 directionRay = new Vector3(xDir, yDir, zDir);

            //DrawLine(directionRay, startPosition);

            if (Physics.Raycast(startPosition, directionRay,out raycastHit,_radiusOfScatteringShards))
            {
                GameObject hitGameObject = raycastHit.collider.gameObject;
                if (hitGameObject.tag == "Enemy")
                {
                    EnemyBehavior enemyBehavior = hitGameObject.GetComponent<EnemyBehavior>();

                    float distanceHit = Vector3.Distance(hitGameObject.transform.position, transform.position);

                    if (distanceHit > _radiusOfScatteringShards / 2)
                    {
                        enemyBehavior.Injury(transform.position);
                    }
                    else
                    {
                        enemyBehavior.Die();
                    }                 
                }
            }
        }

        if(_isShowDistanceHit)
            print($"Real distance: {Vector3.Distance(transform.position, GameObject.FindWithTag("Player").transform.position)}  MILS: {_launchAngle * 17.453f}");

        CreateExplosionTask(_shellExplosionEffect);
    }

    private async Task DrawLine(Vector3 directionRay, Vector3 startPosition)
    {
        float timer = 0;
        while (timer < 5)
        {
            Debug.DrawRay(startPosition, directionRay * _radiusOfScatteringShards * 100, Color.green);
            timer += Time.deltaTime;
            await Task.Yield();
        }
    }

    private void Update(){
        ShellRotationDuringFlight();

        if(transform.position.y < _posYWhenDestroy)
        {
            Destroy(gameObject);
        }
    }

    public virtual void Shoot(float launchAngle, float azimuth, bool isShowDistanceHit)
    {
        _launchAngle = launchAngle;
        _isShowDistanceHit = isShowDistanceHit;

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


    protected virtual async Task CreateExplosionTask(GameObject shellExplosionEffect)
    {
        GameObject explosionEffect = Instantiate(shellExplosionEffect, gameObject.transform.position, Quaternion.identity);
        ParticleSystem explosionEffectParticleSystem = explosionEffect.GetComponent<ParticleSystem>();
        EnemiesManager.FindCoverEventHandler?.Invoke();

        if (!_isShowDistanceHit)
        {
            AudioSource explosionSound = Instantiate(_audioSource, gameObject.transform.position, Quaternion.identity);
            explosionSound.PlayOneShot(explosionSound.clip);

            while (explosionEffectParticleSystem.isPlaying || explosionSound.isPlaying)
            {
                await Task.Yield();
            }

            Destroy(explosionSound.gameObject);
        }

        Destroy(explosionEffect);
        Destroy(gameObject);
    }
}
