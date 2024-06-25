using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using System;

public class Enemy : MonoBehaviour , IDamage
{
    // 애니메이터 해시 값
    readonly int SkillTree = Animator.StringToHash("SkillTree"); // 블렌드 트리 Float
    readonly int OnSkill_Hash = Animator.StringToHash("OnSkill"); // 스킬을 쓸 수 있는 트랜지션Bool
    readonly int See_Hash = Animator.StringToHash("See");  // 플레이어를 찾고 플레이어한테 걸어가는 Trigger 트랜지션
    readonly int Chase_Hash = Animator.StringToHash("Chase");  // 플레이어와 일정 거리 닿는 순간 배틀포즈 애니메이이션으로 바뀌는 Trigger트랜지션
    readonly int ReadyAttack_Hash = Animator.StringToHash("ReadyAttack");  // 배틀포즈에서 플레이어와 싸울 준비하는 Trigger트랜지션

    // 이동 속도
    public float chaseSpeed = 5.0f;

    // 체력
    public float hp = 100.0f;
    float maxHp = 100.0f;

    // 시야 설정
    public float sightAngle = 90.0f;


    float attackDamage = 3;


    // 컴포넌트
    Animator animator;
    NavMeshAgent agent;

    // 이벤트 및 콜백
    public Action<Enemy> onDie;
    Action onUpdate;
    public Action onAttack;  // 적이 스킬을 쓴다는 걸 알리는 델리게이트(플레이어와의 패링)

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
        onAttack += CheckParry;
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
                animator.SetFloat(SkillTree, UnityEngine.Random.Range(0, 4));
                onAttack.Invoke();
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

    public void TakeDamage(float amount)
    {
        if (isInvincible) return;

        StartCoroutine(HandleDamage(amount));
    }

    private IEnumerator HandleDamage(float amount)
    {
        isInvincible = true;
        HP -= amount;
        Debug.Log($"Enemy HP {HP}");

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

