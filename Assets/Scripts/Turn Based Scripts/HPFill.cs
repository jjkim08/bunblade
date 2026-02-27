using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// simple program meant to display the health of the player
public class HPFill : MonoBehaviour
{
    public Image HPBar;
    private PlayerState currentPlayer;

    void Start()
    {

        if (GameSession.gs != null && GameSession.gs.playerMember != null)
        {
            currentPlayer = GameSession.gs.playerMember;
            currentPlayer.onHealthChanged += updateBar;

            float healthPct = currentPlayer.currentHealth / Mathf.Max(1f, currentPlayer.playerStats.baseMaxHealth);
            updateBar(healthPct);
        }
    }

    void OnDisable()
    {
        if (currentPlayer != null)
        {
            currentPlayer.onHealthChanged -= updateBar;
        }
    }

    void updateBar(float healthPercentage)
    {
        if (HPBar != null) HPBar.fillAmount = healthPercentage;
    }
}
