using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class InventorySlot
{

    public GameObject Icon;
    public ItemData item;
    public int quantity;
    public int enhancementLevel = 0;  // 강화 레벨
    public bool IsEmpty => item == null;

    public void SetItem(ItemData newItem, int qty = 1)
    {
        item = newItem;
        quantity = qty;
    }

    public void Clear()
    {
        item = null;
        quantity = 0;
    }

    // 아이템을 강화하는 메서드
    public bool EnhanceItem(ref int playerGold)
    {
        if (Icon != null)
        {
            // 아이콘이 UI에서 Image로 표시될 때
            Image iconImage = Icon.GetComponent<Image>();  // Image 컴포넌트 가져오기

            // 아이콘에 이미지가 할당되어 있는지 확인
            if (iconImage != null && iconImage.sprite != null)  // 아이콘에 이미지가 할당되어 있으면 강화
            {
                int enhancementCost = 10;  // 강화 비용 (예시로 10골드 소모)
                if (playerGold >= enhancementCost)
                {
                    PlayerStats.Instance?.AddGold(-enhancementCost);  // 골드 차감
                    enhancementLevel += 1;  // 강화 레벨 증가
                    Debug.Log($"아이템 강화됨! 강화 레벨: {enhancementLevel}");
                    return true;
                }
                else
                {
                    Debug.Log("골드가 부족합니다.");
                    return false;
                }
            }
            else
            {
                Debug.Log("아이콘에 이미지가 할당되어 있지 않습니다.");
                return false;
            }
        }
        else
        {
            Debug.Log("강화할 아이템이 없습니다.");
            return false;
        }
    }
}
