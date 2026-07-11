using System;
using UnityEngine;

[CreateAssetMenu(fileName ="NewWeaponData", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Basic Info")]
    public WeaponType weaponType;
    public WeaponRangeType weaponRangeType;
    public String weaponName;

    [Header("Attack Stats")]
    public int damage = 10;
    public float attackRange = 1.8f;
    public float attackRadius = 1.2f; 
    public float attackCooldown = 0.5f;
    private float attackMoveLockDuration = 0.5f;

    [Header("Long Range Settings")]
    public float longRangeDistance = 12f;

    [Header("Visual")] //무기 넣으면 됨..
    public GameObject weaponPrefab;

    [Header("UI")]
    public Sprite icon;
}