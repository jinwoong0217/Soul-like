using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public float damaged = 10f;

    private void OnTriggerEnter(Collider other)
    {
        IDamage damage = other.GetComponent<IDamage>();
        if (damage != null)
        {
            if ((gameObject.CompareTag("PlayerSword") && other.CompareTag("Enemy")) ||
                (gameObject.CompareTag("EnemyWeapon") && other.CompareTag("Player")))
            {
                damage.TakeDamage(damaged);
            }
        }
    }
}
