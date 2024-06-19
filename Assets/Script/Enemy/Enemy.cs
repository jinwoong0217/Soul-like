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
    public float walkSpeed = 2.0f;
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
    Transform chaseTarget = null;
    Player target = null;

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
    }

    private void Update()
    {
        onUpdate?.Invoke();
    }

    // Idle 상태 업데이트
    void UpdateIdle()
    {
        // 기본 애니메이션 상태 유지
        animator.SetBool("IsIdle", true);

        // 플레이어 탐색
        if (FindPlayer())
        {
            State = EnemyState.Find;
        }
    }

    void UpdateFind()
    {
        // 플레이어를 발견하면 See_Hash 애니메이션 실행
        animator.SetTrigger(See_Hash);

        // 플레이어를 따라가기 시작
        agent.speed = chaseSpeed;
        agent.SetDestination(chaseTarget.position);

        // 플레이어와 일정 거리 이내에 도달하면 Chase_Hash 애니메이션 실행
        float distanceSquared = (transform.position - chaseTarget.position).sqrMagnitude;
        float thresholdDistanceSquared = 5.0f * 5.0f; // 5.0f 거리에 대한 제곱 값

        if (distanceSquared < thresholdDistanceSquared)
        {
            // NavMeshAgent를 멈추고 애니메이션만 진행
            agent.isStopped = true;
            animator.SetTrigger(Chase_Hash);
            Invoke("TransitionToFight", 1.0f); // 1초 후 Fight 상태로 전환
        }
    }

    // Fight 상태 업데이트
    void UpdateFight()
    {
        // ReadyAttack_Hash 애니메이션 실행
        animator.SetTrigger(ReadyAttack_Hash);

        // 플레이어를 계속 따라감
        agent.SetDestination(chaseTarget.position);

        // 랜덤 스킬 사용
        if (attackElapsed <= 0)
        {
            int skillIndex = UnityEngine.Random.Range(0, 4); // 0, 1, 2, 3 중 하나 선택
            animator.SetInteger(SkillTree, skillIndex);
            animator.SetTrigger(OnSkill_Hash);
            attackElapsed = 3.0f; // 3초 후 다시 스킬 사용 가능
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
        // 시야 범위 내에 플레이어가 있는지 확인
        Collider[] hits = Physics.OverlapSphere(transform.position, sightRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Vector3 direction = (hit.transform.position - transform.position).normalized;
                if (Vector3.Angle(transform.forward, direction) < sightAngle / 2)
                {
                    chaseTarget = hit.transform;
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
        State = EnemyState.Fight;
    }
}

