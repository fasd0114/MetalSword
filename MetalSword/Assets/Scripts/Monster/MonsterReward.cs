using UnityEngine;

public class MonsterReward : MonoBehaviour
{
    private MonsterData data;

    public void Setup(MonsterData newData)
    {
        data = newData;
    }

    public void GiveReward()
    {
        if (data == null) return;

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddGold(data.goldReward); // SO 데이터 사용[cite: 12, 13]
            PlayerStats.Instance.AddExp(data.expReward);
        }

        if (data.deathEffectPrefab != null)
            Instantiate(data.deathEffectPrefab, transform.position, Quaternion.identity);
    }
}