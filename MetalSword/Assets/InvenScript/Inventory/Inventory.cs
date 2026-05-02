using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewInventory", menuName = "Inventory/Inventory")]
public class Inventory : ScriptableObject
{
    public int maxSlots = 20;
    public List<InventorySlot> items = new();

    // 인벤토리 초기화
    public void ResetInventory()
    {
        items.Clear();  // 아이템 목록 초기화
        for (int i = 0; i < maxSlots; i++)
        {
            items.Add(new InventorySlot());  // 빈 슬롯 생성
        }
    }


    private void OnEnable()
    {
        // OnEnable에서 인벤토리가 비어있을 경우 초기화
        if (items.Count < maxSlots)
        {
            ResetInventory();  // 인벤토리 초기화 함수 호출
        }
    }

    // 아이템 추가 메서드
    public bool AddItem(ItemData newItem, int amount = 1)
    {
        // 이미 있는 아이템이면 수량 추가
        var existing = items.Find(s => s.item == newItem);
        if (existing != null)
        {
            existing.quantity += amount;
            return true;
        }

        // 비어 있는 슬롯 찾아 추가
        var empty = items.Find(s => s.IsEmpty);
        if (empty != null)
        {
            empty.SetItem(newItem, amount);
            return true;
        }

        Debug.LogWarning("인벤토리에 빈 슬롯이 없습니다.");
        return false;
    }
}
