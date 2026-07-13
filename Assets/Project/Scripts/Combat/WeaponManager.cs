using UnityEngine;
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
        Vector3 castOrigin = transform.position;

        Vector3 direction = transform.forward;  
        //플레이어가 마우스 방향을 바라보고 있으므로 전방으로 발사
        
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
                zombieHealth.TakeDamage(weapon.damage);
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
                zombieHealth.TakeDamage(weapon.damage);
            }
        }
    
        Debug.Log($"Melee Attack - Hit Count: {hitZombies.Length}");
    }
}