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

    private void Start()
    {
        stats = PlayerStats.Instance;
        playerCtrl = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        // 레벨/경험치
        levelText.text = $"{stats.PlayerLevel}";
        xpFillImage.fillAmount =
            Mathf.Clamp01((float)stats.CurrentExp / stats.ExpToNextLevel);

        // HP
        if (playerCtrl != null)
            hpFillImage.fillAmount =
            Mathf.Clamp01((float)playerCtrl.CurrentHealth
                                         / playerCtrl.MaxHealth);

        // 스킬 쿨타임
        if (playerCtrl.IsSkillOnCooldown)
        {
            float ratio = playerCtrl.RemainingSkillCooldown
                        / playerCtrl.SkillCooldownTime;
            skillCooldownOverlay.fillAmount = Mathf.Clamp01(ratio);
        }
        else
        {
            skillCooldownOverlay.fillAmount = 0f;
        }
    }
}
