using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public float speed = 5.0f;
    public float sprintSpeed = 8.0f;
    float defaultSpeed;

    Vector3 dir = Vector3.zero;

    PlayerInputActions playerInputActions;
    Animator animator;
    CharacterController characterController;
    readonly int Attack_Hash = Animator.StringToHash("Attack");
    readonly int Parry_Hash = Animator.StringToHash("Parry");
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
        playerInputActions.Player.Run.started += OnSprint;
        playerInputActions.Player.Run.canceled += OnSprintCanceled;
    }

    private void OnDisable()
    {
        playerInputActions.Player.Run.canceled -= OnSprintCanceled;
        playerInputActions.Player.Run.started -= OnSprint;
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
        StartCoroutine(ParryCoroutine());
    }

    IEnumerator ParryCoroutine()
    {
        animator.SetTrigger(Parry_Hash);
        playerInputActions.Disable();
        yield return new WaitForSeconds(1);
        if (EnemyAttack())
        {
            animator.SetBool("isParry", true);
        }
        else
        {
            animator.SetBool("isParry", false);
        }
        playerInputActions.Enable();
    }

    bool EnemyAttack()
    {
        return true;
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        animator.SetBool(Sprint_Hash, true);
        speed = sprintSpeed;
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        animator.SetBool(Sprint_Hash, false);
        speed = defaultSpeed;
    }
}
