using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public float damaged = 10f;
    public float minDistance = 2f; // 최소 닿는 거리

    private void OnTriggerEnter(Collider other)
    {
        IDamage damage = other.GetComponent<IDamage>();
        if (damage != null)
        {
            float distanceSqr = (transform.position - other.transform.position).sqrMagnitude;
            if (distanceSqr <= minDistance)
            {
                if ((gameObject.CompareTag("PlayerSword") && other.CompareTag("Enemy")) ||
                    (gameObject.CompareTag("EnemyWeapon") && other.CompareTag("Player")))
                {
                    if (!other.GetComponent<Player>().isInvincible)
                    {
                        damage.TakeDamage(damaged);
                    }
                }
            }
        }
    }
}
