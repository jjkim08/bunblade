using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPFillEnemy : MonoBehaviour
{
    public Image HPBar;
    private EnemyState currentEnemy;

    void Start()
    {
        if (GameSession.gs != null && GameSession.gs.enemyMember != null)
        {
            currentEnemy = GameSession.gs.enemyMember;
            currentEnemy.onHealthChanged += updateBar;
            // Initialize bar to current health
            float healthPct = currentEnemy.currentHealth / Mathf.Max(1f, currentEnemy.enemyStats.baseMaxHealth);
            updateBar(healthPct);
        }
    }

    void OnDisable()
    {
        if (currentEnemy != null)
        {
            currentEnemy.onHealthChanged -= updateBar;
        }
    }

    void updateBar(float healthPercentage)
    {
        if (HPBar != null) HPBar.fillAmount = healthPercentage;
    }
}
