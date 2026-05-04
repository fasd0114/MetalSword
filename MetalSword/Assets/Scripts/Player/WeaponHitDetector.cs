using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitDetector : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombat;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            playerCombat.HandleWeaponHit(other);
        }
    }
}
