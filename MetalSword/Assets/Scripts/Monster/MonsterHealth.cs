using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MonsterHealth : MonoBehaviour
{
    public event Action OnDeath;
    public MonsterData monsterData;

    [SerializeField] private int currentHp;
    public bool IsDead = false;

    private void Awake()
    {
        // SO에 설정된 maxHp로 현재 체력 초기화
        if (monsterData != null)
            currentHp = monsterData.maxHp;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        currentHp = Mathf.Max(currentHp - amount, 0);
        if (currentHp == 0)
        {
            IsDead = true;
            OnDeath?.Invoke();
        }
    }
}
