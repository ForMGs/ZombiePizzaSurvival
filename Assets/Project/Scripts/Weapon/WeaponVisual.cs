using UnityEngine;

public class WeaponVisual : MonoBehaviour
{
    [SerializeField] private Transform muzzle;

    public Transform Muzzle => muzzle;
}