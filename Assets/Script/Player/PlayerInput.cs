using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public float speed = 5.0f;
    Vector3 dir = Vector3.zero;
    PlayerInputActions playerInputActions;
    Animator animator;
    CharacterController characterController;
    readonly int Attack_Hash = Animator.StringToHash("Attack");

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        playerInputActions.Player.Enable();
        playerInputActions.Player.Move.performed += OnMove;
        playerInputActions.Player.Move.canceled += OnMove;
        playerInputActions.Player.Look.performed += OnLook;
        playerInputActions.Player.Look.canceled += OnLook;
        playerInputActions.Player.Attack.performed += OnAttack;
    }

    private void OnDisable()
    { 
        playerInputActions.Player.Attack.performed -= OnAttack;
        playerInputActions.Player.Look.canceled -= OnLook;
        playerInputActions.Player.Look.performed -= OnLook;
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
        if(context.performed)
        {
            animator.SetBool("isMove", true);
            Vector3 input = context.ReadValue<Vector2>();
            dir = new Vector3(input.x, 0, input.y);

            animator.SetFloat("inputX", input.x);
            animator.SetFloat("inputY", input.y);
        }
        else if(context.canceled)
        {
            animator.SetBool("isMove", false);
            dir = Vector3.zero;
        }
    }


    private void OnLook(InputAction.CallbackContext context)
    {
        
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    IEnumerator AttackCoroutine()
    {
        animator.SetBool("isMove", false);
        animator.SetTrigger(Attack_Hash);
        playerInputActions.Disable();

        yield return new WaitForSeconds(1f);
        playerInputActions.Enable();
        animator.SetBool("isMove", true);
    }

}
