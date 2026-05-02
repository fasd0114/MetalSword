using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitDetector : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            playerController.HandleWeaponHit(other);
        }
    }
}
