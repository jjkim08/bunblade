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

    public int[] frontLine = new int[2]; // ids of the front line characters, first is player, second is enemy

    public event Action<int> turnChanged;

    private SortedList<(int time, int id), int> turnOrder = new SortedList<(int time, int id), int>();
    // key: (time, id) for uniqueness, value: id
    private Dictionary<int, int> speedDict = new Dictionary<int, int>();
    // id : speed hashmap

    private int determineSpeedCoefficient(int speed)
    {
        return (int)((float)1 / speed * 10000);
    }

    private void addEventsToAction()
    {
        playerAction.removeTriggers();
        enemyAction.removeTriggers();

        // Get the first player turn (id < 10) in sorted order
        for (int i = 0; i < turnOrder.Count; i++)
        {
            if (turnOrder.Values[i] < 10)
            {
                playerAction.addTriggers(turnOrder.Values[i]);
                break;
            }
        }

        // Get the first enemy turn (id >= 10) in sorted order
        for (int i = 0; i < turnOrder.Count; i++)
        {
            if (turnOrder.Values[i] >= 10)
            {
                enemyAction.addTriggers(turnOrder.Values[i]);
                break;
            }
        }
    }

    void Start()
    {
        foreach (KeyValuePair<int, PlayerState> member in GameSession.gs.partyMembers)
        {
            int time = determineSpeedCoefficient(member.Value.playerStats.baseSpeed);
            int id = member.Value.playerStats.id;
            turnOrder.Add((time, id), id);
            speedDict.Add(id, time);
        }

        foreach (KeyValuePair<int, EnemyState> enemy in GameSession.gs.enemyPartyMembers)
        {
            int time = determineSpeedCoefficient(enemy.Value.enemyStats.baseSpeed);
            int id = enemy.Value.enemyStats.id;
            turnOrder.Add((time, id), id);
            speedDict.Add(id, time);
        }

        foreach (KeyValuePair<int, int> i in speedDict)
        {
            print(i.Key + " has speed " + i.Value);
        }

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


    private void continueGameFlow()
    {
        var firstKey = turnOrder.Keys[0]; // (time, id) tuple
        int currentTurnTime = firstKey.time;
        int currentID = turnOrder.Values[0];

        addEventsToAction();

        turnOrder.RemoveAt(0); // remove the first element

        print("It's now character " + currentID + "'s turn!");

        turnOrder.Add((currentTurnTime + speedDict[currentID], currentID), currentID); // add it back with updated time
        turnChanged?.Invoke(currentID); // tell the PlayerAction or EnemyAction component that it's their turn
    }
}
