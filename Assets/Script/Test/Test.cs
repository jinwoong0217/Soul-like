using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{
    Testinput input;
    Player player;

    private void Awake()
    {
        player = GameManager.Instance.Player;
        input = new();
    }

    private void OnEnable()
    {
        input.Test.Enable();
        input.Test.Test1.performed += Test1;
    }

    private void Test1(InputAction.CallbackContext context)
    {
        player.HP = 5;
    }
}
