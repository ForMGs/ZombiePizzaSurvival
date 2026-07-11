using System;
using UnityEngine;

[Serializable]
public class QuickSlotData
{
    public QuickSlotContentType contentType;

    [Header("Weapon")]
    public WeaponData weaponData;

    [Header("Item")]
    public ItemType itemType;

    [Header("UI")]
    public Sprite customIcon;

    public Sprite Icon
    {
        get
        {
            if(customIcon != null)
                return customIcon;
            if(contentType == QuickSlotContentType.Weapon && weaponData != null)
                return weaponData.icon;

            return null;
        }
    }

    public string DisplayName
    {
        get
        {
            if(contentType == QuickSlotContentType.Weapon && weaponData != null)
                return weaponData.weaponName;

            if (contentType == QuickSlotContentType.Item)
                return itemType.ToString();
            return "Empty";
        }
    }
}