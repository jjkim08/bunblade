using System;
using System.Collections;
using System.Collections.Generic;
using battleEnum;
using UnityEngine;

public class EnemyAction : MonoBehaviour
{
    public GameFlow gameManager;
    public event Action enemyTurnEnd;
    public event Action<float> enemyDealDamage;
    public PlayerAction playerAction;

    private int currentID;
    public EnemyState currentEnemy;
    void OnEnable()
    {
        gameManager.turnChanged += myTurnStart;
    }

    void OnDisable()
    {
        gameManager.turnChanged -= myTurnStart;

        // playerAction.applyBurn -= takeBurn;
    }

    public void addTriggers(int id)
    {
        playerAction.dealDamage += GameSession.gs.enemyPartyMembers[id].takeDamage;
        playerAction.applyBurn += GameSession.gs.enemyPartyMembers[id].takeBurn;
    }

    public void removeTriggers()
    {
        enemyDealDamage = null;
    }

    private void myTurnStart(int id)
    {
        if (id < 10) return; // not this enemy turn

        currentID = id;
        currentEnemy = GameSession.gs.enemyPartyMembers[currentID];

        myTurn();
    }

    private void myTurn()
    {
        enemyDealDamage?.Invoke(currentEnemy.calculateAttack()); // add randomizer attacks

        if (currentEnemy.currentBurnStacks.Count > 0) // removing burn stacks
        {
            currentEnemy.currentBurnStacks.RemoveAll(stack => stack.Item2 <= 0); // remove expired stacks

            for (int i = 0; i < currentEnemy.currentBurnStacks.Count; i++)
            {
                var (stacks, duration) = currentEnemy.currentBurnStacks[i];
                currentEnemy.currentBurnStacks[i] = (stacks, duration - 1); // decrease duration
            }

            currentEnemy.takeDamage(currentEnemy.calculateBurnDamage());
        }

        enemyTurnEnd?.Invoke();
    }
}
