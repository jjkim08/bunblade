using System;
using System.Collections;
using System.Collections.Generic;
using battleEnum;
using UnityEngine;

public class EnemyAction : MonoBehaviour
{
    public GameFlow gameManager;
    public event Action enemyTurnEnd;
    public event Action<float, Element> enemyDealDamage; // (damage, element)
    public PlayerAction playerAction;

    public EnemyState currentEnemy;

    void OnEnable()
    {
        gameManager.turnChanged += myTurnStart;
    }

    void OnDisable()
    {
        gameManager.turnChanged -= myTurnStart;
    }

    public void addTriggers()
    {
        playerAction.dealDamage += GameSession.gs.enemyMember.takeDamage;
        playerAction.applyBurn += GameSession.gs.enemyMember.takeBurn;
        playerAction.applySlow += GameSession.gs.enemyMember.takeSlow;
    }

    public void removeTriggers()
    {
        enemyDealDamage = null;
        playerAction.dealDamage -= GameSession.gs.enemyMember.takeDamage;
        playerAction.applyBurn -= GameSession.gs.enemyMember.takeBurn;
        playerAction.applySlow -= GameSession.gs.enemyMember.takeSlow;
    }

    private void myTurnStart(int turnOwner)
    {
        if (turnOwner == 0) return; // player turn

        currentEnemy = GameSession.gs.enemyMember;
        ExecuteTurn();
    }

    private void ExecuteTurn()
    {
        ApplyBurnDamage();

        Element attackElement = Element.None;
        enemyDealDamage?.Invoke(currentEnemy.calculateAttack(), attackElement);

        int iconCost = currentEnemy.calculateIconCost(attackElement);
        currentEnemy.consumePushTurnIcons(iconCost);
        Debug.Log($"Enemy action consumed {iconCost} half-icons. Remaining halves: {currentEnemy.pushTurnHalves}");

        // Tick debuff durations at end of turn
        currentEnemy.tickBurnDuration();
        currentEnemy.tickSlowDuration();

        enemyTurnEnd?.Invoke();
    }

    private void ApplyBurnDamage()
    {
        if (currentEnemy.currentBurnStacks > 0)
        {
            float burnDamage = currentEnemy.calculateBurnDamage();
            currentEnemy.takeDamage(burnDamage, Element.Fire);
            Debug.Log($"Enemy took {burnDamage} burn damage ({currentEnemy.currentBurnStacks} stacks, {currentEnemy.burnTurnsRemaining} turns remaining)");
        }
    }
}
