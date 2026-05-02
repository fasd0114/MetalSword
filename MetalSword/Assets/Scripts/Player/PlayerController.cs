using System.Linq;
using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runMultiplier = 2f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float speedAcceleration = 5f;

    [Header("쿨타임 설정")]
    [SerializeField] private float skillCooldownTime = 5f;

    [Header("무기 설정")]
    [SerializeField] private Collider weaponCollider;

    [Header("데미지 설정")]
    private const int baseSwordDamage = 15;
    private const int swordDamagePerLevel = 5;

    [Header("체력 설정")]
    private const int baseMaxHealth = 50;
    private const int maxHealthPerLevel = 10;

    [Header("오디오 클립")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip skillSound;
    [SerializeField] private AudioClip hitSound;

    [Header("오디오 딜레이 (초)")]
    [SerializeField] private float attackSoundDelay = 0f;
    [SerializeField] private float skillSoundDelay = 0f;
    [SerializeField] private GameObject deathUIPanel;

    [SerializeField] private Transform spawnPoint;

    // 내부 상태
    private CharacterController cc;
    private Animator animator;
    private AudioSource audioSource;
    private Vector3 velocity;
    private float currentSpeed;
    private bool isGrounded;
    private bool isAttacking;
    private bool isUsingSkill;
    private bool isSkillOnCooldown;
    private float remainingSkillCooldown;
    private HashSet<Collider> alreadyHit = new HashSet<Collider>();

    // 체력·피격·사망 관리
    private int currentHealth;
    private bool isHit;
    private bool isDead;
    private bool deathUIShown = false;
    private Coroutine attackCoroutine;
    private Coroutine skillCoroutine;
    private Coroutine hitCoroutine;

    // 강화 수치 참조
    private Inventory inventory;
    private InventorySlot helmetSlot;
    private InventorySlot swordSlot;
    private InventorySlot axeSlot;

    // 외부 접근용 프로퍼티
    public bool IsControlLocked { get; set; } = false;
    public int CurrentHealth => currentHealth;
    public int MaxHealth
    {
        get
        {
            int baseHp = baseMaxHealth + PlayerStats.Instance.PlayerLevel * maxHealthPerLevel;
            int helmetBonus = helmetSlot != null
                ? (helmetSlot.enhancementLevel+1) * 5
                : 0;
            return baseHp + helmetBonus;
        }
    }
    public bool IsDead => isDead;
    public bool IsSkillOnCooldown => isSkillOnCooldown;
    public float RemainingSkillCooldown => remainingSkillCooldown;
    public float SkillCooldownTime => skillCooldownTime;
    public int AttackDamage
    {
        get
        {
            // 기본 + 레벨 당 증가
            int baseDmg = baseSwordDamage + PlayerStats.Instance.PlayerLevel * swordDamagePerLevel;

            // 강화 레벨 합산 보너스
            int swordLvl = (swordSlot?.enhancementLevel + 1) ?? 0;
            int axeLvl = (axeSlot?.enhancementLevel + 1) ?? 0;
            int bonus = swordLvl * 7 + axeLvl * 5;

            return baseDmg + bonus;
        }
    }
    public int SkillDamage => AttackDamage * 2;

    void AssignSlots()
    {
        if (inventory == null) return;
        helmetSlot = inventory.items.FirstOrDefault(s => s.item != null && s.item.name == "Helmet");
        swordSlot = inventory.items.FirstOrDefault(s => s.item != null && s.item.name == "Sword");
        axeSlot = inventory.items.FirstOrDefault(s => s.item != null && s.item.name == "Axe");
    }

    public void RefreshStatsAfterEnhancement(int oldMax)
    {
        AssignSlots();

        int newMax = MaxHealth;

        // 증가분만큼 currentHealth에 추가
        int delta = newMax - oldMax;
        if (delta > 0)
        {
            currentHealth = Mathf.Clamp(currentHealth + delta, 0, newMax);
        }
    }

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        weaponCollider.enabled = false;
        currentHealth = MaxHealth;

        inventory = Resources.Load<Inventory>("Items/Inventory");     
    }

    private void Start()
    {
        deathUIPanel = GameObject.FindWithTag("DeathUI");
        if (deathUIPanel == null)
            Debug.LogError("▶ DeathUIPanel을 태그 ‘DeathUI’로 설정했는지 확인하세요!");
        else
            deathUIPanel.SetActive(false);
    }
    private void Update()
    {
        AssignSlots();

        if (isDead)
        {
            // 사망 UI 한 번만 띄우기
            if (!deathUIShown)
            {
                // 커서 활성화해서 마우스 클릭은 가능하도록
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (deathUIPanel != null)
                    deathUIPanel.SetActive(true);
                deathUIShown = true;
            }
            // 모든 키 입력 무시
            return;
        }
        if (IsControlLocked)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, speedAcceleration * Time.deltaTime);
            animator.SetFloat("Speed", currentSpeed / (moveSpeed * runMultiplier));
            return;
        }

        //스킬 쿨다운 갱신
        if (isSkillOnCooldown)
        {
            remainingSkillCooldown -= Time.deltaTime;
            if (remainingSkillCooldown <= 0f)
            {
                remainingSkillCooldown = 0f;
                isSkillOnCooldown = false;
            }
        }
        // 공격 입력
        if (!isAttacking && !isUsingSkill && !isHit)
        {
            if (Input.GetMouseButtonDown(0))
            {
                bool leftBack = CheckIfLeftFootIsBehind();
                string triggerName = leftBack ? "Attack_LeftBack" : "Attack_RightBack";
                string stateName = leftBack ? "Base_Attack_Left" : "Base_Attack";
                attackCoroutine = StartCoroutine(DoAttack_Specific(triggerName, stateName));
            }
            if (Input.GetKeyDown(KeyCode.C) && !isSkillOnCooldown)
                skillCoroutine = StartCoroutine(DoSkill_WaitActualClip());
        }
        // 공격 중엔 Speed=0
        if (isAttacking || isUsingSkill || isHit)
        {
            animator.SetFloat("Speed", 0f);
            currentSpeed = 0f;
            return;
        }
        // 평상시 이동 처리
        HandleMovement();
    }

    #region 이동 처리
    private void HandleMovement()
    {
        isGrounded = cc.isGrounded;
        velocity.y = isGrounded ? -1f : velocity.y + Physics.gravity.y * Time.deltaTime;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 camF = Camera.main.transform.forward; camF.y = 0; camF.Normalize();
        Vector3 camR = Camera.main.transform.right; camR.y = 0; camR.Normalize();
        Vector3 dir = camR * x + camF * z;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationSpeed * Time.deltaTime);
        }

        bool running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float targetSpeed = dir.sqrMagnitude > 0.001f
            ? (running ? moveSpeed * runMultiplier : moveSpeed)
            : 0f;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedAcceleration * Time.deltaTime);
        animator.SetFloat("Speed", currentSpeed / (moveSpeed * runMultiplier));

        Vector3 motion = dir * currentSpeed;
        motion.y = velocity.y;
        cc.Move(motion * Time.deltaTime);
    }
    #endregion

    #region 공격·스킬 코루틴
    private IEnumerator DoAttack_Specific(string triggerName, string targetStateName)
    {
        isAttacking = true;
        if (attackSound != null)
            StartCoroutine(PlaySoundWithDelay(attackSound, attackSoundDelay));

        animator.CrossFade(targetStateName, 0.03f);
        alreadyHit.Clear();
        EnableWeaponCollider();

        yield return WaitForStateComplete(targetStateName);

        DisableWeaponCollider();

        if (!isHit)
        {
            animator.CrossFade("Locomotion", 0.03f);
            yield return new WaitForSeconds(0.03f);
        }

        isAttacking = false;
        attackCoroutine = null;
    }

    private IEnumerator DoSkill_WaitActualClip()
    {
        isUsingSkill = true;
        isSkillOnCooldown = true;
        remainingSkillCooldown = skillCooldownTime;

        if (skillSound != null)
            StartCoroutine(PlaySoundWithDelay(skillSound, skillSoundDelay));

        // 스킬 애니메이션 시작
        animator.CrossFade("Base_Skill", 0.03f);

        // 애니메이션 상태가 전환될 때까지 대기
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("Base_Skill"));

        // 히트 초기화 및 콜라이더 활성화 
        alreadyHit.Clear();
        EnableWeaponCollider();

        // 애니메이션 길이만큼 대기
        float clipLength = GetCurrentClipLength("Skill");
        yield return new WaitForSeconds(clipLength);

        // 애니메이션 종료 시 비활성화
        DisableWeaponCollider();

        // 스킬 종료 후 대기 없이 바로 Locomotion
        animator.CrossFade("Locomotion", 0.03f);
        yield return new WaitForSeconds(0.03f);

        isUsingSkill = false;
        skillCoroutine = null;
    }


    private IEnumerator PlaySoundWithDelay(AudioClip clip, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        audioSource.PlayOneShot(clip);
    }

    private IEnumerator WaitForStateComplete(string stateName)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;
        var info = animator.GetCurrentAnimatorStateInfo(0);
        while (info.IsName(stateName) && info.normalizedTime < 1f)
        {
            yield return null;
            info = animator.GetCurrentAnimatorStateInfo(0);
        }
    }
    #endregion

    #region 무기 히트 처리
    public void HandleWeaponHit(Collider other)
    {
        if (!weaponCollider.enabled
            || !other.CompareTag("Monster")
            || alreadyHit.Contains(other))
            return;

        alreadyHit.Add(other);
        int dmg = isUsingSkill ? SkillDamage : AttackDamage;

        other.GetComponent<MonsterHealth>()?.TakeDamage(dmg);
        other.GetComponent<MonsterAI>()?.HandleHitReaction();

        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);
    }
    public void EnableWeaponCollider() => weaponCollider.enabled = true;
    public void DisableWeaponCollider() => weaponCollider.enabled = false;
    #endregion

    #region 피격 & 사망 처리
    public void SetCurrentHealth(int hp)
    {
        currentHealth = Mathf.Clamp(hp, 0, MaxHealth);
    }
    public void ReceiveDamage(int dmg, Vector3 attackerPos)
    {
        if (isDead) return;

        // 체력 감소
        currentHealth -= dmg;

        // 공격자 바라보기
        Vector3 dir = attackerPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        // 피격 애니메이션
        isHit = true;
        isAttacking = false;
        isUsingSkill = false;
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        if (skillCoroutine != null) StopCoroutine(skillCoroutine);
        DisableWeaponCollider();

        animator.SetTrigger("Hit");
        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);
        hitCoroutine = StartCoroutine(HandleHit());

        // 사망 체크
        if (currentHealth <= 0)
            HandleDeath();
    }

    private IEnumerator HandleHit()
    {
        yield return WaitForStateComplete("Hit");
        isHit = false;
        animator.CrossFade("Locomotion", 0.03f);
    }

    private void HandleDeath()
    {
        isDead = true;
        IsControlLocked = true;
        StopAllCoroutines();
        DisableWeaponCollider();
        animator.SetTrigger("Die");
        cc.enabled = false;
        //this.enabled = false;
    }
    #endregion

    private bool CheckIfLeftFootIsBehind()
    {
        var lf = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        var rf = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        if (lf == null || rf == null) return true;
        Vector3 l = transform.InverseTransformPoint(lf.position);
        Vector3 r = transform.InverseTransformPoint(rf.position);
        return l.z < r.z;
    }

    private float GetCurrentClipLength(string key)
    {
        foreach (var ci in animator.GetCurrentAnimatorClipInfo(0))
            if (ci.clip.name.Contains(key))
                return ci.clip.length;
        return 0.5f;
    }

    public void Revive()
    {
        // 스크립트 활성화
        this.enabled = true;

        // 플래그 초기화
        isDead = false;
        deathUIShown = false;
        IsControlLocked = false;

        isHit = false;
        isAttacking = false;
        isUsingSkill = false;

        // 애니메이터 리셋
        animator.ResetTrigger("Die");
        animator.SetBool("IsDead", false);
        animator.Rebind();           
        animator.Update(0f);         

        // 기본 대기 상태로 이동
        animator.Play("Idle", 0, 0f);

        // 체력 회복
        SetCurrentHealth(MaxHealth);

        // 컨트롤러, 위치, 커서 등 복구
        cc.enabled = true;
        var sp = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (sp != null) transform.position = sp.transform.position;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 데스 UI 숨기기
        if (deathUIPanel != null) deathUIPanel.SetActive(false);
    }
}
