using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using System;

public class Enemy : MonoBehaviour , IDamage
{
    // 애니메이터 해시 값
    readonly int SkillTree = Animator.StringToHash("SkillTree"); 
    readonly int OnSkill_Hash = Animator.StringToHash("OnSkill"); 
    readonly int See_Hash = Animator.StringToHash("See"); 
    readonly int Chase_Hash = Animator.StringToHash("Chase"); 
    readonly int ReadyAttack_Hash = Animator.StringToHash("ReadyAttack");  

    // 이동 속도
    public float chaseSpeed = 5.0f;

    // 체력
    public float hp = 100.0f;
    float maxHp = 100.0f;

    // 시야 설정
    public float sightAngle = 90.0f;

    // 컴포넌트
    Animator animator;
    NavMeshAgent agent;

    // 이벤트 및 콜백
    public Action<Enemy> onDie;
    Action onUpdate;

    // 타겟
    Player target;

    bool isInvincible = false;
    float invincibilityDuration = 1.0f; // 무적 시간

    Enemy_IronMace weapon;

    // 적 상태
    enum EnemyState
    {
        Idle,
        Find,
        Fight,
        Dead
    }
    EnemyState state = EnemyState.Idle;

    // 체력 프로퍼티
    public float HP
    {
        get => hp;
        set
        {
            hp = value;
            if (hp <= 0)
            {
                State = EnemyState.Dead;
            }
        }
    }

    // 상태 프로퍼티
    EnemyState State
    {
        get => state;
        set
        {
            state = value;
            switch (state)
            {
                case EnemyState.Idle:
                    onUpdate = UpdateIdle;
                    break;
                case EnemyState.Find:
                    onUpdate = UpdateFind;
                    break;
                case EnemyState.Fight:
                    onUpdate = UpdateFight;
                    break;
                case EnemyState.Dead:
                    onUpdate = UpdateDead;
                    break;
            }
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        weapon = FindAnyObjectByType<Enemy_IronMace>();
        target = GameManager.Instance.Player;
        HP = maxHp;

        onUpdate = UpdateIdle;
    }

    private void FixedUpdate()
    {
        onUpdate();
    }

    // Idle 상태 업데이트
    void UpdateIdle()
    {
        if(FindPlayer())
        {
            State = EnemyState.Find;
        }    
    }

    /// <summary>
    /// 타겟을 찾는 함수
    /// </summary>
    /// <returns>시야각에 들어오면 true 아니면 false</returns>
    bool FindPlayer()
    {
        Vector3 findPlayer = (target.transform.position - transform.position).normalized;
        float insightPlayer = Vector3.Angle(transform.forward, findPlayer);

        if(insightPlayer < sightAngle)
        {
            return true;
        }
        return false;
    }

    // Find상태 업데이트
    void UpdateFind()
    {
       if(FindPlayer())
        {
            animator.SetTrigger(See_Hash);
            agent.SetDestination(target.transform.position);
            agent.speed = chaseSpeed;
            if(Vector3.SqrMagnitude(target.transform.position - transform.position) <= 5f * 5f)
            {
                animator.SetTrigger(Chase_Hash);
                State = EnemyState.Fight;
            }
        }
        else
        {
            State = EnemyState.Idle;
        }
    }

    // Fight 상태 업데이트
    void UpdateFight()
    {
        if (Vector3.SqrMagnitude(target.transform.position - transform.position) > 2f * 2f)
        {
            animator.SetTrigger(ReadyAttack_Hash);
            agent.SetDestination(target.transform.position);
        }
        else
        {
            if (animator.GetBool(OnSkill_Hash) == false)
            {
                // OnSkill_Hash가 false에서 true로 변경될 때만 SkillTree 값을 설정
                animator.SetBool(OnSkill_Hash, true);
                animator.SetFloat(SkillTree, UnityEngine.Random.Range(0, 3));
            }
            agent.isStopped = true;
        }

        if (animator.GetBool(OnSkill_Hash) == false)
        {
            agent.isStopped = false;
            State = EnemyState.Find;
        }
        else
        {
            if (SkillUseFinished())
            {
                animator.SetBool(OnSkill_Hash, false);
            }
        }
    }


    /// <summary>
    /// 스킬사용이 끝났는지 체크하는 함수
    /// </summary>
    /// <returns>끝났으면 true 아니면 false</returns>
    bool SkillUseFinished()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Skill Blend Tree") && stateInfo.normalizedTime >= 1.0f)
        {
            return true;
        }

        return false;
    }

    // Dead 상태 업데이트
    void UpdateDead()
    {
        onDie?.Invoke(this);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 인터페이스를 이용한 함수
    /// </summary>
    /// <param name="amount">데미지</param>
    public void TakeDamage(float amount)
    {
        if (isInvincible) return;

        StartCoroutine(HandleDamage(amount));
    }

    /// <summary>
    /// 적이 데미지를 받는 코루틴(연속된 데미지 방지)
    /// </summary>
    /// <param name="amount">데미지</param>
    /// <returns></returns>
    private IEnumerator HandleDamage(float amount)
    {
        isInvincible = true;
        HP -= amount;

        if (HP <= 0)
        {
            Die();
        }

        // 무적 시간 동안 대기
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    private void Die()
    {
        State = EnemyState.Dead;
    }

    /// <summary>
    /// 플레이어의 패링을 체크하는 함수( 애니메이터 이벤트함수 )
    /// </summary>
    private void CheckParry()
    {
        bool playerparrySuccess = target.GetComponent<ParrySystem>().IsEnemyAttack(weapon.damaged);
        if (playerparrySuccess)
        {
            animator.SetTrigger("GetParry");
            animator.SetBool(OnSkill_Hash, false);
            agent.isStopped = false;
            State = EnemyState.Find;
        }
    }
}