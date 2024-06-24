using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour , IDamage
{
    public float health = 100f;

    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log($"Player HP -{amount}");
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 플레이어 사망 처리
    }

}
