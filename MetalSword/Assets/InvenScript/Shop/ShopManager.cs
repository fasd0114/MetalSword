using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public Shop shop;
    public Inventory playerInventory;
    public InventoryUI inventoryUI;

    public static ShopManager Instance { get; private set; }

    public void TryBuyItem(ItemData item, int quantity)
    {
        int totalPrice = item.price * quantity;
        var playerStats = PlayerStats.Instance;

        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats.Instance가 존재하지 않습니다.");
            return;
        }

        int currentGold = playerStats.CurrentGold;

        if (shop.PurchaseItem(item, playerInventory, currentGold, ref currentGold) && totalPrice <= playerStats.CurrentGold)
        {
            playerStats.AddGold(-totalPrice);  // 골드 차감
            Debug.Log($"{item.itemName} {quantity}개 구매 성공!");
            inventoryUI.RefreshUI();  // UI 갱신
        }
        else
        {
            Debug.Log("구매 실패 (금액 부족 또는 품절)");
        }
    }

    public void ResetShop()
    {
        shop.ResetShopItems();
        playerInventory.ResetInventory();
    }
}
