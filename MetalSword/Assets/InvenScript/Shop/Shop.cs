using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShop", menuName = "Shop/Shop")]
public class Shop : ScriptableObject
{
    public List<ShopItem> shopItems;

    public bool PurchaseItem(ItemData item, Inventory playerInventory, int playerGold, ref int updatedGold)
    {
        ShopItem shopItem = shopItems.Find(s => s.item == item);
        if (shopItem != null && playerGold >= item.price && shopItem.stock > 0)
        {
            playerInventory.AddItem(item, 1);
            shopItem.stock--;
            updatedGold = playerGold - item.price;
            return true;
        }

        return false;
    }

    // 추가된 초기화 함수
    public void ResetShopItems()
    {
        foreach (var shopItem in shopItems)
        {
            shopItem.stock = 10;  // 예시로, 아이템을 초기화할 수 있는 수량 설정
        }
    }
}
