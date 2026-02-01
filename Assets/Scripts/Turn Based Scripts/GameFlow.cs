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

    private int determineSpeedCoefficient(int speed)
    {
        if (speed <= 0) return 10000;
        return (int)((float)1 / speed * 10000); // makes the usable speed out of a speed value
    }

    private int getActorTime(int actor)
    {
        if (actor == 0)
        {
            var player = GameSession.gs != null ? GameSession.gs.playerMember : null;
            if (player != null)
            {
                int baseTime = determineSpeedCoefficient(player.playerStats.baseSpeed);
                int adjusted = Mathf.CeilToInt(baseTime * player.getSpeedTimeMultiplier());
                return adjusted > 0 ? adjusted : 1;
            }
        }
        else
        {
            var enemy = GameSession.gs != null ? GameSession.gs.enemyMember : null;
            if (enemy != null)
            {
                int baseTime = determineSpeedCoefficient(enemy.enemyStats.baseSpeed);
                int adjusted = Mathf.CeilToInt(baseTime * enemy.getSpeedTimeMultiplier());
                return adjusted > 0 ? adjusted : 1;
            }
        }

        return 1;
    }

    void Start()
    {
        if (GameSession.gs == null || GameSession.gs.playerMember == null || GameSession.gs.enemyMember == null) return;
        if (playerAction == null || enemyAction == null) return;

        int playerTime = getActorTime(0);
        int enemyTime = getActorTime(1);

        turnOrder.Add((playerTime, 0), 0);
        turnOrder.Add((enemyTime, 1), 1);

        // add triggers
        playerAction.addTriggers();
        enemyAction.addTriggers();

        playerAction.playerTurnEnd += continueGameFlow;
        enemyAction.enemyTurnEnd += continueGameFlow;

        continueGameFlow();
    }

    void OnDisable()
    {
        if (playerAction != null)
        {
            playerAction.playerTurnEnd -= continueGameFlow;
        }
        if (enemyAction != null)
        {
            enemyAction.enemyTurnEnd -= continueGameFlow;
        }
    }

    private void cleanupTriggers()
    {
        if (playerAction != null)
        {
            playerAction.removeTriggers();
        }
        if (enemyAction != null)
        {
            enemyAction.removeTriggers();
        }
    }


    private void continueGameFlow()
    {
        // check if battle is over
        if (GameSession.gs == null || GameSession.gs.playerMember == null || GameSession.gs.enemyMember == null)
        {
            EndBattle();
            return;
        }

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
            int playerTime = getActorTime(0);
            int enemyTime = getActorTime(1);
            turnOrder.Add((playerTime, 0), 0);
            turnOrder.Add((enemyTime, 1), 1);
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
                pState.addPushTurnHalves(bonus * 2); // 1 full icon = 2 halves
                pState.pendingBonusTurnIcons = 0;
            }
        }
        else
        {
            GameSession.gs.enemyMember.initializePushTurnIcons();
        }

        // Reschedule this unit's next turn
        turnOrder.Add((time + getActorTime(actor), actor), actor);

        turnChanged?.Invoke(actor);
    }

    private void EndBattle()
    {
        currentState = BattleState.gameOver;

        // Award gold if enemy was defeated
        if (GameSession.gs != null && GameSession.gs.enemyMember != null && GameSession.gs.enemyMember.currentHealth <= 0)
        {
            int goldReward = GameSession.gs.enemyMember.enemyStats.goldDropAmount;
            GameSession.gs.AddGold(goldReward);
        }

        cleanupTriggers();
        gameObject.SetActive(false);
    }

    // No extra helpers needed; stored on PlayerState
}

