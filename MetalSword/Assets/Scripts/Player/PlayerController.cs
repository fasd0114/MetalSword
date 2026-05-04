using System.Linq;
using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    public System.Action<float> OnHealthChanged;

    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runMultiplier = 2f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float speedAcceleration = 5f;

    [Header("체력 설정")]
    private const int baseMaxHealth = 50;
    private const int maxHealthPerLevel = 10;

    [Header("사망 UI")]
    [SerializeField] private GameObject deathUIPanel;

    // 내부 상태
    private CharacterController cc;
    private Animator animator;
    private AudioSource audioSource;
    private Vector3 velocity;
    private float currentSpeed;
    private bool isGrounded;

    private PlayerCombat combat;

    // 체력·피격·사망 관리
    private int currentHealth;
    private bool isHit;
    private bool isDead;
    private bool deathUIShown = false;
    private Coroutine hitCoroutine;

    // 강화 수치 참조
    private Inventory inventory;
    private int cachedMaxHealth;
    private InventorySlot helmetSlot;

    // 외부 접근용 프로퍼티
    public bool IsControlLocked { get; set; } = false;
    public int MaxHealth => cachedMaxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;
    public bool IsHit => isHit;


    private void AssignSlots()
    {
        if (inventory == null) return;
        helmetSlot = inventory.items.FirstOrDefault(s => s.item != null && s.item.name == "Helmet");
    }
    private void UpdateCachedStats()
    {
        int baseHp = baseMaxHealth + PlayerStats.Instance.PlayerLevel * maxHealthPerLevel;
        int helmetBonus = helmetSlot != null ? (helmetSlot.enhancementLevel + 1) * 5 : 0;

        cachedMaxHealth = baseHp + helmetBonus;
    }
    public void RefreshStatsAfterEnhancement(int oldMax)
    {
        AssignSlots();
        UpdateCachedStats();

        int newMax = MaxHealth;

        int delta = cachedMaxHealth - oldMax;
        if (delta > 0)
        {
            currentHealth = Mathf.Clamp(currentHealth + delta, 0, cachedMaxHealth);
        }
        NotifyHealthChanged();
    }

    private void Awake()
    {
        combat = GetComponent<PlayerCombat>();
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = MaxHealth;

        inventory = Resources.Load<Inventory>("Items/Inventory");
        if (inventory != null)
        {
            inventory.OnInventoryChanged += HandleInventoryChanged;
        }

        AssignSlots();
        UpdateCachedStats();
        currentHealth = MaxHealth;
    }
    private void HandleInventoryChanged()
    {
        int oldMax = cachedMaxHealth;
        AssignSlots();
        UpdateCachedStats();

        int delta = cachedMaxHealth - oldMax;
        if (delta > 0)
        {
            currentHealth = Mathf.Clamp(currentHealth + delta, 0, cachedMaxHealth);
        }
        NotifyHealthChanged();
    }
    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= HandleInventoryChanged;
        }
    }
    private void Start()
    {
        deathUIPanel = GameObject.FindWithTag("DeathUI");
        if (deathUIPanel == null)
            Debug.LogError("DeathUI 없음");
        else
            deathUIPanel.SetActive(false);
    }
    private void Update()
    {
        // 사망 상태 체크
        if (HandleDeathState()) return;

        // 제어 잠금 상태 체크
        if (HandleControlLockedState()) return;

        // 전투 입력 및 로직 처리
        HandleCombatInput();

        // 이동 처리
        if (CanMove())
        {
            HandleMovement();
        }
        else
        {
            StopMovement();
        }
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

    #region 피격 & 사망 처리
    public void SetCurrentHealth(int hp)
    {
        currentHealth = Mathf.Clamp(hp, 0, MaxHealth);
        NotifyHealthChanged();
    }
    private void NotifyHealthChanged()
    {
        float hpRatio = Mathf.Clamp01((float)currentHealth / MaxHealth);
        OnHealthChanged?.Invoke(hpRatio);
    }

    public void ReceiveDamage(int dmg, Vector3 attackerPos)
    {
        if (isDead) return;

        // 체력 감소
        currentHealth -= dmg;

        float hpRatio = Mathf.Clamp01((float)currentHealth / MaxHealth);
        OnHealthChanged?.Invoke(hpRatio);

        // 공격자 바라보기
        Vector3 dir = attackerPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        // 피격 애니메이션
        isHit = true;
        combat.ResetCombatStates();
        animator.SetTrigger("Hit");

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
    private void HandleDeathUI()
    {
        if (!deathUIShown)
        {
            // 커서 활성화
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // UI 패널 활성화
            if (deathUIPanel != null)
                deathUIPanel.SetActive(true);

            deathUIShown = true; // 중복 실행 방지
        }
    }
    private void HandleDeath()
    {
        isDead = true;
        IsControlLocked = true;
        StopAllCoroutines();
        combat.ResetCombatStates();
        animator.SetTrigger("Die");
        cc.enabled = false;
        //this.enabled = false;
    }
    #endregion

    #region 상태 관리
    private bool HandleDeathState()
    {
        if (!isDead) return false;

        // 사망 UI가 아직 표시되지 않았다면 출력
        if (!deathUIShown)
        {
            HandleDeathUI();
        }
        return true;
    }
    private bool HandleControlLockedState()
    {
        if (!IsControlLocked) return false;

        // 제어 잠금 시 서서히 정지
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, speedAcceleration * Time.deltaTime);
        UpdateAnimationSpeed();
        return true;
    }
    private bool CanMove()
    {
        // 공격 중이거나 피격 중일 때는 이동 불가
        return !combat.IsAttacking && !combat.IsUsingSkill && !isHit;
    }

    private void StopMovement()
    {
        currentSpeed = 0f;
        UpdateAnimationSpeed();
    }

    private void UpdateAnimationSpeed()
    {
        animator.SetFloat("Speed", currentSpeed / (moveSpeed * runMultiplier));
    }
    #endregion

    #region 전투 관련
    private void HandleCombatInput()
    {
        // 피격 중이 아닐 때만 공격 가능
        if (isHit) return;

        // 일반 공격
        if (Input.GetMouseButtonDown(0))
        {
            bool leftBack = CheckIfLeftFootIsBehind();
            string trigger = leftBack ? "Attack_LeftBack" : "Attack_RightBack";
            string state = leftBack ? "Base_Attack_Left" : "Base_Attack";

            combat.Attack(trigger,state);
        }

        // 스킬 공격
        if (Input.GetKeyDown(KeyCode.C))
        {
            combat.UseSkill(); 
        }
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

    public void Revive()
    {
        // 스크립트 활성화
        this.enabled = true;

        // 플래그 초기화
        isDead = false;
        deathUIShown = false;
        IsControlLocked = false;

        isHit = false;

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
