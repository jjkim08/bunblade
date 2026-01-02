using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession gs { get; private set; }

    [Header("1v1 Battle Configuration")]
    public PlayerStats playerStats;
    public EnemyStats enemyStats;
    public GlobalPlayerData globalData; // persistent data shared across scenes

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

        // Initialize 1v1 combatants
        InitializeCombatants();
    }

    private void InitializeCombatants()
    {
        if (playerStats == null || enemyStats == null)
        {
            Debug.LogError("GameSession requires both playerStats and enemyStats to be assigned!");
            return;
        }

        playerMember = new PlayerState(playerStats);
        // Import health from global data if available, otherwise use base max health
        float playerMaxHealth = playerStats.baseMaxHealth;
        if (globalData != null)
        {
            playerMaxHealth = globalData.maxHealth;
            globalData.currentHealth = Mathf.Clamp(globalData.currentHealth, 0, globalData.maxHealth);
            playerMember.currentHealth = globalData.currentHealth;
        }
        else
        {
            playerMember.currentHealth = playerMaxHealth;
        }
        playerMember.currentMana = 0; // Start with 0 mana
        playerMember.InitializePushTurnIcons(); // Initialize with 6 half-icons (3 full equivalents)

        enemyMember = new EnemyState(enemyStats);
        enemyMember.currentHealth = enemyStats.baseMaxHealth;
        enemyMember.InitializePushTurnIcons(); // Initialize with 6 half-icons (3 full equivalents)
    }

    public void SyncPlayerToGlobal()
    {
        if (globalData == null) return;
        float clamped = Mathf.Clamp(playerMember.currentHealth, 0, globalData.maxHealth);
        globalData.currentHealth = clamped;
        // Items and other globals can be synced here when added.
    }
}