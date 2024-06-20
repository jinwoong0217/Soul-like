using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using System;

public class Enemy : MonoBehaviour
{
    // 애니메이터 해시 값
    readonly int SkillTree = Animator.StringToHash("SkillTree"); // 블렌드 트리 
    readonly int OnSkill_Hash = Animator.StringToHash("OnSkill"); // 스킬을 쓸 수 있는 트랜지션
    readonly int See_Hash = Animator.StringToHash("See");  // 플레이어를 찾고 실행될 트랜지션
    readonly int Chase_Hash = Animator.StringToHash("Chase");  // 플레이어와 일정 거리 닿는 순간 배틀페이즈로 바뀌는 트랜지션
    readonly int ReadyAttack_Hash = Animator.StringToHash("ReadyAttack");  // 배틀페이즈에서 플레이어와 싸울 준비하는 트랜지션

    // 이동 속도
    public float chaseSpeed = 5.0f;

    // 체력
    float hp = 100.0f;
    float maxHp = 100.0f;

    // 시야 설정
    public float sightAngle = 90.0f;
    public float sightRange = 20.0f;

    // 공격 대기 시간
    float attackElapsed = 0;

    // 컴포넌트
    Animator animator;
    NavMeshAgent agent;

    // 이벤트 및 콜백
    public Action<Enemy> onDie;
    Action onUpdate = null;

    // 타겟
    Player target;

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
        // 플레이어 탐색
        if (FindPlayer())
        {
            State = EnemyState.Find;
        }
    }

    void UpdateFind()
    {
        // 플레이어를 발견하면 See_Hash 애니메이션 실행
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
        {
            animator.SetTrigger(See_Hash);
        }

        // 플레이어를 따라가기 시작
        agent.speed = chaseSpeed;
        agent.SetDestination(target.transform.position);

        // 플레이어와 일정 거리 이내에 도달하면 Chase_Hash 애니메이션 실행
        float distanceSquared = (transform.position - target.transform.position).sqrMagnitude;
        float thresholdDistanceSquared = 5.0f * 5.0f; // 5.0f 거리에 대한 제곱 값

        if (distanceSquared < thresholdDistanceSquared)
        {
            // NavMeshAgent를 멈추고 애니메이션만 진행
            agent.isStopped = true;
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("battle poze"))
            {
                animator.SetTrigger(Chase_Hash);
            }
            Invoke("TransitionToFight", 1.0f); // 1초 후 Fight 상태로 전환
        }
    }

    // Fight 상태 업데이트
    void UpdateFight()
    {
        // 타겟과의 거리 계산
        float distanceToTarget = (target.transform.position - transform.position).sqrMagnitude;
        float skillDistanceThreshold = 2f * 2f; // sqrMagnitude를 사용하므로 2f의 제곱값 사용

        // ReadyAttack 애니메이션이 실행 중인지 확인
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isReadyAttack = stateInfo.IsName("ReadyAttack");

        if (isReadyAttack && distanceToTarget <= skillDistanceThreshold)
        {
            // 스킬 사용 타이머가 0 이하일 때 스킬 사용
            if (attackElapsed <= 0)
            {
                int skillIndex = UnityEngine.Random.Range(0, 4); // 0, 1, 2, 3 중 하나 선택
                animator.SetFloat("SkillTree", skillIndex);
                animator.SetTrigger(OnSkill_Hash);
                attackElapsed = 3.0f; // 3초 후 다시 스킬 사용 가능

                // 스킬을 사용할 때 이동 멈춤
                agent.isStopped = true;
            }
        }
        else if (!isReadyAttack)
        {
            // ReadyAttack 애니메이션이 실행 중이 아니면 ReadyAttack 트리거 설정
            animator.SetTrigger(ReadyAttack_Hash);
        }

        // 스킬 사용 후 ReadyAttack 상태로 돌아감
        if (stateInfo.IsName("Skill") && stateInfo.normalizedTime >= 1.0f)
        {
            animator.SetTrigger(ReadyAttack_Hash); // ReadyAttack 상태로 전환
            agent.isStopped = false; // 이동 재개
        }

        // 플레이어를 계속 따라감
        if (!agent.isStopped)
        {
            agent.SetDestination(target.transform.position);
        }

        attackElapsed -= Time.deltaTime;
    }

    // Dead 상태 업데이트
    void UpdateDead()
    {
        // 죽었을 때 처리
        onDie?.Invoke(this);
        Destroy(gameObject);
    }

    // 플레이어 탐색 함수
    bool FindPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, sightRange, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") && !hit.isTrigger)
            {
                Vector3 direction = (hit.transform.position - transform.position).normalized;
                if (Vector3.Angle(transform.forward, direction) < sightAngle / 2)
                {
                    target.transform.position = hit.transform.position;
                    return true;
                }
            }
        }
        return false;
    }

    // Fight 상태로 전환하는 함수
    void TransitionToFight()
    {
        // NavMeshAgent를 다시 시작
        agent.isStopped = false;
        animator.SetTrigger(ReadyAttack_Hash); // ReadyAttack_Hash 트리거 설정
        State = EnemyState.Fight;
    }


}

