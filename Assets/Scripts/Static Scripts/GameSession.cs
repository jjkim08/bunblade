using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession gs { get; private set; }

    [HideInInspector] public PlayerState playerMember;
    [HideInInspector] public EnemyState enemyMember;
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

        InitializePlayer();
    }

    private void InitializePlayer()
    {
        if (playerMember == null)
        {
            playerMember = new PlayerState(null);

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