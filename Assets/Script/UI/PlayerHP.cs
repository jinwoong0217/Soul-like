using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    //컴포넌트
    Player player;
    Slider playerHP;
    TextMeshProUGUI currentHP;
    TextMeshProUGUI maxHP;

    private void Awake()
    {
        player = GameManager.Instance.Player;
        playerHP = GetComponent<Slider>();

        Transform child = transform.GetChild(2);
        currentHP = child.GetComponent<TextMeshProUGUI>();

        child = transform.GetChild(4);
        maxHP = child.GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        player.OnHealthChanged += UpdateUI;  // 플레이어의 OnHealthChanged 델리게이트에 함수 연결
        UpdateUI();
    }

    private void UpdateUI()
    {
        playerHP.maxValue = player.maxHP;
        playerHP.value = player.HP;
        currentHP.text = player.HP.ToString("F0");
        maxHP.text = player.maxHP.ToString("F0");
    }
}