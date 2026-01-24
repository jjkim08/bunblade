using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPFillEnemy : MonoBehaviour
{
    public Image HPBar;
    private EnemyState currentEnemy;
    public EnemyAction enemyAction;

    void Start()
    {
        if (GameSession.gs != null && GameSession.gs.enemyMember != null)
        {
            bindEnemy(GameSession.gs.enemyMember);
        }

        if (enemyAction != null)
        {
            enemyAction.onEnemyInitialized += bindEnemy;
        }
    }

    void OnDisable()
    {
        if (currentEnemy != null)
        {
            currentEnemy.onHealthChanged -= updateBar;
        }

        if (enemyAction != null)
        {
            enemyAction.onEnemyInitialized -= bindEnemy;
        }
    }

    void updateBar(float healthPercentage)
    {
        if (HPBar != null) HPBar.fillAmount = healthPercentage;
    }

    private void bindEnemy(EnemyState enemy)
    {
        if (currentEnemy != null)
        {
            currentEnemy.onHealthChanged -= updateBar;
        }

        currentEnemy = enemy;

        if (currentEnemy != null)
        {
            currentEnemy.onHealthChanged += updateBar;
            float healthPct = currentEnemy.currentHealth / Mathf.Max(1f, currentEnemy.enemyStats.baseMaxHealth);
            updateBar(healthPct);
        }
    }
}
