using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("레벨 표시")]
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("체력 바")]
    [SerializeField] private Image hpFillImage;

    [Header("경험치 바")]
    [SerializeField] private Image xpFillImage;

    [Header("스킬 쿨다운 오버레이")]
    [SerializeField] private Image skillCooldownOverlay;

    private PlayerStats stats;
    private PlayerController playerCtrl;
    private PlayerCombat playerCombat;

    private void Start()
    {
        stats = PlayerStats.Instance;
        playerCtrl = FindObjectOfType<PlayerController>();
        playerCombat = FindObjectOfType<PlayerCombat>();

        if (stats != null)
            UpdateXpUI(stats.PlayerLevel, (float)stats.CurrentExp / stats.ExpToNextLevel);

        if (playerCtrl != null)
        {
            UpdateHpUI((float)playerCtrl.CurrentHealth / playerCtrl.MaxHealth);
            UpdateCooldownUI(0f);

            stats.OnXpChanged += UpdateXpUI;
            playerCtrl.OnHealthChanged += UpdateHpUI;
        }
        if (playerCombat != null) // 추가
        {
            playerCombat.OnCooldownChanged += UpdateCooldownUI; // Combat의 이벤트를 구독
        }
    }
    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnXpChanged -= UpdateXpUI;

        if (playerCtrl != null)
        {
            playerCtrl.OnHealthChanged -= UpdateHpUI;
        }
        if (playerCombat != null)
            playerCombat.OnCooldownChanged -= UpdateCooldownUI;
    }
    private void UpdateXpUI(int level, float xpRatio)
    {
        levelText.text = level.ToString();
        xpFillImage.fillAmount = xpRatio;
    }

    private void UpdateHpUI(float hpRatio)
    {
        hpFillImage.fillAmount = hpRatio;
    }

    private void UpdateCooldownUI(float cooldownRatio)
    {
        skillCooldownOverlay.fillAmount = cooldownRatio;
    }
}
