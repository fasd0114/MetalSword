using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerCombat : MonoBehaviour
{
    // UI 업데이트를 위한 이벤트
    public System.Action<float> OnCooldownChanged;

    [Header("전투 설정")]
    [SerializeField] private float skillCooldownTime = 5f;
    [SerializeField] private Collider weaponCollider;
    private const int baseSwordDamage = 15;
    private const int swordDamagePerLevel = 5;

    [Header("오디오")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip skillSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float attackSoundDelay = 0f;
    [SerializeField] private float skillSoundDelay = 0f;

    private Animator animator;
    private AudioSource audioSource;
    private PlayerController playerController;
    private Inventory inventory;
    private HashSet<Collider> alreadyHit = new HashSet<Collider>();

    private bool isAttacking;
    private bool isUsingSkill;
    private bool isSkillOnCooldown;
    private float remainingSkillCooldown;

    private int cachedAttackDamage; // 변수명 통일
    private int cachedSkillDamage;

    // 상태 확인용 프로퍼티
    public bool IsAttacking => isAttacking;
    public bool IsUsingSkill => isUsingSkill;
    public bool IsSkillOnCooldown => isSkillOnCooldown;
    public int AttackDamage => cachedAttackDamage;
    public int SkillDamage => cachedSkillDamage;
    private void RefreshCombatStats()
    {
        if (inventory == null) return;

        var swordSlot = inventory.items.FirstOrDefault(s => s.item?.name == "Sword");
        var axeSlot = inventory.items.FirstOrDefault(s => s.item?.name == "Axe");

        int swordLvl = (swordSlot?.enhancementLevel + 1) ?? 0;
        int axeLvl = (axeSlot?.enhancementLevel + 1) ?? 0;

        int baseDmg = baseSwordDamage + PlayerStats.Instance.PlayerLevel * swordDamagePerLevel;

        // 연산 결과를 변수에 저장(캐싱)함[cite: 1]
        cachedAttackDamage = baseDmg + (swordLvl * 7 + axeLvl * 5);
        cachedSkillDamage = cachedAttackDamage * 2;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponent<PlayerController>();
        inventory = Resources.Load<Inventory>("Items/Inventory");
        if (inventory != null)
            inventory.OnInventoryChanged += RefreshCombatStats;
        RefreshCombatStats();
        weaponCollider.enabled = false;
    }


    private void Update()
    {
        UpdateCooldown();
    }
    private void UpdateCooldown()
    {
        if (isSkillOnCooldown)
        {
            remainingSkillCooldown -= Time.deltaTime;
            OnCooldownChanged?.Invoke(Mathf.Clamp01(remainingSkillCooldown / skillCooldownTime));

            if (remainingSkillCooldown <= 0f)
            {
                remainingSkillCooldown = 0f;
                isSkillOnCooldown = false;
                OnCooldownChanged?.Invoke(0f);
            }
        }
    }
    public void Attack(string trigger, string state)
    {
        if (!isAttacking && !isUsingSkill && !playerController.IsHit)
            StartCoroutine(DoAttack_Specific(trigger, state));
    }

    public void UseSkill()
    {
        if (!isSkillOnCooldown && !isAttacking && !isUsingSkill)
            StartCoroutine(DoSkill_WaitActualClip());
    }

    private IEnumerator DoAttack_Specific(string triggerName, string targetStateName)
    {
        isAttacking = true;
        if (attackSound != null) StartCoroutine(PlaySoundWithDelay(attackSound, attackSoundDelay));

        animator.CrossFade(targetStateName, 0.03f);
        alreadyHit.Clear();
        weaponCollider.enabled = true; 

        yield return WaitForStateComplete(targetStateName);

        weaponCollider.enabled = false;
        if (!playerController.IsHit) // PlayerController의 IsHit 상태 참조 필요
        {
            animator.CrossFade("Locomotion", 0.03f);
        }

        isAttacking = false;
    }

    private IEnumerator DoSkill_WaitActualClip()
    {
        isUsingSkill = true;
        isSkillOnCooldown = true;
        remainingSkillCooldown = skillCooldownTime;

        if (skillSound != null) StartCoroutine(PlaySoundWithDelay(skillSound, skillSoundDelay));

        animator.CrossFade("Base_Skill", 0.03f);
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Base_Skill"));

        alreadyHit.Clear();
        weaponCollider.enabled = true;

        float clipLength = GetCurrentClipLength("Skill");
        yield return new WaitForSeconds(clipLength);

        weaponCollider.enabled = false;
        animator.CrossFade("Locomotion", 0.03f);
        isUsingSkill = false;
    }

    public void HandleWeaponHit(Collider other)
    {
        if (!weaponCollider.enabled || !other.CompareTag("Monster") || alreadyHit.Contains(other))
            return;

        alreadyHit.Add(other);
        int dmg = isUsingSkill ? SkillDamage : AttackDamage;

        other.GetComponent<MonsterHealth>()?.TakeDamage(dmg);
        other.GetComponent<MonsterAI>()?.HandleHitReaction();

        if (hitSound != null) audioSource.PlayOneShot(hitSound);

    }
    private IEnumerator PlaySoundWithDelay(AudioClip clip, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        audioSource.PlayOneShot(clip);
    }

    private IEnumerator WaitForStateComplete(string stateName)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName)) yield return null;
        var info = animator.GetCurrentAnimatorStateInfo(0);
        while (info.IsName(stateName) && info.normalizedTime < 1f)
        {
            yield return null;
            info = animator.GetCurrentAnimatorStateInfo(0);
        }
    }

    private float GetCurrentClipLength(string key)
    {
        foreach (var ci in animator.GetCurrentAnimatorClipInfo(0))
            if (ci.clip.name.Contains(key)) return ci.clip.length;
        return 0.5f;
    }

    public void ResetCombatStates()
    {
        isAttacking = false;
        isUsingSkill = false;
        weaponCollider.enabled = false;
        StopAllCoroutines();
    }
}