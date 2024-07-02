using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamage
{
    float hp;
    public float maxHP = 100f;
    public float HP
    {
        get => hp;
        set
        {
            hp = value;
            OnHealthChanged?.Invoke();
            if (hp <= 0)
            {
                Die();
            }
        }
    }


    public Action OnHealthChanged;  // 플레이어의 체력감소 델리게이트
    public bool isInvincible = false;  // 무적 체크
    float invincibilityDuration = 3.0f; // 무적 시간

    ParrySystem parrySystem;

    void Start()
    {
        parrySystem = GetComponent<ParrySystem>();
        HP = maxHP;
        
    }

    public void TakeDamage(float amount)
    {
        if (isInvincible)
        {
            return;
        }

        if (parrySystem.isParrying)
        {
            // 패링 중이면 무적 처리
            parrySystem.StopParry(true);
            StartCoroutine(HandleInvincibility());
        }
        else
        {
            StartCoroutine(HandleDamage(amount));
        }
    }

    /// <summary>
    /// 데미지를 받는 코루틴(연속적으로 들어오지 않게 조절)
    /// </summary>
    /// <param name="amount">데미지 계수</param>
    /// <returns></returns>
    private IEnumerator HandleDamage(float amount)
    {
        isInvincible = true;
        HP -= amount;
        Debug.Log($"Player : {HP}");
        if (HP <= 0)
        {
            Die();
        }

        // 무적 시간 동안 대기
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    /// <summary>
    /// 플레이어 무적 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleInvincibility()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    private void Die()
    {
        // 플레이어 사망 처리 로직
        Debug.Log("");
    }
}