using System;
using System.Linq;
using battleEnum;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    public GameFlow gameManager;
    public MenuActions menuActions;

    public event Action playerTurnEnd;
    public event Action<bool> menuDisplay; // whether to show or not
    public event Action<int> applyBurn; // (stacks, element)
    public event Action<int> applySlow;
    public event Action<int> applyDamageDebuff;
    public event Action<float, Element> dealDamage; // (damage, element)

    public EnemyAction enemyAction;

    public PlayerState currentPlayer;

    void OnEnable()
    {
        // adds the event listeners
        gameManager.turnChanged += myTurnInitialization;
        menuActions.playerActionFired += myTurnFinalization;
    }

    void OnDisable()
    {
        // so that it doesn't add multiple times
        gameManager.turnChanged -= myTurnInitialization;
        menuActions.playerActionFired -= myTurnFinalization;
    }

    public void addTriggers()
    {
        enemyAction.enemyDealDamage += GameSession.gs.playerMember.takeDamage;
    }

    public void removeTriggers()
    {
        dealDamage = null;
        applyBurn = null;
        applySlow = null;
        applyDamageDebuff = null;
        enemyAction.enemyDealDamage -= GameSession.gs.playerMember.takeDamage;
    }

    private void myTurnInitialization(int turnOwner)
    {
        if (turnOwner == 1) return; // enemy turn

        currentPlayer = GameSession.gs.playerMember;

        applyBurnDamage();

        menuDisplay?.Invoke(true);
    }

    private void applyBurnDamage()
    {
        if (currentPlayer.currentBurnStacks > 0)
        {
            float burnDamage = currentPlayer.calculateBurnDamage();
            currentPlayer.takeDamage(burnDamage, Element.Fire);
            Debug.Log($"Player took {burnDamage} burn damage ({currentPlayer.currentBurnStacks} stacks, {currentPlayer.burnTurnsRemaining} turns remaining)");
        }
    }

    private void myTurnFinalization(string action)
    {
        // a menu action has been chosen
        Element actionElement = Element.None;

        if (action == "attack")
        {
            float totalDamage = currentPlayer.calculateBasicAttack();
            actionElement = Element.None;

            dealDamage?.Invoke(totalDamage, actionElement);
            currentPlayer.gainMana(1);
            Debug.Log($"Player gained 1 mana from attack. Current: {currentPlayer.currentMana}/{PlayerState.MAX_MANA}");
            print("Player dealt " + totalDamage + " damage with a basic attack.");
            // add a pause or something for attack animation to play
        }
        else if (currentPlayer.playerStats.spellInfo.Keys.ToList().Contains(action))
        {
            // Check if player has enough mana
            if (!currentPlayer.canCastSpell(action))
            {
                print("Not enough mana to cast " + action + "!");
                return;
            }

            int manaCost = currentPlayer.playerStats.spellInfo[action].manaCost;
            actionElement = currentPlayer.playerStats.spellInfo[action].element;
            float totalDamage = currentPlayer.calculateSpellAttack(action);

            dealDamage?.Invoke(totalDamage, actionElement);
            currentPlayer.consumeMana(manaCost);
            Debug.Log($"Player cast {action}, consumed {manaCost} mana. Remaining: {currentPlayer.currentMana}/{PlayerState.MAX_MANA}");

            // add a pause or something for spell animation to play

            // also, apply spell effects here
            if (action == "Ignia")
            {
                applyBurn?.Invoke(currentPlayer.playerStats.spellInfo[action].appliedStacks); // 2 stacks of burn
            }
            else if (action == "Glacia")
            {
                applySlow?.Invoke(currentPlayer.playerStats.spellInfo[action].appliedStacks); // slows by 10%
            }
            else if (action == "Tearre")
            {
                currentPlayer.giveShield(currentPlayer.calculateShieldAmount()); // shields by ability power
            }
            else if (action == "Curatia")
            {
                currentPlayer.heal(currentPlayer.calculateHealAmount()); // heals ability power * 1.5 health
            }
            else if (action == "Aquis")
            {
                applyDamageDebuff?.Invoke(currentPlayer.playerStats.spellInfo[action].appliedStacks); // deals 10% less damage
            }
        }

        int iconCostHalves = currentPlayer.calculateIconCost(actionElement);
        currentPlayer.consumePushTurnIcons(iconCostHalves);
        Debug.Log($"Player action consumed {iconCostHalves} half-icons. Remaining halves: {currentPlayer.pushTurnHalves}");

        // make items work later

        // Tick debuff durations at end of turn
        currentPlayer.tickBurnDuration();
        currentPlayer.tickSlowDuration();

        playerTurnEnd?.Invoke();
    }
}
