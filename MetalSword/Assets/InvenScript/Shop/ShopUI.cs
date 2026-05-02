using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public GameObject itemPrefab;            // ShopItemPrefab (아이템 프리팹)
    public Transform shopItemParent;         // 아이템들이 배치될 부모 오브젝트
    public Shop shop;                        // 판매할 아이템 데이터 (Shop 스크립터블 오브젝트)
    public ShopManager shopManager;          // 구매 처리 스크립트 참조

    private void Start()
    {
        // null 체크를 추가하여 필드들이 올바르게 설정되어 있는지 확인
        if (shop == null)
        {
            Debug.LogError("Shop is not assigned! Please assign the Shop ScriptableObject.");
            return;
        }
        if (itemPrefab == null)
        {
            Debug.LogError("Item Prefab is not assigned! Please assign the Item Prefab.");
            return;
        }
        if (shopItemParent == null)
        {
            Debug.LogError("Shop Item Parent is not assigned! Please assign the Shop Item Parent.");
            return;
        }
        if (shopManager == null)
        {
            Debug.LogError("ShopManager is not assigned! Please assign the ShopManager.");
            return;
        }

        DisplayShopItems();  // 상점 아이템을 표시하는 함수 호출
    }

    private void DisplayShopItems()
    {
        // 기존 UI 초기화
        foreach (Transform child in shopItemParent)
        {
            Destroy(child.gameObject);
        }

        // 상점 아이템들을 동적으로 생성
        foreach (var shopItem in shop.shopItems)
        {
            // 아이템 프리팹을 인스턴스화하여 생성
            GameObject itemUI = Instantiate(itemPrefab, shopItemParent);

            // 아이템 정보 영역
            var itemInfo = itemUI.transform.Find("ItemE").gameObject;  // ItemE (아이템 이름, 수량 입력, 구매 버튼을 포함한 배경 이미지)
            var itemImage = itemUI.transform.Find("ItemImage").GetComponent<Image>(); // ItemImage (아이템 이미지)
            var priceText = itemUI.transform.Find("Price").GetComponent<TMP_Text>(); // Price (아이템 가격)
            var itemNameText = itemUI.transform.Find("ItemE/ItemName").GetComponent<TMP_Text>(); // ItemName (아이템 이름)
            var buyButton = itemUI.transform.Find("ItemE/Button").GetComponent<Button>(); // Button (구매 버튼)

            // 아이템 이미지, 가격, 이름을 설정
            itemImage.sprite = shopItem.item.icon;  // 아이템 이미지 설정 (아이템 데이터에서 가져옴)
            priceText.text = $"{shopItem.item.price} ";  // 가격 설정 (아이템 데이터에서 가져옴)
            itemNameText.text = shopItem.item.itemName;  // 아이템 이름 설정 (아이템 데이터에서 가져옴)

            // 구매 버튼 클릭 시 1개씩 아이템을 구매 처리
            buyButton.onClick.AddListener(() =>
            {
                shopManager.TryBuyItem(shopItem.item, 1);  // 아이템 1개 구매 시도
            });

            // 초기에는 아이템 정보 영역(itemInfo)을 비활성화
            itemInfo.SetActive(false);
        }
    }
}
