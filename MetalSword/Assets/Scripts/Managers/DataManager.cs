using System;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private const string SAVE_KEY = "MyRPG_Save";
    public bool HasSaveData => PlayerPrefs.HasKey(SAVE_KEY);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        // 씬 로드 시 호출될 함수 등록
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 오브젝트 파괴 시 이벤트 해제 (메모리 누수 방지)
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드될 때마다 실행되는 함수
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 메인 게임 씬(예: "GameScene")일 때만 로드 수행
        if (scene.name == "GameScene")
        {
            TryAutoLoad();
        }
    }

    private void TryAutoLoad()
    {
        var playerObj = GameObject.FindWithTag("Player");
        var playerController = playerObj?.GetComponent<PlayerController>();
        var inventoryData = Resources.Load<Inventory>("Items/Inventory");

        if (playerController != null && inventoryData != null)
        {
            LoadGame(playerController, inventoryData);
        }
    }

    [Serializable]
    private class SaveData
    {
        public float posX, posY, posZ;
        public int gold, exp, level, expToNextLevel;
        public int currentHealth, maxHealth;
        public string[] itemNames;
        public int[] itemQuantities;
        public int[] itemEnhancementLevels;
    }

    public void SaveCurrentGame(PlayerController playerController, Inventory inventoryData)
    {
        if (playerController == null || inventoryData == null) return;

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) return;

        var stats = PlayerStats.Instance;
        var data = new SaveData
        {
            posX = playerObj.transform.position.x,
            posY = playerObj.transform.position.y,
            posZ = playerObj.transform.position.z,
            gold = stats.CurrentGold,
            exp = stats.CurrentExp,
            level = stats.PlayerLevel,
            expToNextLevel = stats.ExpToNextLevel,
            currentHealth = playerController.CurrentHealth,
            maxHealth = playerController.MaxHealth
        };

        foreach (var up in FindObjectsOfType<InventoryUp>())
        {
            int index = up.transform.GetSiblingIndex();
            if (index >= 0 && index < inventoryData.items.Count)
                inventoryData.items[index].enhancementLevel = up.inventorySlot.enhancementLevel;
        }

        int count = inventoryData.items.Count;
        data.itemNames = new string[count];
        data.itemQuantities = new int[count];
        data.itemEnhancementLevels = new int[count];

        for (int i = 0; i < count; i++)
        {
            var slot = inventoryData.items[i];
            data.itemNames[i] = slot.item?.itemName ?? "";
            data.itemQuantities[i] = slot.quantity;
            data.itemEnhancementLevels[i] = slot.enhancementLevel;
        }

        string json = JsonUtility.ToJson(data);

        Debug.Log($"JSON 직렬화 완료:\n{json}");

        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("DataManager: 저장 완료");
    }

    public void LoadGame(PlayerController playerController, Inventory inventoryData)
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY)) return;
                
        // 데이터 역직렬화
            string json = PlayerPrefs.GetString(SAVE_KEY);
            var data = JsonUtility.FromJson<SaveData>(json);

        // 데이터 복구
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerObj.transform.position = new Vector3(data.posX, data.posY, data.posZ);

        PlayerStats.Instance.SetStats(data.gold, data.exp, data.level, data.expToNextLevel);
        playerController.SetCurrentHealth(data.currentHealth);

        var allItems = Resources.LoadAll<ItemData>("Items");
        for (int i = 0; i < data.itemNames.Length; i++)
        {
            var nameKey = data.itemNames[i];
            if (string.IsNullOrEmpty(nameKey)) continue;

            var itemAsset = allItems.FirstOrDefault(x => x.itemName == nameKey);
            if (itemAsset != null)
            {
                var slot = inventoryData.items[i];
                slot.SetItem(itemAsset, data.itemQuantities[i]);
                slot.enhancementLevel = data.itemEnhancementLevels[i];
            }
        }

        FindObjectOfType<InventoryUI>()?.RefreshUI();
        foreach (var up in FindObjectsOfType<InventoryUp>())
            up.SendMessage("UpdateEnhancementText", SendMessageOptions.DontRequireReceiver);

        Debug.Log("DataManager: 로드 완료");
    }

    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
    }

    public void ResetAllGameData(Inventory inventoryData)
    {
        // 저장된 물리 데이터 삭제
        DeleteSaveData();

        // 게임 시스템 값 초기화
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ResetToStartingValues();
        }

        // 인벤토리 데이터 리셋
        if (inventoryData != null)
        {
            inventoryData.ResetInventory();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}