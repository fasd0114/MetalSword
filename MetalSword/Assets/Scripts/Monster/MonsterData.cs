using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "ScriptableObject/MonsterData")]
public class MonsterData : ScriptableObject
{
    [Header("능력치 설정")]
    public int maxHp = 100;         // 기존 MonsterHealth의 체력 변수[cite: 5]
    public float attackRange = 2f;  // 기존 MonsterAI의 공격 사거리[cite: 7, 10]
    public float attackCooldown = 1.5f; // 기존 MonsterAI의 쿨타임[cite: 7, 10]

    [Header("보상 설정")]
    public int goldReward = 10;     // 분리된 보상 변수
    public int expReward = 5;       // 분리된 보상 변수
    public GameObject deathEffectPrefab; // 사망 이펙트[cite: 7, 10]
}