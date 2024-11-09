using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class MortarShootingController : MonoBehaviour
{
    private bool _isMortarCanShoot = true;

    public virtual void Shoot(AudioSource mortarAudioSource, GameObject shellPrefab, Vector3 shellSpawnPosition, ParticleSystem shotParticleSystem, float verticalSpread, float horizontalSpread, float verticalPointingAngle, float horizontalPointingAngle,float reloadTimer , bool isCreatingPointingTable)
    {
        if (_isMortarCanShoot)
        {
            ShootingShell(shellPrefab, shellSpawnPosition, verticalSpread, horizontalSpread, verticalPointingAngle, horizontalPointingAngle, isCreatingPointingTable);

            if (!isCreatingPointingTable)
            {
                PlayEffects(mortarAudioSource, shotParticleSystem);             
                Reload(reloadTimer);
            }
        }
    }

    protected virtual void PlayEffects(AudioSource mortarAudioSource, ParticleSystem shotParticleSystem)
    {
        mortarAudioSource.PlayOneShot(mortarAudioSource.clip);
        shotParticleSystem.Play(true);
    }

    private void ShootingShell(GameObject shellPrefab, Vector3 shellSpawnPosition, float verticalSpread, float horizontalSpread, float verticalPointingAngle, float horizontalPointingAngle, bool isCreatingPointingTable)
    {
        GameObject shellPrefabGameObject = Instantiate(
            shellPrefab,
            shellSpawnPosition,
            Quaternion.identity
            );

        Shell shell = shellPrefabGameObject.GetComponent<Shell>(); ;

        float spreadAngleAim = horizontalPointingAngle + Random.Range(-verticalSpread, verticalSpread);
        float spreadAzimuth = verticalPointingAngle + Random.Range(-horizontalSpread, horizontalSpread);

        shell.Shoot(verticalPointingAngle,horizontalPointingAngle, isCreatingPointingTable);

        shell = null;
        shellPrefabGameObject = null;
    }

    protected virtual async void Reload(float reloadTime)
    {
        await StartReloadTimer(reloadTime);
    }

    private async Task StartReloadTimer(float reloadTime)
    {
        _isMortarCanShoot = false;

        float timer = 0;

        while(timer < reloadTime)
        {
            timer += Time.deltaTime;
            await Task.Yield();
        }

        _isMortarCanShoot = true;
    }
}
