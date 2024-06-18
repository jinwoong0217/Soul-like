using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public float walkSpeed = 2.0f;
    public float ChaseSpeed = 5.0f;

    float hp = 100.0f;
    float maxHp = 100.0f;

    public float sightAngle = 90.0f;
    public float sightRange = 20.0f;

    float attackElapsed = 0;

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

    public Action<Enemy> onDie;
    Action onUpdate = null;

    Transform chaseTarget = null;
    Player target = null;
    NavMeshAgent agent;

    enum EnemyState
    {
        Idle,
        Find,
        Fight,
        Dead
    }
    EnemyState State = EnemyState.Idle;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
    }
}
