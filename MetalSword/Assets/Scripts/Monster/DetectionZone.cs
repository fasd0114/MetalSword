using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DetectionZone : MonoBehaviour
{
    [Tooltip("MonsterAI 컴포넌트 할당")]
    public MonsterAI monsterAI;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            monsterAI.OnPlayerDetected(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            monsterAI.OnPlayerLost();
    }
}
