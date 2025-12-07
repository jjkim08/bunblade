using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession gs { get; private set; }

    public Dictionary<int, PlayerState> partyMembers = new Dictionary<int, PlayerState>();
    public List<PlayerStats> playerStats;
    public Dictionary<int, EnemyState> enemyPartyMembers = new Dictionary<int, EnemyState>();
    public List<EnemyStats> enemyStats;

    void Awake()
    {
        if (gs != null)
        {
            Destroy(gameObject);
            return;
        }

        gs = this;
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < playerStats.Count; i++)
        {
            partyMembers.Add(playerStats[i].id, new PlayerState(playerStats[i]));
            partyMembers[playerStats[i].id].currentHealth = playerStats[i].baseMaxHealth;
        }

        for (int i = 0; i < enemyStats.Count; i++)
        {
            enemyPartyMembers.Add(enemyStats[i].id, new EnemyState(enemyStats[i]));
            enemyPartyMembers[enemyStats[i].id].currentHealth = enemyStats[i].baseMaxHealth;
        }
    }
}