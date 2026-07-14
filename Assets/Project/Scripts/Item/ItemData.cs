using UnityEngine;

[CreateAssetMenu(
    fileName = "New Item Data",
    menuName = "Game/Item Data"
)]
public class ItemData : ScriptableObject
{
    public string itemId;
    public string displayName;
    public Sprite icon;
    public ItemCategory category;

    public bool stackable = true;
    public int maxStack =99;
    public float weight;

    public WeaponData weaponData;
    public int healAmount;

}