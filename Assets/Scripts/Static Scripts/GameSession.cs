using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession gs { get; private set; }

    [SerializeField] public PlayerStats demoPlayerStats;
    [SerializeField] public EnemyStats demoEnemyStats;

    [HideInInspector] public PlayerState playerMember;
    [HideInInspector] public EnemyState enemyMember;

    public int currentGold { get; private set; } = 0;

    void Awake()
    {
        if (gs != null)
        {
            Destroy(gameObject);
            return;
        }

        gs = this;
        DontDestroyOnLoad(gameObject);

        InitializePlayer();
        InitializeEnemy();
    }

    private void InitializePlayer()
    {
        if (playerMember == null)
        {
            playerMember = new PlayerState(demoPlayerStats);
            playerMember.currentHealth = demoPlayerStats.baseMaxHealth;
            playerMember.currentMana = 3;
            playerMember.initializePushTurnIcons();
        }
    }

    public void InitializeEnemy()
    {
        if (enemyMember == null)
        {
            enemyMember = new EnemyState(demoEnemyStats);
            enemyMember.currentHealth = demoEnemyStats.baseMaxHealth;
            enemyMember.initializePushTurnIcons();
        }
    }

    public void SaveGame()
    {
        // Save system disabled for now
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        Debug.Log($"Gold added: {amount}. Total gold: {currentGold}");
    }

}
