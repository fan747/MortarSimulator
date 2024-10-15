using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ShellView : MonoBehaviour
{
    [SerializeField] private Shell shell;

    private const int _bezierCorrection = 140;
    private Vector3 _startPosition;
    private Vector3 _peakPosition;
    private Vector3 _endPosition;
    private Vector3 _freeFallEndPosition;

    private void Awake()
    {
        shell.DisplayShellEventHundler += StartDisplayShell;
    }

    private void OnDestroy()
    {
        shell.DisplayShellEventHundler -= StartDisplayShell;
    }

    protected virtual void StartDisplayShell(Vector3 startPosition, Vector3 peakPosition, Vector3 endPosition, Vector3 freeFallEndPosition, float flyTime, float freeFallTime, GameObject shellExplosionEffect)
    {
        DisplayShell(startPosition, peakPosition, endPosition, freeFallEndPosition, flyTime,freeFallTime, shellExplosionEffect);
    }

    protected virtual async Task DisplayShell(Vector3 startPosition, Vector3 peakPosition, Vector3 endPosition, Vector3 freeFallEndPosition, float flyTime, float freeFallTime, GameObject shellExplosionEffect)
    {
        _startPosition = startPosition;
        _peakPosition = peakPosition;
        _endPosition = endPosition;
        _freeFallEndPosition = freeFallEndPosition;

        await Fly(startPosition, peakPosition, endPosition, freeFallEndPosition, flyTime, freeFallTime);

        gameObject.GetComponent<MeshRenderer>().enabled = false;

        await CreateExplosion(shellExplosionEffect);

        Destroy(gameObject);
    }

    protected virtual async Task Fly(Vector3 startPosition, Vector3 peakPosition, Vector3 endPosition, Vector3 freeFallEndPosition, float flyTime, float freeFallTime)
    {
        Quaternion startRotation = gameObject.transform.rotation;
        Quaternion endRotation = Quaternion.Euler(180 - startRotation.eulerAngles.x, startRotation.eulerAngles.y, startRotation.eulerAngles.z);

        print($"��������� �������: {startRotation.eulerAngles.x}, �������� �������: {endRotation.eulerAngles.x}");

        float timer = 0f;

        while (timer < flyTime)
        {
            float timerNormalized = timer / flyTime;

            timer += Time.deltaTime;

            gameObject.transform.rotation = Quaternion.Lerp(startRotation, endRotation, timerNormalized);

            gameObject.transform.position = -Bezier.GetPoint(startPosition, peakPosition - new Vector3(0, _bezierCorrection), endPosition, timerNormalized);

            print($"�������: {gameObject.transform.position}, ������: {timer}");

            await Task.Yield();
        }


        timer = 0f;

        while (timer < freeFallTime)
        {
            float timerNormalized = timer / freeFallTime;

            timer += Time.deltaTime;

            gameObject.transform.position = Vector3.Lerp(-_endPosition, _freeFallEndPosition, timerNormalized);
            print($"�������: {gameObject.transform.position}, ������: {timer}");

            await Task.Yield();
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
    }

    private void OnDrawGizmos()
    {
        //����������� ���������� ������
        float segmentsNumber = 20;
        Vector3 previousPoint = _startPosition;
        Gizmos.color = Color.red;

        for (float i = 0; i < segmentsNumber + 1; i++)
        {
            float parametr = i / segmentsNumber;
            Vector3 point = -Bezier.GetPoint(_startPosition, _peakPosition - new Vector3(0, _bezierCorrection), _endPosition, parametr);
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        for (float i = 0; i < segmentsNumber + 1; i++)
        {
            float parametr = i / segmentsNumber;
            Vector3 point = Vector3.Lerp(-_endPosition, _freeFallEndPosition, parametr);
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }
}
