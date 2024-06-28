using UnityEngine;
using System.Collections;

public class ParrySystem : MonoBehaviour
{
    public bool isParrying = false;
    float maxParryDuration = 2.0f; // 패링 지속 시간

    Animator animator;
    Coroutine parryCoroutine;
    Player player;
    PlayerInput playerInput;

    readonly int Parry_Hash = Animator.StringToHash("Parry");
    readonly int ParryTrue_Hash = Animator.StringToHash("ParryTrue");
    readonly int ParryFalse_Hash = Animator.StringToHash("ParryFalse");

    private void Start()
    {
        player = GameManager.Instance.Player;
        animator = GetComponent<Animator>();
        playerInput = player.GetComponent<PlayerInput>();
    }

    /// <summary>
    /// 패링을 시작하고나서 적용되는 함수
    /// </summary>
    public void StartParry()
    {
        if (!isParrying)
        {
            isParrying = true;
            animator.SetTrigger(Parry_Hash);
            playerInput.canMove = false;
            if (parryCoroutine != null)
            {
                StopCoroutine(parryCoroutine);
            }
            parryCoroutine = StartCoroutine(ParryCoroutine());
        }
    }

    /// <summary>
    /// 패링이 끝나거나 패링이 실패할때의 함수
    /// </summary>
    /// <param name="successful">패링 성공 여부</param>
    public void StopParry(bool successful)
    {
        if (!isParrying) return; // 이미 패링이 종료된 경우 중복 호출 방지
        isParrying = false;
        playerInput.canMove = true;
        if (parryCoroutine != null)
        {
            StopCoroutine(parryCoroutine);
            parryCoroutine = null;
        }
        if (successful)
        {
            animator.SetTrigger(ParryTrue_Hash);
        }
        else
        {
            animator.SetTrigger(ParryFalse_Hash);
        }
    }

    /// <summary>
    /// 패링 유지 시간 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator ParryCoroutine()
    {
        float startTime = Time.time;

        while (isParrying)
        {
            if (Time.time - startTime >= maxParryDuration)
            {
                StopParry(false);
                yield break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// 외부에서 적의 공격을 체크하는 함수
    /// </summary>
    /// <param name="damage">적의 데미지</param>
    /// <returns></returns>
    public bool IsEnemyAttack(float damage)
    {
        if (isParrying)
        {
            StopParry(true);
            return true;
        }
        else
        {
            player.TakeDamage(damage);
            return false;
        }
    }
}