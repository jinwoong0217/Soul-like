using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHP : MonoBehaviour
{
    Enemy enemy;
    Slider hpSlider;
    TextMeshProUGUI currentHp;
    TextMeshProUGUI maxHp;

    private void Awake()
    {
        enemy = GameManager.Instance.Enemy;
        hpSlider = GetComponent<Slider>();

        Transform child = transform.GetChild(2);
        currentHp = child.GetComponent<TextMeshProUGUI>();
        

        child = transform.GetChild(4);
        maxHp = child.GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        UpdateHPUI();
    }

    public void UpdateHPUI()
    {
        hpSlider.value = enemy.HP / enemy.maxHp;
        currentHp.text = Mathf.Ceil(enemy.HP).ToString();
        maxHp.text = Mathf.Ceil(enemy.maxHp).ToString();
    }
}
