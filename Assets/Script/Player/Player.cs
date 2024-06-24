using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour , IDamage
{
    public float hp = 100f;
    bool isInvincible = false;
    float invincibilityDuration = 1.0f; // 무적 시간

    public void TakeDamage(float amount)
    {
        if (isInvincible) return;

        StartCoroutine(HandleDamage(amount));
    }

    private IEnumerator HandleDamage(float amount)
    {
        isInvincible = true;
        hp -= amount;
        Debug.Log($"Player HP -{hp}");

        if (hp <= 0)
        {
            Die();
        }

        // 무적 시간 동안 대기
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    private void Die()
    {
        // 플레이어 사망 처리
    }

}
