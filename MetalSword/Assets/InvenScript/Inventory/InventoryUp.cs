using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryUp : MonoBehaviour, IPointerClickHandler
{
    public InventorySlot inventorySlot;  // 슬롯 데이터
    public TMP_Text enhancementText;     // 강화 텍스트 (슬롯 우측 상단에 표시)
    public int playerGold;         // 플레이어의 골드 (게임 매니저에서 관리해야 할 경우 수정)

    public void Update()
    {
        playerGold = PlayerStats.Instance.CurrentGold;
        UpdateEnhancementText();
    }
    // 마우스 클릭 시 이벤트 처리
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)  // 오른쪽 클릭으로 강화
        {
            var pc = FindObjectOfType<PlayerController>();
            int oldMax = pc != null ? pc.MaxHealth : 0;

            if (inventorySlot.EnhanceItem(ref playerGold))
            {
                UpdateEnhancementText();
                if (pc != null)
                    pc.RefreshStatsAfterEnhancement(oldMax);
            }
        }
    }

    // 강화 후 UI에 표시되는 강화 레벨 텍스트 갱신
    public void UpdateEnhancementText()
    {
        if (inventorySlot.enhancementLevel > 0)
        {
            enhancementText.text = $"+{inventorySlot.enhancementLevel}";  // 강화 레벨을 텍스트로 표시
        }
        else
        {
            enhancementText.text = "";  // 초기 상태에서는 텍스트를 비워둡니다.
        }
    }
}
