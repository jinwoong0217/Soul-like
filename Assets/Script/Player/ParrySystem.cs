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

    private void Awake()
    {
        player = GameManager.Instance.Player;
        animator = GetComponent<Animator>();
        playerInput = player.GetComponent<PlayerInput>();
    }

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

    public void StopParry(bool successful)
    {
        isParrying = false;
        playerInput.canMove = true; 
        if (successful)
        {
            animator.SetTrigger(ParryTrue_Hash);
        }
        else
        {
            animator.SetTrigger(ParryFalse_Hash);
        }
    }

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

    public void HandleEnemyAttack(float damage)
    {
        if (isParrying)
        {
            StopParry(true);
        }
        else
        {
            player.TakeDamage(damage);
        }
    }
}

