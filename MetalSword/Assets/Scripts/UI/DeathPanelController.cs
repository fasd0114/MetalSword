using UnityEngine;
using UnityEngine.UI;

public class DeathPanelController : MonoBehaviour
{
    [Header("버튼 참조")]
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button resetButton;

    private PlayerController playerController;
    private SettingsUI settingsUI;

    private void Start()
    {
        // 버튼 리스너 연결
        respawnButton.onClick.AddListener(OnRespawnClicked);
        resetButton.onClick.AddListener(OnResetClicked);

        // 컴포넌트 찾아두기
        playerController = GameObject.FindWithTag("Player")
                          .GetComponent<PlayerController>();
        settingsUI = FindObjectOfType<SettingsUI>();
    }

    private void OnRespawnClicked()
    {
        // PlayerController 쪽 Revive 호출
        playerController.Revive();
    }

    private void OnResetClicked()
    {
        // SettingsUI 쪽 리셋 및 메인 메뉴 이동 호출
        settingsUI.ResetAndBackToMenu();
    }
}
