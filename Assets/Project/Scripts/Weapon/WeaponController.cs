using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Holder")]
    [SerializeField] private Transform weaponHolder;

    [Header("Current Weapon")]
    [SerializeField] private WeaponData currentWeapon;

    private GameObject currentWeaponObject; 
    private PlayerAttack playerAttack;
    public WeaponData CurrentWeapon => currentWeapon;

    private void Awake()
    {
        playerAttack = GetComponent<PlayerAttack>();
    }
    public void EquipWeapon(WeaponData weaponData)
    {
        if(weaponData == null)
        {
            Debug.LogWarning("장착할 무기가 없습니다.");
            return;
        }
        playerAttack?.CancelPendingAttack();
        currentWeapon = weaponData;

        ClearCurrentWeaponObject();
        CreateWeaponObject(weaponData);

        Debug.Log($"무기 장착:{weaponData.weaponName}");
    }

    public void ClearCurrentWeaponObject()
    {
        if (currentWeaponObject != null)
        {
            Destroy(currentWeaponObject);
            currentWeaponObject = null;
        }
    }
    public void CreateWeaponObject(WeaponData weaponData)
    {
        if(weaponHolder == null)
        {
            return;
        }

        if(weaponData.weaponPrefab == null)
        {
            return;
        }

        currentWeaponObject = Instantiate(
            weaponData.weaponPrefab,
            weaponHolder.position,
            weaponHolder.rotation,
            weaponHolder
        );
        Transform weaponTransform = currentWeaponObject.transform;
        weaponTransform.localPosition = weaponData.equipLocalPosition;
        weaponTransform.localRotation = Quaternion.Euler(weaponData.equipLocalRotation);
        weaponTransform.localScale = weaponData.equipLocalScale;
    }

    public Transform CurrentMuzzle
    {
        get
        {
            if(currentWeaponObject == null)
            {
                return null;
            }
            WeaponVisual weaponVisual = currentWeaponObject.GetComponent<WeaponVisual>();
            return weaponVisual != null ? weaponVisual.Muzzle : null;
        }
    }

    public void UnequipWeapon()
    {
        currentWeapon = null;

        if (currentWeaponObject != null)
            Destroy(currentWeaponObject);
    }
}