using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Holder")]
    [SerializeField] private Transform weaponHolder;

    [Header("Current Weapon")]
    [SerializeField] private WeaponData currentWeapon;

    private GameObject currentWeaponObject; 

    public WeaponData CurrentWeapon => currentWeapon;
    public void EquipWeapon(WeaponData weaponData)
    {
        if(weaponData == null)
        {
            Debug.LogWarning("장착할 무기가 없습니다.");
            return;
        }
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

        currentWeaponObject.transform.localPosition = Vector3.zero;
        currentWeaponObject.transform.localRotation = Quaternion.identity;
    }
}