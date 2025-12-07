using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPFill : MonoBehaviour
{
    public Image HPBar;
    public PlayerAction playerAction;

    void OnEnable()
    {
        // playerAction.currentPlayer.onHealthChanged += updateBar;
    }

    void OnDisable()
    {
        // playerAction.currentPlayer.onHealthChanged -= updateBar;
    }

    void updateBar(float healthPercentage) {
        HPBar.fillAmount = healthPercentage;
    }
}
