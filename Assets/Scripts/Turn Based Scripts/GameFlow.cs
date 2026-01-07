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

    private int currentCharacter = -1; // used to see whos turn it is, -1 means the default, 0 means player and 1 means enemy

    private SortedList<int time, int character> turnOrder = new SortedList<int time, int character>();
    // key: time, value: character(0 = player, 1 = enemy)

    private Dictionary<int, int> speedByActor = new Dictionary<int, int>();
    // actor : speed hashmap

    private int determineSpeedCoefficient(int speed)
    {
        return (int)((float)1 / speed * 10000); // makes the usable speed out of a speed value
    }

    void Start()
    {
        turnOrder.Add(playerTime, 0);
        turnOrder.Add(enemyTime, 1);

        speedByActor.Add(0, determineSpeedCoefficient(GameSession.gs.playerMember.playerStats.baseSpeed));
        speedByActor.Add(1, determineSpeedCoefficient(GameSession.gs.enemyMember.enemyStats.baseSpeed));

        // add triggers
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
        // check if battle is over
        if (GameSession.gs.playerMember.currentHealth <= 0 || GameSession.gs.enemyMember.currentHealth <= 0)
        {
            // todo: make the game end
            EndBattle();
            return;
        }

        currentCharacter
        turnChanged?.Invoke(GameSession.gs.);

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

    private void EndBattle()
    {
        cleanupTriggers();
        gameObject.SetActive(false);
    }
}

