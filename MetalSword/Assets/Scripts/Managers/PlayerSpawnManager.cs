using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [Header("GameSettings 참조")]
    [SerializeField] private GameSettings settings;

    [Header("직업별 플레이어 프리팹")]
    [SerializeField] private GameObject archerPrefab;
    [SerializeField] private GameObject wizardPrefab;
    [SerializeField] private GameObject swordsmanPrefab;

    [Header("플레이어 시작 위치")]
    [SerializeField] private Transform spawnPoint;



    private void Awake()
    {
        SpawnPlayerAndSetupCamera();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SpawnPlayerAndSetupCamera()
    {
        // 프리팹 선택
        GameObject prefab = settings.StartingClass switch
        {
            CharacterClass.Archer => archerPrefab,
            CharacterClass.Wizard => wizardPrefab,
            CharacterClass.Swordsman => swordsmanPrefab,
            _ => throw new System.ArgumentOutOfRangeException()
        };

        // 인스턴스 생성
        var player = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        // 카메라 연결
        var camOrbit = Camera.main.GetComponent<CameraOrbit>();
        if (camOrbit != null)
            camOrbit.SetTarget(player.transform);
    }
}
