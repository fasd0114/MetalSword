using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotParent;
    public Inventory inventoryData;
    public TMP_Text goldText;

    public TMP_Text swordDamageText;
    // ① 플레이어 체력 표시용 텍스트 필드 추가
    public TMP_Text healthText;

    private List<GameObject> currentSlots = new();

    private void Start()
    {
        if (inventoryData == null)
            inventoryData = Resources.Load<Inventory>("Items/Inventory");
        GenerateSlots();
        RefreshUI();
    }

    private void Update()
    {
        if (goldText != null && PlayerStats.Instance != null)
        {
            goldText.text = $"{PlayerStats.Instance.CurrentGold}";
        }

        var player = FindObjectOfType<PlayerController>();
        if (player == null) return;

        if (swordDamageText != null)
            swordDamageText.text = $"{player.AttackDamage}";

        // ② healthText가 연결되어 있으면 현재/최대 체력 표시
        if (healthText != null)
            healthText.text = $"{player.CurrentHealth} / {player.MaxHealth}";
    }

    private void GenerateSlots()
    {
        foreach (Transform child in slotParent)
            Destroy(child.gameObject);
        currentSlots.Clear();
        for (int i = 0; i < inventoryData.maxSlots; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotParent);
            currentSlots.Add(slotGO);
            var up = slotGO.GetComponent<InventoryUp>();
            if (up == null) continue;
            up.inventorySlot = inventoryData.items[i];
            var iconTrans = slotGO.transform.Find("Icon");
            if (iconTrans != null)
                up.inventorySlot.Icon = iconTrans.gameObject;
            var enhTrans = slotGO.transform.Find("EnhancementText");
            if (enhTrans != null)
                up.enhancementText = enhTrans.GetComponent<TMP_Text>();
        }
    }

    public void RefreshUI()
    {
        for (int i = 0; i < currentSlots.Count; i++)
        {
            var ui = currentSlots[i];
            var data = inventoryData.items[i];
            var iconObj = ui.transform.Find("Icon");
            var qtyObj = ui.transform.Find("Quantity");
            if (iconObj == null || qtyObj == null)
            {
                Debug.LogWarning($"[{i}] 아이콘 또는 수량 텍스트가 없습니다.");
                continue;
            }
            var iconImage = iconObj.GetComponent<Image>();
            var qtyText = qtyObj.GetComponent<TMP_Text>();
            if (iconImage == null || qtyText == null)
            {
                Debug.LogWarning($"[{i}] Image 또는 TMP_Text 컴포넌트를 찾을 수 없습니다.");
                continue;
            }
            if (data.IsEmpty)
            {
                iconImage.enabled = false;
                qtyText.text = "";
            }
            else
            {
                iconImage.enabled = true;
                iconImage.sprite = data.item.icon;
                qtyText.text = data.quantity.ToString();
            }
        }
    }



    // 게임 시작 시 아이템의 아이콘과 상태를 초기화하는 메서드
    /*private void InitializeItemIconsAndState()
    {
        for (int i = 0; i < inventoryData.items.Count; i++)
        {
            var ui = currentSlots[i];
            var iconObj = ui.transform.Find("Icon");
            var qtyObj = ui.transform.Find("Quantity");

            if (iconObj != null)
            {
                var iconImage = iconObj.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = null; // 아이콘 초기화 (아이콘을 숨기기)
                    iconImage.enabled = false; // 아이콘 숨기기
                }
            }

            if (qtyObj != null)
            {
                var qtyText = qtyObj.GetComponent<TMP_Text>();
                if (qtyText != null)
                {
                    qtyText.text = ""; // 수량 텍스트 초기화
                }
            }

            // 아이템 상태 초기화 (수량을 0으로 설정)
            inventoryData.items[i].quantity = 0; // 수량 초기화
        }
    }*/
}