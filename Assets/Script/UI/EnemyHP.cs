using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHP : MonoBehaviour
{
    Enemy enemy;
    Slider playerHP;
    TextMeshProUGUI currentHP;
    TextMeshProUGUI maxHP;

    private void Awake()
    {
        enemy = GameManager.Instance.Enemy;
        playerHP = GetComponent<Slider>();

        Transform child = transform.GetChild(2);
        currentHP = child.GetComponent<TextMeshProUGUI>();

        child = transform.GetChild(4);
        maxHP = child.GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        enemy.OnHealthChanged += UpdateUI;
        UpdateUI();
    }

    private void UpdateUI()
    {
        playerHP.maxValue = enemy.maxHP;
        playerHP.value = enemy.HP;
        currentHP.text = enemy.HP.ToString("F0");
        maxHP.text = enemy.maxHP.ToString("F0");
    }
}
