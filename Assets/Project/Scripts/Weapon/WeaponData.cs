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