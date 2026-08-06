using System;
using UnityEngine;

[CreateAssetMenu(fileName ="NewWeaponData", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Basic Info")]
    public WeaponType weaponType;
    public WeaponRangeType weaponRangeType;
    public String weaponName;
    [Header("Projectile")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 35f;
    [Header("Attack Stats")]
    public int damage = 10;
    public float attackRange = 1.8f;
    [Range(0f, 360f)] 
    public float attackAngle = 90f;
    public float attackCooldown = 0.5f;

    public float attackMoveLockDuration = 0.5f;

    [Header("Hit Feedback")]
    [Tooltip("비워 두면 간단한 기본 파티클이 생성됩니다.")]
    public GameObject hitEffectPrefab;
    [Tooltip("명중할 때 이 목록 중 하나를 무작위로 재생합니다.")]
    public AudioClip[] hitSounds;
    [Range(0f, 1f)] public float hitSoundVolume = 0.8f;
    [Tooltip("근접 무기를 휘두를 때 이 목록 중 하나를 무작위로 재생합니다.")]
    public AudioClip[] swingSounds;
    [Range(0f, 1f)] public float swingSoundVolume = 0.7f;
    [Min(0f)] public float hitStopDuration = 0.05f;
    [Min(0f)] public float knockbackDistance = 0.35f;
    [Min(0.01f)] public float knockbackDuration = 0.12f;
   
    [Header("Long Range Settings")]
    public float longRangeDistance = 12f;

    [Header("Visual")] //무기 넣으면 됨..
    public GameObject weaponPrefab;

    [Header("UI")]
    public Sprite icon;

    [Header("Equip Transform")]
    public Vector3 equipLocalPosition;
    public Vector3 equipLocalRotation;
    public Vector3 equipLocalScale = Vector3.one;
}
