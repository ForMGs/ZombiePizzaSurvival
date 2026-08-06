using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    private WeaponController weaponController;
    private void Awake()
    {
        weaponController = GetComponent<WeaponController>();
    }
    
    //총공격
    public void FirePistol(WeaponData weapon ,LayerMask zombieLayer)
    {
        Transform muzzle = weaponController.CurrentMuzzle;

        Vector3 origin = muzzle != null ? muzzle.position : transform.position + Vector3.up ;
        Vector3 castOrigin = origin;

        Vector3 direction = transform.forward;  
        //플레이어가 마우스 방향을 바라보고 있으므로 전방으로 발사
        if (weapon.bulletPrefab != null)
        {
            GameObject bulletObject = Instantiate(
                weapon.bulletPrefab,
                origin,
                Quaternion.LookRotation(direction)
            );

            Bullet bullet = bulletObject.GetComponent<Bullet>();

            if (bullet != null)
            {
                bullet.Initialize(direction, weapon.bulletSpeed);
            }
        }
        if (Physics.SphereCast(
            castOrigin,
            0.2f,
            direction,
            out RaycastHit hitInfo,
            weapon.longRangeDistance,
            zombieLayer))
        {
            Debug.Log($"Ray Hit: {hitInfo.collider.name}");

            ZombieHealth zombieHealth =
                hitInfo.collider.GetComponentInParent<ZombieHealth>();

            if (zombieHealth != null)
            {
                zombieHealth.TakeDamage(
                    weapon.damage,
                    hitInfo.point,
                    direction,
                    weapon);
            }
        }
        else
        {
            Debug.LogWarning("Pistol Ray Miss");
        }
        Debug.DrawRay(
            castOrigin,
            direction * weapon.longRangeDistance,
            Color.red,
            2f
        );
    }

    //근접 공격
    public void MeleeAttack(WeaponData weapon ,LayerMask zombieLayer)
    {
        Collider[] hitZombies = Physics.OverlapSphere(
            transform.position,
            weapon.attackRange,
            zombieLayer
        );

        HashSet<ZombieHealth> damagedZombies = new HashSet<ZombieHealth>();

        foreach(Collider hitZombie in hitZombies)
        {
            Vector3 directionToZombie = hitZombie.transform.position - transform.position;
            directionToZombie.y = 0f;

            if(directionToZombie.sqrMagnitude <=0.001f)
                continue;
            
            float angleZombie = Vector3.Angle(
                transform.forward,
                directionToZombie
            );

            if (angleZombie > weapon.attackAngle * 0.5f)
                continue;
            
            ZombieHealth zombieHealth =
                hitZombie.GetComponentInParent<ZombieHealth>();
            
            if(zombieHealth != null && damagedZombies.Add(zombieHealth))
            {
                Vector3 hitPoint = hitZombie.ClosestPoint(
                    transform.position + Vector3.up);
                zombieHealth.TakeDamage(
                    weapon.damage,
                    hitPoint,
                    directionToZombie.normalized,
                    weapon);
            }
        }
    
        Debug.Log($"Melee Attack - Hit Count: {hitZombies.Length}");
    }
}

public static class CombatHitFeedback
{
    public static void PlaySwing(Vector3 position, WeaponData weapon)
    {
        if (weapon == null)
            return;

        PlaySound(position, weapon.swingSounds, weapon.swingSoundVolume, 0f);
    }

    public static void Play(Vector3 hitPoint, Vector3 hitDirection, WeaponData weapon)
    {
        if (weapon == null)
            return;

        PlayEffect(hitPoint, hitDirection, weapon.hitEffectPrefab);
        PlaySound(hitPoint, weapon.hitSounds, weapon.hitSoundVolume, 0.65f);
        HitStopController.Request(weapon.hitStopDuration);
    }

    private static void PlayEffect(Vector3 position, Vector3 direction, GameObject prefab)
    {
        Quaternion rotation = direction.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(-direction.normalized)
            : Quaternion.identity;

        if (prefab != null)
        {
            Object.Instantiate(prefab, position, rotation);
            return;
        }

        GameObject effectObject = new("Default Hit Effect");
        effectObject.transform.SetPositionAndRotation(position, rotation);
        ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = particles.main;
        main.duration = 0.12f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.75f, 0.2f),
            new Color(0.8f, 0.05f, 0.02f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 8, 12) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 28f;
        shape.radius = 0.03f;
        particles.Play();
    }

    private static void PlaySound(
        Vector3 position,
        AudioClip[] clips,
        float volume,
        float spatialBlend)
    {
        if (clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null)
            return;

        GameObject soundObject = new($"Hit Sound - {clip.name}");
        soundObject.transform.position = position;
        AudioSource source = soundObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.pitch = Random.Range(0.95f, 1.05f);
        source.spatialBlend = Mathf.Clamp01(spatialBlend);
        source.Play();
        Object.Destroy(soundObject, clip.length / Mathf.Abs(source.pitch) + 0.1f);
    }
}

public sealed class HitStopController : MonoBehaviour
{
    private const float HitStopTimeScale = 0.0001f;
    private static HitStopController instance;
    private Coroutine routine;
    private float endTime;
    private float previousTimeScale = 1f;

    public static void Request(float duration)
    {
        if (duration <= 0f || Time.timeScale <= 0f)
            return;

        if (instance == null)
        {
            GameObject controllerObject = new("Hit Stop Controller");
            instance = controllerObject.AddComponent<HitStopController>();
            DontDestroyOnLoad(controllerObject);
        }

        instance.endTime = Mathf.Max(instance.endTime, Time.unscaledTime + duration);
        if (instance.routine == null)
            instance.routine = instance.StartCoroutine(instance.HitStopRoutine());
    }

    private IEnumerator HitStopRoutine()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = HitStopTimeScale;

        while (Time.unscaledTime < endTime)
            yield return null;

        if (Mathf.Approximately(Time.timeScale, HitStopTimeScale))
            Time.timeScale = previousTimeScale;
        routine = null;
    }

    private void OnDestroy()
    {
        if (routine != null && Mathf.Approximately(Time.timeScale, HitStopTimeScale))
            Time.timeScale = previousTimeScale;
        if (instance == this)
            instance = null;
    }
}
