using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemUI : MonoBehaviour
{
    public GameObject itemInfo;           // ItemE 오브젝트 (가격, 버튼 포함)
    public Image itemImage;               // 아이템 이미지
    public TMP_Text priceText;            // 가격 텍스트
    public Button buyButton;              // 구매 버튼
    public Canvas itemInfoCanvas;         // ItemE의 Canvas
    public GraphicRaycaster graphicRaycaster; // ItemE의 GraphicRaycaster
    public int baseSortingOrder = 0;      // 기본 Sorting Order
    public int hoverSortingOrder = 10;    // 마우스가 올라갔을 때 ItemE의 Sorting Order 값

    private bool isHoveringSlot = false;
    private bool isHoveringInfo = false;

    private void Start()
    {
        // 아이템 슬롯에 마우스 오버 이벤트 추가
        AddPointerEvents(this.gameObject,
            () => { isHoveringSlot = true; UpdateInfoState(); },
            () => { isHoveringSlot = false; UpdateInfoState(); }
        );

        // ItemE에 마우스 오버 이벤트 추가
        AddPointerEvents(itemInfo,
            () => { isHoveringInfo = true; UpdateInfoState(); },
            () => { isHoveringInfo = false; UpdateInfoState(); }
        );

        // 처음에는 비활성화 상태로 설정
        itemInfo.SetActive(false);

        // ItemE의 Canvas에 기본 Sorting Order 설정
        if (itemInfoCanvas != null)
        {
            itemInfoCanvas.sortingOrder = baseSortingOrder;
        }

        // GraphicRaycaster는 기본적으로 비활성화 상태로 설정
        if (graphicRaycaster != null)
        {
            graphicRaycaster.enabled = false;
        }
    }

    private void AddPointerEvents(GameObject target, System.Action onEnter, System.Action onExit)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = target.AddComponent<EventTrigger>();

        var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((_) => onEnter());
        trigger.triggers.Add(entryEnter);

        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((_) => onExit());
        trigger.triggers.Add(entryExit);
    }

    private void UpdateInfoState()
    {
        // ItemE 활성화 여부 업데이트
        itemInfo.SetActive(isHoveringSlot || isHoveringInfo);

        // ItemE가 활성화되면 Sorting Order를 조정하여 다른 슬롯 위에 올림
        if (itemInfo.activeSelf)
        {
            itemInfoCanvas.sortingOrder = hoverSortingOrder;
            // GraphicRaycaster를 활성화하여 버튼 클릭 가능하게 설정
            if (graphicRaycaster != null)
            {
                graphicRaycaster.enabled = true;
            }
        }
        else
        {
            // ItemE가 비활성화되면 기본 Sorting Order로 되돌림
            itemInfoCanvas.sortingOrder = baseSortingOrder;
            // GraphicRaycaster를 비활성화하여 클릭 방지
            if (graphicRaycaster != null)
            {
                graphicRaycaster.enabled = false;
            }
        }
    }
}
