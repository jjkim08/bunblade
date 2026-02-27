using UnityEngine;
using System.Collections.Generic;


public class GameSession : MonoBehaviour
{
    public static GameSession gs { get; private set; }

    [SerializeField] public PlayerStats demoPlayerStats; // currently a demo, character select screen will be made if there is more time
    [SerializeField] public List<EnemyStats> enemyPool = new List<EnemyStats>();

    [HideInInspector] public PlayerState playerMember;
    [HideInInspector] public EnemyState enemyMember;

    public int currentGold { get; private set; } = 0;
    private int battlesCompleted = 0;

    // this is the central game session manager whcih handles the gameplay logic through starting the game and containing all important values

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
        playerMember = new PlayerState(demoPlayerStats);
        playerMember.currentHealth = demoPlayerStats.baseMaxHealth;
        playerMember.currentMana = 3;
        playerMember.initializePushTurnIcons();
    }

    // this calculates the buff that the enemy recieves after winning a battle to make the scaling fair
    public void InitializeEnemy()
    {
        if (enemyPool == null || enemyPool.Count == 0)
        {
            return;
        }


        EnemyStats selectedEnemy = enemyPool[Random.Range(0, enemyPool.Count)];


        EnemyStats buffedEnemy = ScriptableObject.CreateInstance<EnemyStats>();


        buffedEnemy.id = selectedEnemy.id;
        float buffMultiplier = 1f + (battlesCompleted * 0.5f);

        buffedEnemy.baseMaxHealth = Mathf.RoundToInt(selectedEnemy.baseMaxHealth * buffMultiplier);
        buffedEnemy.baseAttackDamage = Mathf.RoundToInt(selectedEnemy.baseAttackDamage * buffMultiplier);
        buffedEnemy.baseAbilityPower = Mathf.RoundToInt(selectedEnemy.baseAbilityPower * buffMultiplier);
        buffedEnemy.baseDefense = Mathf.RoundToInt(selectedEnemy.baseDefense * buffMultiplier);
        buffedEnemy.baseSpeed = selectedEnemy.baseSpeed;
        buffedEnemy.baseluck = selectedEnemy.baseluck;
        buffedEnemy.goldDropAmount = Mathf.RoundToInt(selectedEnemy.goldDropAmount * buffMultiplier);


        buffedEnemy.weaknesses = new List<Element>(selectedEnemy.weaknesses);
        buffedEnemy.resistances = new List<Element>(selectedEnemy.resistances);
        buffedEnemy.attackPatterns = new List<EnemyStats.AttackPattern>(selectedEnemy.attackPatterns);


        enemyMember = new EnemyState(buffedEnemy);
        enemyMember.currentHealth = buffedEnemy.baseMaxHealth;
        enemyMember.initializePushTurnIcons();

        battlesCompleted++;
    }
    public void AddGold(int amount) // adds gold after combat
    {
        currentGold += amount;
    }

    
    public bool SpendGold(int cost)
    {
        if (currentGold >= cost)
        {
            currentGold -= cost;
            return true;
        }
        return false;
    }
}