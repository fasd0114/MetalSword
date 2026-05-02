// ItemData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int price;
    public string description;
    public ItemType itemType;
}

public enum ItemType
{
    Consumable,
    Equipment,
    Material
}
