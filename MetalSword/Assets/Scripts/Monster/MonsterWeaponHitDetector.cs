using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MonsterWeaponHitDetector : MonoBehaviour
{
    public int damage = 10;

    private bool hasHit = false;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        hasHit = false;
    }

    public void ResetHit()
    {
        hasHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        if (!other.CompareTag("Player")) return;

        var pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.ReceiveDamage(damage, transform.position);
            hasHit = true;
        }
    }
}
