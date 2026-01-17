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
    public event Action<AttackData> enemyAttackDeclared; // declares the attack sequence
    public event Action<EnemyState> onEnemyInitialized; // fired when enemy is ready
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
        // Notify UI listeners that enemy is initialized (for health bar subscription)
        onEnemyInitialized?.Invoke(currentEnemy);
        ExecuteTurn();
    }

    private void ExecuteTurn()
    {
        ApplyBurnDamage();

        // Pick a random attack from enemy's attack patterns
        if (currentEnemy.enemyStats.attackPatterns.Count == 0)
        {
            return; // No attacks defined
        }

        EnemyStats.AttackPattern selectedPattern = currentEnemy.enemyStats.attackPatterns[UnityEngine.Random.Range(0, currentEnemy.enemyStats.attackPatterns.Count)];

        // Instantiate the attack with scaled damage
        AttackData attack = currentEnemy.enemyStats.InstantiateAttack(selectedPattern, currentEnemy.calculateAttack() / Mathf.Max(1f, selectedPattern.hits.Count));
        attack.iconCostHalves = currentEnemy.calculateIconCost(attack.element);

        // Declare the attack; GameFlow will resolve parry windows and finalize the turn
        enemyAttackDeclared?.Invoke(attack);
    }

    // Called by PlayerAction to apply resolved damage to the player
    public void RaiseEnemyDealDamage(float damage, Element element)
    {
        enemyDealDamage?.Invoke(damage, element);
    }

    // Called by PlayerAction to consume icons, tick debuffs, and end the enemy turn
    public void FinalizeAttack(AttackData attack)
    {
        var enemy = GameSession.gs.enemyMember;
        enemy.consumePushTurnIcons(attack.iconCostHalves);
        enemy.tickBurnDuration();
        enemy.tickSlowDuration();
        enemyTurnEnd?.Invoke();
    }

    private void ApplyBurnDamage()
    {
        if (currentEnemy.currentBurnStacks > 0)
        {
            float burnDamage = currentEnemy.calculateBurnDamage();
            currentEnemy.takeDamage(burnDamage, Element.Fire);
        }
    }
}
