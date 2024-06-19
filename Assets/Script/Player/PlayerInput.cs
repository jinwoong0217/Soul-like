using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [Header("이동속도")]
    public float speed = 5.0f;
    public float sprintSpeed = 8.0f;
    float defaultSpeed;  // 이동속도 저장용
    bool isSprinting = false;

    bool isParrying = false;
    float maxParryDuration = 3.0f;  // 패링을 유지하는 시간


    Vector3 dir = Vector3.zero;

    Player player;
    ParrySystem parrySystem;
    Animator animator;
    PlayerInputActions playerInputActions;
    CharacterController characterController;

    readonly int Attack_Hash = Animator.StringToHash("Attack");
    readonly int Run_Hash = Animator.StringToHash("Run");
    readonly int StopRun_Hash = Animator.StringToHash("StopRun");

    private void Awake()
    {
        defaultSpeed = speed;

        player = GameManager.Instance.Player;
        animator = GetComponent<Animator>();
        parrySystem = GetComponent<ParrySystem>();
        playerInputActions = new PlayerInputActions();
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        playerInputActions.Player.Enable();
        playerInputActions.Player.Move.performed += OnMove;
        playerInputActions.Player.Move.canceled += OnMove;
        playerInputActions.Player.Attack.performed += OnAttack;
        playerInputActions.Player.Parring.started += OnParry;
        playerInputActions.Player.Parring.canceled += OnParryFalse;
        playerInputActions.Player.Run.started += OnSprint;
        playerInputActions.Player.Run.canceled += OnSprintCanceled;
    }

    private void OnDisable()
    {
        playerInputActions.Player.Run.canceled -= OnSprintCanceled;
        playerInputActions.Player.Run.started -= OnSprint;
        playerInputActions.Player.Parring.canceled -= OnParryFalse;
        playerInputActions.Player.Parring.started -= OnParry;
        playerInputActions.Player.Attack.performed -= OnAttack;
        playerInputActions.Player.Move.canceled -= OnMove;
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Disable();
    }

    private void FixedUpdate()
    {
        Vector3 move = transform.TransformDirection(dir);
        characterController.Move(Time.deltaTime * move * speed);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        dir = new Vector3(input.x, 0, input.y);

        if (context.performed)
        {
            animator.SetBool("isMove", true);
            animator.SetFloat("inputX", input.x);
            animator.SetFloat("inputY", input.y);

            // 스프린트 상태 체크 및 애니메이션 업데이트
            if (isSprinting)
            {
                animator.SetFloat("sprintInputX", input.x);
                animator.SetFloat("sprintInputY", input.y);
                animator.SetTrigger(Run_Hash);
            }
        }
        else if (context.canceled)
        {
            dir = Vector3.zero;
            animator.SetBool("isMove", false);
            animator.SetFloat("inputX", 0);
            animator.SetFloat("inputY", 0);

            // 이동 중지 시 스프린트 상태도 초기화
            if (isSprinting)
            {
                isSprinting = false;
                speed = defaultSpeed;
                animator.SetTrigger(StopRun_Hash);
            }
        }
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started && dir != Vector3.zero)
        {
            isSprinting = true;
            speed = sprintSpeed;
            animator.SetTrigger(Run_Hash);

            animator.SetFloat("sprintInputX", dir.x);
            animator.SetFloat("sprintInputY", dir.z);
        }
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            isSprinting = false;
            speed = defaultSpeed;
            animator.SetTrigger(StopRun_Hash);
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        StartCoroutine(AttackCoroutine());
    }

    IEnumerator AttackCoroutine()
    {
        animator.SetBool("isMove", false);
        animator.SetTrigger(Attack_Hash);
        playerInputActions.Disable();

        yield return new WaitForSeconds(1f);
        playerInputActions.Enable();
    }

    private void OnParry(InputAction.CallbackContext context)
    {
        parrySystem.StartParry();
    }

    private void OnParryFalse(InputAction.CallbackContext context)
    {
        if (parrySystem.isParrying)
        {
            parrySystem.StopParry(false);
        }
    }
}
