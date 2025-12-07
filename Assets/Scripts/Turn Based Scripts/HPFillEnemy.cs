using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPFillEnemy : MonoBehaviour
{
    public Image HPBar;
    public EnemyAction enemyAction;
    void OnEnable()
    {
        // enemyAction.currentEnemy.onHealthChanged += updateBar;
    }

    void OnDisable()
    {
        // enemyAction.currentEnemy.onHealthChanged -= updateBar;
    }

    void updateBar(float healthPercentage) {
        HPBar.fillAmount = healthPercentage;
    }
}
