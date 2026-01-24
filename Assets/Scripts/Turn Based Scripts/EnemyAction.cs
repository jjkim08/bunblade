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

    void Start()
    {
        if (gameManager != null)
        {
            gameManager.turnChanged += myTurnStart;
        }
    }

    void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.turnChanged -= myTurnStart;
        }
    }

    public void addTriggers()
    {
        if (playerAction == null || GameSession.gs == null || GameSession.gs.enemyMember == null) return;

        playerAction.dealDamage += GameSession.gs.enemyMember.takeDamage;
        playerAction.applyBurn += GameSession.gs.enemyMember.takeBurn;
        playerAction.applySlow += GameSession.gs.enemyMember.takeSlow;
        playerAction.applyDamageDebuff += GameSession.gs.enemyMember.takeDamageDebuff;
    }

    public void removeTriggers()
    {
        if (playerAction == null || GameSession.gs == null || GameSession.gs.enemyMember == null) return;

        playerAction.dealDamage -= GameSession.gs.enemyMember.takeDamage;
        playerAction.applyBurn -= GameSession.gs.enemyMember.takeBurn;
        playerAction.applySlow -= GameSession.gs.enemyMember.takeSlow;
        playerAction.applyDamageDebuff -= GameSession.gs.enemyMember.takeDamageDebuff;
    }

    private void myTurnStart(int turnOwner)
    {
        if (turnOwner == 0) return; // player turn

        if (GameSession.gs == null || GameSession.gs.enemyMember == null) return;

        currentEnemy = GameSession.gs.enemyMember;
        // Notify UI listeners that enemy is initialized (for health bar subscription)
        onEnemyInitialized?.Invoke(currentEnemy);
        ExecuteTurn();
    }

    private void ExecuteTurn()
    {
        if (currentEnemy == null) return;

        ApplyBurnDamage();

        // Pick a random attack from enemy's attack patterns
        if (currentEnemy.enemyStats == null || currentEnemy.enemyStats.attackPatterns == null || currentEnemy.enemyStats.attackPatterns.Count == 0)
        {
            return; // No attacks defined
        }

        EnemyStats.AttackPattern selectedPattern = currentEnemy.enemyStats.attackPatterns[UnityEngine.Random.Range(0, currentEnemy.enemyStats.attackPatterns.Count)];

        // Instantiate the attack with total damage (division by hit count happens in InstantiateAttack)
        AttackData attack = currentEnemy.enemyStats.InstantiateAttack(selectedPattern, currentEnemy.calculateAttack());
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
        if (enemy == null) return;

        int iconCost = attack != null ? attack.iconCostHalves : 0;

        enemy.consumePushTurnIcons(iconCost);
        enemy.tickBurnDuration();
        enemy.tickSlowDuration();
        enemy.tickDamageDebuffDuration();
        enemyTurnEnd?.Invoke();
    }

    private void ApplyBurnDamage()
    {
        if (currentEnemy != null && currentEnemy.currentBurnStacks > 0)
        {
            float burnDamage = currentEnemy.calculateBurnDamage();
            currentEnemy.takeDamage(burnDamage, Element.Fire);
        }
    }
}
