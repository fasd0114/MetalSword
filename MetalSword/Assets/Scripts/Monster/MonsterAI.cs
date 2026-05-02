using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum MonsterState { Idle, Wander, Chase, Attack, Hit, Die }

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(MonsterHealth))]
[RequireComponent(typeof(MonsterReward))]
public class MonsterAI : MonoBehaviour
{
    [Header("상태 확인")]
    [SerializeField] private MonsterState currentState = MonsterState.Idle;

    [Header("데이터 설정")]
    public MonsterData monsterData;
    public Collider roamArea;   
    public Collider weaponCollider;

    private NavMeshAgent agent;
    private Animator animator;
    private MonsterHealth health;
    private MonsterReward rewardSystem;
    private Transform player;

    private float lastAttackTime;
    private MonsterWeaponHitDetector[] weaponDetectors;
    private Dictionary<string, float> clipLengthCache = new Dictionary<string, float>();

    private float attackRange => monsterData.attackRange;
    private float attackCooldown => monsterData.attackCooldown;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        health = GetComponent<MonsterHealth>();
        rewardSystem = GetComponent<MonsterReward>();

        if (monsterData != null)
        {
            agent.stoppingDistance = attackRange;
            rewardSystem.Setup(monsterData);
        }

        agent.stoppingDistance = attackRange;
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
            weaponDetectors = weaponCollider.GetComponentsInChildren<MonsterWeaponHitDetector>();
        }
        CacheAnimationClips();
    }

    private void OnEnable() => health.OnDeath += () => SetState(MonsterState.Die);
    private void Start() => SetState(MonsterState.Wander);

    private void Update()
    {
        if (currentState == MonsterState.Die) return;

        UpdateAnimation();
        CheckPlayerStatus();
        HandleStateUpdate();
    }

    // 상태 변경 중앙 제어
    private void SetState(MonsterState newState)
    {
        if (currentState == MonsterState.Die) return;

        // 이전 상태 종료 로직
        StopAllCoroutines();
        agent.isStopped = true;

        currentState = newState;

        // 새로운 상태 시작 로직
        switch (currentState)
        {
            case MonsterState.Wander: StartCoroutine(WanderRoutine()); break;
            case MonsterState.Chase: StartCoroutine(ChaseRoutine()); break;
            case MonsterState.Attack: StartCoroutine(AttackRoutine()); break;
            case MonsterState.Hit: StartCoroutine(HitRoutine()); break;
            case MonsterState.Die: StartCoroutine(DieRoutine()); break;
        }
    }

    private void HandleStateUpdate()
    {
        if (currentState == MonsterState.Chase && player != null)
        {
            // 바라보기 로직 유지
            Vector3 dir = player.position - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }
    }

    // 기존 로밍/추격/공격 로직들
    private IEnumerator WanderRoutine()
    {
        while (currentState == MonsterState.Wander)
        {
            agent.isStopped = false;
            Vector3 targetPos = GetRandomRoamPos();
            agent.SetDestination(targetPos);

            yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);

            agent.isStopped = true;
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
    }

    private IEnumerator ChaseRoutine()
    {
        while (currentState == MonsterState.Chase)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            if (agent.remainingDistance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                SetState(MonsterState.Attack);
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator AttackRoutine()
    {
        animator.SetTrigger("Attack");

        if (weaponDetectors != null)
            foreach (var mw in weaponDetectors) mw.ResetHit();

        weaponCollider.enabled = true;
        yield return new WaitForSeconds(GetClipLength("Attack"));
        weaponCollider.enabled = false;

        lastAttackTime = Time.time;
        SetState(player != null ? MonsterState.Chase : MonsterState.Wander);
    }

    public void HandleHitReaction()
    {
        if (currentState != MonsterState.Die)
            SetState(MonsterState.Hit);
    }

    private IEnumerator HitRoutine()
    {
        animator.SetTrigger("Hit");
        yield return new WaitForSeconds(GetClipLength("Hit"));
        SetState(player != null ? MonsterState.Chase : MonsterState.Wander);
    }

    private IEnumerator DieRoutine()
    {
        agent.isStopped = true;
        GetComponent<Collider>().enabled = false;
        animator.SetTrigger("Die");

        yield return new WaitForSeconds(GetClipLength("Die"));
        rewardSystem.GiveReward();
        Destroy(gameObject);
    }

    // 외부 감지 신호 처리
    public void OnPlayerDetected(Transform pl) { player = pl; SetState(MonsterState.Chase); }
    public void OnPlayerLost() { if (currentState != MonsterState.Attack) SetState(MonsterState.Wander); }

    // 유틸리티 메서드
    private void CacheAnimationClips()
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.Contains("Attack")) clipLengthCache["Attack"] = clip.length;
            else if (clip.name.Contains("Hit")) clipLengthCache["Hit"] = clip.length;
            else if (clip.name.Contains("Die")) clipLengthCache["Die"] = clip.length;
        }
    }

    private float GetClipLength(string key) => clipLengthCache.TryGetValue(key, out float len) ? len : 0.5f;
    private void UpdateAnimation() => animator.SetFloat("Speed", agent.velocity.magnitude);
    private void CheckPlayerStatus() { if (player != null && player.GetComponent<PlayerController>().IsDead) { player = null; SetState(MonsterState.Wander); } }
    private Vector3 GetRandomRoamPos()
    {
        var b = roamArea.bounds;
        Vector3 rnd = new Vector3(Random.Range(b.min.x, b.max.x), b.center.y, Random.Range(b.min.z, b.max.z));
        return NavMesh.SamplePosition(rnd, out var hit, 2f, NavMesh.AllAreas) ? hit.position : transform.position;
    }
}