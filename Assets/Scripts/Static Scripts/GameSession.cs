using UnityEngine;

/*
// TESTING VERSION - Commented out for now
public class GameSessionTesting : MonoBehaviour
{
    public static GameSession gs { get; private set; }

    [HideInInspector] public PlayerState playerMember;
    [HideInInspector] public EnemyState enemyMember;

    [SerializeField] private EnemyStats demoEnemyForTesting;
    [SerializeField] private PlayerStats demoPlayerStats;

    private SaveSystem saveSystem;

    void Awake()
    {
        if (gs != null)
        {
            Destroy(gameObject);
            return;
        }

        gs = this;
        DontDestroyOnLoad(gameObject);

        saveSystem = GetComponent<SaveSystem>();
        if (saveSystem == null)
            saveSystem = gameObject.AddComponent<SaveSystem>();

        // TODO: Remove this line when done testing
        InitializePlayer();

        // TODO: Remove this line when done testing
        if (demoEnemyForTesting != null)
            InitializeBattle(demoEnemyForTesting);
    }

    private void InitializePlayer()
    {
        if (playerMember == null)
        {
            playerMember = new PlayerState(demoPlayerStats);

            PlayerSaveData saved = saveSystem.LoadPlayer();
            if (saved != null)
            {
                playerMember.currentHealth = saved.currentHealth;
            }
            else
            {
                // there is no save, todo: laterrrrr
            }

            playerMember.currentMana = 3;
            playerMember.initializePushTurnIcons();
        }
    }

    public void InitializeBattle(EnemyStats enemyToFight)
    {
        if (enemyToFight == null)
            return;

        enemyMember = new EnemyState(enemyToFight);
        enemyMember.currentHealth = enemyToFight.baseMaxHealth;
        enemyMember.initializePushTurnIcons();
    }

    public void SaveGame()
    {
        saveSystem.SavePlayer(playerMember);
    }
}
*/

// ORIGINAL VERSION - Restored
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