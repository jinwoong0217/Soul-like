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
    public bool canMove = true;

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
        playerInputActions.Player.Parring.canceled += OnParryCanceled;
        playerInputActions.Player.Run.started += OnSprint;
        playerInputActions.Player.Run.canceled += OnSprintCanceled;
    }

    private void OnDisable()
    {
        playerInputActions.Player.Run.canceled -= OnSprintCanceled;
        playerInputActions.Player.Run.started -= OnSprint;
        playerInputActions.Player.Parring.canceled -= OnParryCanceled;
        playerInputActions.Player.Parring.started -= OnParry;
        playerInputActions.Player.Attack.performed -= OnAttack;
        playerInputActions.Player.Move.canceled -= OnMove;
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Disable();
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            Vector3 move = transform.TransformDirection(dir);
            characterController.Move(Time.deltaTime * move * speed);
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        dir = new Vector3(input.x, 0, input.y);

        if (context.performed && canMove)
        {
            if (input != Vector2.zero)
            {
                animator.SetBool("isMove", true);
                animator.SetFloat("inputX", input.x);
                animator.SetFloat("inputY", input.y);

                if (isSprinting)
                {
                    animator.SetFloat("sprintInputX", input.x);
                    animator.SetFloat("sprintInputY", input.y);
                    animator.SetTrigger(Run_Hash);
                }
            }
            else
            {
                animator.SetBool("isMove", false);
            }
        }
        else if (context.canceled)
        {
            dir = Vector3.zero;
            animator.SetBool("isMove", false);
            animator.SetFloat("inputX", 0);
            animator.SetFloat("inputY", 0);

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
        if (canMove) 
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    IEnumerator AttackCoroutine()
    {
        canMove = false; 
        animator.SetBool("isMove", false);
        animator.SetTrigger(Attack_Hash);
        playerInputActions.Disable();

        yield return new WaitForSeconds(1f);
        playerInputActions.Enable();
        canMove = true; 
    }

    private void OnParry(InputAction.CallbackContext context)
    {
        if (canMove) 
        {
            parrySystem.StartParry();
            canMove = false;
            dir = Vector3.zero;
            animator.SetBool("isMove", false);
            animator.SetFloat("inputX", 0);
            animator.SetFloat("inputY", 0);
        }
    }

    private void OnParryCanceled(InputAction.CallbackContext context)
    {
        if (parrySystem.isParrying)
        {
            parrySystem.StopParry(false);
            canMove = true;
        }
    }


}
