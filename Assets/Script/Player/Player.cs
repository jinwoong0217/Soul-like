using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamage
{
    float hp = 100f;
    public float HP
    {
        get => hp;
        set
        {
            hp = value;
            if (hp <= 0)
            {
                Die();
            }
        }
    }

    bool isInvincible = false;
    float invincibilityDuration = 1.0f; // 무적 시간
    Coroutine damageCoroutine;

    public void TakeDamage(float amount)
    {
        if (isInvincible) return;

        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }

        damageCoroutine = StartCoroutine(HandleDamage(amount));
    }

    private IEnumerator HandleDamage(float amount)
    {
        isInvincible = true;
        HP -= amount;
        Debug.Log($"Player HP: {HP}");

        // 무적 시간 동안 대기
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
        damageCoroutine = null;
    }

    private void Die()
    {
        // 플레이어 사망 처리
        Debug.Log("Player Died");
    }
}


