using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class Shell : MonoBehaviour
{
    private const int _distanceUnderground = 140;

    //� ��/�
    [SerializeField] protected float _initialSpeedKmh;
    [SerializeField] protected GameObject _shellExplosionEffect;
    [SerializeField] protected AudioSource _audioSource;
    [SerializeField] protected float _radiusOfScatteringShards;
    [SerializeField] protected float _injuryRadius;
    [SerializeField] protected int _shardsCount;

    protected Rigidbody _rigidbody;
    protected Renderer _renderer;
    protected const float _angleOfScatteringShards = 180;

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

        for (int i = 0; i < _shardsCount; i++)
        {
            RaycastHit raycastHit;
            float xDir = Random.Range(transform.position.x - _angleOfScatteringShards, transform.position.x + _angleOfScatteringShards);
            float yDir = Random.Range(transform.position.y, transform.position.y + _angleOfScatteringShards / 3); // division by 3, so that most of the fragments do not fly up
            float zDir = Random.Range(transform.position.z - _angleOfScatteringShards, transform.position.z + _angleOfScatteringShards);
            Vector3 directionRay = new Vector3(xDir, yDir, zDir);

            //DrawLine(directionRay);


            if (Physics.Raycast(transform.position,directionRay,out raycastHit,_radiusOfScatteringShards))
            {
                if (raycastHit.collider.gameObject.tag == "Enemy")
                {
                    EnemyBehavior enemyBehavior = raycastHit.collider.gameObject.GetComponent<EnemyBehavior>();
                    enemyBehavior.Die();
                    Debug.Log($"{enemyBehavior.gameObject.name} hit!");
                }
            }
        }

        //Collider[] killColliders = Physics.OverlapSphere(transform.position, _killRadius);
        //Collider[] injuryColliders = Physics.OverlapSphere(transform.position, _injuryRadius);

        //foreach (Collider collider in injuryColliders)
        //{
        //    if (collider.gameObject.tag == "Enemy")
        //    {
        //        EnemyBehavior enemyBehavior = collider.gameObject.GetComponent<EnemyBehavior>();
        //        enemyBehavior.Die();
        //    }
        //}

        //foreach (Collider collider in killColliders)
        //{
        //    if (collider.gameObject.tag == "Enemy")
        //    {
        //        EnemyBehavior enemyBehavior = collider.gameObject.GetComponent<EnemyBehavior>();
        //        enemyBehavior.Die();
        //    }
        //}

        CreateExplosionTask(_shellExplosionEffect);
    }

    private async Task DrawLine(Vector3 directionRay)
    {
        float timer = 0;
        while (timer < 5)
        {
            Debug.DrawRay(transform.position, directionRay * _angleOfScatteringShards * 100, Color.green);
            timer += Time.deltaTime;
            await Task.Yield();
        }
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


    protected virtual async Task CreateExplosionTask(GameObject shellExplosionEffect)
    {
        GameObject explosionEffect = Instantiate(shellExplosionEffect, gameObject.transform.position, Quaternion.identity);
        ParticleSystem explosionEffectParticleSystem = explosionEffect.GetComponent<ParticleSystem>();

        AudioSource explosionSound = Instantiate(_audioSource, gameObject.transform.position, Quaternion.identity);
        explosionSound.PlayOneShot(explosionSound.clip);

        EnemiesManager.FindCoverEventHandler?.Invoke();

        while (explosionEffectParticleSystem.isPlaying || explosionSound.isPlaying)
        {
            await Task.Yield();
        }

        Destroy(explosionSound.gameObject);
        Destroy(explosionEffect);
        Destroy(gameObject);
    }
}
