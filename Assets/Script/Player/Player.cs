using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float maxHealth = 100.0f;
    float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{currentHealth}");
        if (currentHealth <= 0)
        {
            Debug.Log("Player Dead");
            
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}
