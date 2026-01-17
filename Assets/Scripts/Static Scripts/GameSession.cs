using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession gs { get; private set; }

    [Header("1v1 Battle Configuration")]
    public PlayerStats playerStats;
    public EnemyStats enemyStats;

    [HideInInspector] public PlayerState playerMember;
    [HideInInspector] public EnemyState enemyMember;

    void Awake()
    {
        // Singleton pattern
        if (gs != null)
        {
            Destroy(gameObject);
            return;
        }

        gs = this;
        DontDestroyOnLoad(gameObject);

        initializeCombatants();
    }

    private void initializeCombatants()
    {
        if (playerStats == null || enemyStats == null)
        {
            return;
        }

        playerMember = new PlayerState(playerStats);
        playerMember.currentHealth = playerStats.baseMaxHealth; // use maximum health
        playerMember.currentMana = 0;
        playerMember.initializePushTurnIcons();

        enemyMember = new EnemyState(enemyStats);
        enemyMember.currentHealth = enemyStats.baseMaxHealth;
        enemyMember.initializePushTurnIcons();
    }


}