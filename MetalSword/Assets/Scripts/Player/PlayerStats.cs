// PlayerStats.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }
    public Action<int, float> OnXpChanged;

    [Header("골드·경험치·레벨 설정")]
    [SerializeField] private int startingGold = 100;
    [SerializeField] private int startingExp = 0;
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private int startingExpToNextLevel = 100;

    public int CurrentGold { get; private set; }
    public int CurrentExp { get; private set; }
    public int PlayerLevel { get; private set; }
    public int ExpToNextLevel { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 시작값 초기화
        CurrentGold = startingGold;
        CurrentExp = startingExp;
        PlayerLevel = startingLevel;
        ExpToNextLevel = startingExpToNextLevel;
    }

    public void SetStats(int gold, int exp, int level, int expToNext)
    {
        CurrentGold = gold;
        CurrentExp = exp;
        PlayerLevel = level;
        ExpToNextLevel = expToNext;
    }

    public void AddGold(int amount)
    {
        CurrentGold += amount;
        Debug.Log($"[PlayerStats] Gold +{amount} → {CurrentGold}");
    }

    public void AddExp(int amount)
    {
        CurrentExp += amount;
        while (CurrentExp >= ExpToNextLevel)
        {
            CurrentExp -= ExpToNextLevel;
            PlayerLevel++;
            ExpToNextLevel = Mathf.RoundToInt(ExpToNextLevel * 1.1f);
        }

        // 값이 변경된 후 이벤트 호출
        float xpRatio = Mathf.Clamp01((float)CurrentExp / ExpToNextLevel);
        OnXpChanged?.Invoke(PlayerLevel, xpRatio);
    }
    public void ResetToStartingValues()
    {
        CurrentGold = startingGold;
        CurrentExp = startingExp;
        PlayerLevel = startingLevel;
        ExpToNextLevel = startingExpToNextLevel;
    }

}
