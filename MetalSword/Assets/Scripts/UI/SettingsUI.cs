using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("UI 패널 & 버튼")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button exitToMenuButton;
    [SerializeField] private Button resetSaveButton;

    [Header("저장 대상 컴포넌트")]
    [SerializeField] private Inventory inventoryData;
    [SerializeField] private PlayerController playerController;
    private void Awake()
    {
        // 에디터에서 할당 안 했을 때 기본값 로드
        
        if (playerController == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                playerController = player.GetComponent<PlayerController>();
        }
    }

    private void Start()
    {
        if (inventoryData == null)
            inventoryData = Resources.Load<Inventory>("Items/Inventory");

        settingsPanel.SetActive(false);

        exitToMenuButton.onClick.AddListener(() =>
        {
            DataManager.Instance?.SaveCurrentGame(playerController, inventoryData);
            SceneManager.LoadScene("MainMenu");
        });
        resetSaveButton.onClick.AddListener(ResetAndBackToMenu);
    }

    private void Update()
    {
        if (playerController.IsDead)
            return;
        if (Input.GetKeyDown(KeyCode.Escape))
            settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void ResetAndBackToMenu()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.ResetAllGameData(inventoryData);
        }
    }

    private void OnApplicationQuit()
    {
        DataManager.Instance?.SaveCurrentGame(playerController, inventoryData);
    }
}
