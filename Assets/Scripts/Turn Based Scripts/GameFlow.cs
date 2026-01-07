using System;
using UnityEngine;
using battleEnum;
using System.Collections.Generic;
using System.Linq;

public class GameFlow : MonoBehaviour
{
    public BattleState currentState = BattleState.playerTurn;
    public PlayerAction playerAction;
    public EnemyAction enemyAction;

    public event Action<int> turnChanged; // 0 = player turn, 1 = enemy turn

    private int currentTurnActor = -1; // Track which actor (player/enemy) is currently taking actions
    private SortedList<(int time, int actor), int> turnOrder = new SortedList<(int time, int actor), int>();
    // key: (time, actor) for uniqueness, value: actor (0 = player, 1 = enemy)
    private Dictionary<int, int> speedByActor = new Dictionary<int, int>();
    // actor : speed hashmap

    private int determineSpeedCoefficient(int speed)
    {
        return (int)((float)1 / speed * 10000);
    }

    void Start()
    {
        // Initialize turn order with team 0 (player) and team 1 (enemy)
        int playerTime = determineSpeedCoefficient(GameSession.gs.playerMember.playerStats.baseSpeed);
        int enemyTime = determineSpeedCoefficient(GameSession.gs.enemyMember.enemyStats.baseSpeed);

        turnOrder.Add((playerTime, 0), 0);
        turnOrder.Add((enemyTime, 1), 1);
        speedByActor.Add(0, playerTime);
        speedByActor.Add(1, enemyTime);

        print("Player has speed coefficient " + playerTime);
        print("Enemy has speed coefficient " + enemyTime);

        // Set up damage event triggers once for 1v1
        playerAction.addTriggers();
        enemyAction.addTriggers();

        continueGameFlow();
    }

    void OnEnable()
    {
        playerAction.playerTurnEnd += continueGameFlow;
        enemyAction.enemyTurnEnd += continueGameFlow;
    }

    void OnDisable()
    {
        playerAction.playerTurnEnd -= continueGameFlow;
        enemyAction.enemyTurnEnd -= continueGameFlow;
    }

    private void cleanupTriggers()
    {
        playerAction.removeTriggers();
        enemyAction.removeTriggers();
    }


    private void continueGameFlow()
    {
        // Check if battle is over
        if (GameSession.gs.playerMember.currentHealth <= 0)
        {
            EndBattle("Enemy wins! Player has been defeated.");
            return;
        }

        if (GameSession.gs.enemyMember.currentHealth <= 0)
        {
            EndBattle("Player wins! Enemy has been defeated.");
            return;
        }

        bool currentActorHasIcons = false;
        if (currentTurnActor == 0 && GameSession.gs.playerMember.hasPushTurnIcons())
        {
            currentActorHasIcons = true;
        }
        else if (currentTurnActor == 1 && GameSession.gs.enemyMember.hasPushTurnIcons())
        {
            currentActorHasIcons = true;
        }

        // If current actor still has icons, continue their turn
        if (currentActorHasIcons)
        {
            print($"{(currentTurnActor == 0 ? "Player" : "Enemy")} continues with {(currentTurnActor == 0 ? GameSession.gs.playerMember.pushTurnHalves : GameSession.gs.enemyMember.pushTurnHalves)} half-icons remaining");
            turnChanged?.Invoke(currentTurnActor);
            return;
        }

        // Current actor is out of icons, switch to next actor in initiative
        var (time, actor) = turnOrder.Keys[0];
        turnOrder.RemoveAt(0);

        currentTurnActor = actor;

        if (actor == 0)
        {
            GameSession.gs.playerMember.initializePushTurnIcons();
        }
        else
        {
            GameSession.gs.enemyMember.initializePushTurnIcons();
        }

        print($"It's now {(actor == 0 ? "Player" : "Enemy")}'s turn phase! (6 half-icons)");

        // Reschedule this unit's next turn
        turnOrder.Add((time + speedByActor[actor], actor), actor);
        turnChanged?.Invoke(actor);
    }

    private void EndBattle(string message)
    {
        print(message);
        GameSession.gs.syncPlayerToGlobal();
        cleanupTriggers();
        gameObject.SetActive(false);
    }
}
