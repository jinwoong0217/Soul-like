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

    bool isParrying = false;
    float maxParryDuration = 3.0f;  // 패링을 유지하는 시간

    Vector3 dir = Vector3.zero;

    Coroutine parryCoroutine;
    PlayerInputActions playerInputActions;
    Animator animator;
    CharacterController characterController;

    readonly int Attack_Hash = Animator.StringToHash("Attack");
    readonly int Parry_Hash = Animator.StringToHash("Parry");
    readonly int ParryTrue_Hash = Animator.StringToHash("ParryTrue");
    readonly int ParryFalse_Hash = Animator.StringToHash("ParryFalse");
    readonly int Sprint_Hash = Animator.StringToHash("Run");

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        defaultSpeed = speed;
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
        if (context.performed)
        {
            animator.SetBool("isMove", true);
            Vector3 input = context.ReadValue<Vector2>();
            dir = new Vector3(input.x, 0, input.y);

            animator.SetFloat("inputX", input.x);
            animator.SetFloat("inputY", input.y);
        }
        else if (context.canceled)
        {
            animator.SetBool("isMove", false);
            dir = Vector3.zero;
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
        if (!isParrying)
        {
            isParrying = true;
            animator.SetTrigger(Parry_Hash);
            if (parryCoroutine != null)
            {
                StopCoroutine(parryCoroutine);
            }
            parryCoroutine = StartCoroutine(ParryCoroutine());
        }
    }

    private void OnParryFalse(InputAction.CallbackContext context)
    {
        if (isParrying)
        {
            StopParry(false);
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

            if (EnemyAttack())
            {
                StopParry(true);
                yield break;
            }

            yield return null;
        }
    }

    private void StopParry(bool successful)
    {
        isParrying = false;
        if (successful)
        {
            animator.SetTrigger(ParryTrue_Hash);
        }
        else
        {
            animator.SetTrigger(ParryFalse_Hash);
        }
    }

    bool EnemyAttack()
    {
        return true;  // 임시
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            animator.SetTrigger(Sprint_Hash);
            speed = sprintSpeed;
        }
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            animator.ResetTrigger(Sprint_Hash);
            animator.SetTrigger("StopRun");
            speed = defaultSpeed;
        }
    }
}
