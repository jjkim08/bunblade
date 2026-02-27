using UnityEngine;
using TMPro;

// quick class to display the enemy texts
public class EnemyStatusDisplay : MonoBehaviour
{
    public TMP_Text statusText;
    public EnemyAction enemyAction;

    private EnemyState currentEnemy;

    void Start()
    {
        if (GameSession.gs != null && GameSession.gs.enemyMember != null)
        {
            BindEnemy(GameSession.gs.enemyMember);
        }

        if (enemyAction != null)
        {
            enemyAction.onEnemyInitialized += BindEnemy;
        }
    }

    void OnDisable()
    {
        UnbindEnemy();

        if (enemyAction != null)
        {
            enemyAction.onEnemyInitialized -= BindEnemy;
        }
    }

    
    private void BindEnemy(EnemyState enemy) // used to accurately display the right enemy
    {
        UnbindEnemy();

        currentEnemy = enemy;

        if (currentEnemy != null)
        {

            UpdateStatusDisplay();
        }
    }

    
    private void UnbindEnemy()
    {
        if (currentEnemy != null)
        {

            currentEnemy = null;
        }
    }


    
    public void UpdateStatusDisplay()
    {
        if (currentEnemy == null || statusText == null)
        {
            if (statusText != null) statusText.text = "";
            return;
        }

        string statusDisplay = "";


        int fullIcons = currentEnemy.pushTurnHalves / 2;
        int halfIcons = currentEnemy.pushTurnHalves % 2;
        statusDisplay += $"Turn Icons: {fullIcons}";
        if (halfIcons > 0)
        {
            statusDisplay += ".5";
        }
        statusDisplay += "\n";


        if (currentEnemy.currentBurnStacks > 0)
        {
            statusDisplay += $"Burn x{currentEnemy.currentBurnStacks}\n";
        }


        if (currentEnemy.currentSlowStacks > 0)
        {
            statusDisplay += $"Slow x{currentEnemy.currentSlowStacks}\n";
        }


        if (currentEnemy.currentDamageDebuffStacks > 0)
        {
            statusDisplay += $"Weaken x{currentEnemy.currentDamageDebuffStacks}\n";
        }

        statusText.text = statusDisplay;
    }


    void Update()
    {
        if (currentEnemy != null)
        {
            UpdateStatusDisplay();
        }
    }
}
