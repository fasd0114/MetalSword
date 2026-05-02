using UnityEngine;

public class UIToggleManager : MonoBehaviour
{
    public GameObject inventoryUI;
    public GameObject shopUI;
    public GameObject settingsPanel;

    private bool isInventoryOpen = false;
    private bool isShopOpen = false;
    private bool isSettingsOpen => settingsPanel != null && settingsPanel.activeSelf;

    private PlayerController player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (player.IsDead)
            return;

        // 인벤토리 토글 (언제든지)
        if (Input.GetKeyDown(KeyCode.I))
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryUI.SetActive(isInventoryOpen);
        }

        // 상점 토글 (언제든지)
        if (Input.GetKeyDown(KeyCode.P))
        {
            isShopOpen = !isShopOpen;
            shopUI.SetActive(isShopOpen);
        }

        // 설정창은 기존 로직 유지
        // (만약 ESC로 닫고 싶다면 추가 처리가 필요합니다)

        bool isAnyUIOpen = isInventoryOpen || isShopOpen || isSettingsOpen;

        // 커서 락/언락
        Cursor.lockState = isAnyUIOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isAnyUIOpen;

        // 플레이어 조작 잠금
        player.IsControlLocked = isAnyUIOpen;
    }
}
