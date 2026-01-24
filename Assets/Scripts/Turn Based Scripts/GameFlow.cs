using System;
using System.Collections;
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

    private int currentTurnActor = -1; // -1 default, 0 = player, 1 = enemy

    private SortedList<(int time, int actor), int> turnOrder = new SortedList<(int time, int actor), int>();
    // key: (time, actor), value: actor (0 = player, 1 = enemy)

    private Dictionary<int, int> speedByActor = new Dictionary<int, int>();
    // actor : speed hashmap

    private int determineSpeedCoefficient(int speed)
    {
        return (int)((float)1 / speed * 10000); // makes the usable speed out of a speed value
    }

    void Start()
    {

        int playerTime = determineSpeedCoefficient(GameSession.gs.playerMember.playerStats.baseSpeed);
        int enemyTime = determineSpeedCoefficient(GameSession.gs.enemyMember.enemyStats.baseSpeed);

        turnOrder.Add((playerTime, 0), 0);
        turnOrder.Add((enemyTime, 1), 1);

        speedByActor[0] = playerTime;
        speedByActor[1] = enemyTime;

        // add triggers
        playerAction.addTriggers();
        enemyAction.addTriggers();

        playerAction.playerTurnEnd += continueGameFlow;
        enemyAction.enemyTurnEnd += continueGameFlow;

        continueGameFlow();
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
        // check if battle is over
        if (GameSession.gs.playerMember.currentHealth <= 0 || GameSession.gs.enemyMember.currentHealth <= 0)
        {
            EndBattle();
            return;
        }

        // If current actor still has icons, continue their phase
        bool currentActorHasIcons = false;

        if (currentTurnActor == 0)
        {
            currentActorHasIcons = GameSession.gs.playerMember.hasPushTurnIcons();
        }
        else if (currentTurnActor == 1)
        {
            currentActorHasIcons = GameSession.gs.enemyMember.hasPushTurnIcons();
        }

        if (currentActorHasIcons)
        {
            turnChanged?.Invoke(currentTurnActor);
            return;
        }

        // Current actor is out of icons, switch to next actor in initiative
        if (turnOrder.Count == 0)
        {
            // Rebuild schedule if empty (safety)
            int playerTime = determineSpeedCoefficient(GameSession.gs.playerMember.playerStats.baseSpeed);
            int enemyTime = determineSpeedCoefficient(GameSession.gs.enemyMember.enemyStats.baseSpeed);
            turnOrder.Add((playerTime, 0), 0);
            turnOrder.Add((enemyTime, 1), 1);
            speedByActor[0] = playerTime;
            speedByActor[1] = enemyTime;
        }

        var nextKey = turnOrder.Keys[0];
        int time = nextKey.time;
        int actor = nextKey.actor;
        turnOrder.RemoveAt(0);

        currentTurnActor = actor;

        if (actor == 0)
        {
            GameSession.gs.playerMember.initializePushTurnIcons();
            // Apply any pending bonus full icons awarded from parries
            var pState = GameSession.gs.playerMember;
            if (pState.pendingBonusTurnIcons > 0)
            {
                int bonus = pState.pendingBonusTurnIcons;
                pState.pushTurnHalves += bonus * 2; // 1 full icon = 2 halves
                pState.pendingBonusTurnIcons = 0;
            }
        }
        else
        {
            GameSession.gs.enemyMember.initializePushTurnIcons();
        }

        // Reschedule this unit's next turn
        turnOrder.Add((time + speedByActor[actor], actor), actor);

        turnChanged?.Invoke(actor);
    }

    private void EndBattle()
    {
        cleanupTriggers();
        gameObject.SetActive(false);
    }

    // No extra helpers needed; stored on PlayerState
}

