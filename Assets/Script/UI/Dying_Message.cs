using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Dying_Message : MonoBehaviour
{
    float FadeInColor = 2f;
    TextMeshProUGUI dyingText;
    Player player;

    private void Awake()
    {
        player = GameManager.Instance.Player;
        dyingText = GetComponentInChildren<TextMeshProUGUI>();

        //gameObject.SetActive(false);
        player.OnDie += OnPlayerDead;
    }

    void OnPlayerDead()
    {
        dyingText.gameObject.SetActive(true);
        StartCoroutine(FadeInText());
    }

    IEnumerator FadeInText()
    {
        
        dyingText.color = new Color(dyingText.color.r, dyingText.color.g, dyingText.color.b, 0f); 

        float elapsedTime = 0f;
        while (elapsedTime < FadeInColor)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / FadeInColor);
            dyingText.color = new Color(dyingText.color.r, dyingText.color.g, dyingText.color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        dyingText.color = new Color(dyingText.color.r, dyingText.color.g, dyingText.color.b, 1f);
    }
}
