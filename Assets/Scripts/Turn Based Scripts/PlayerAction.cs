using System;
using System.Linq;
using battleEnum;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    public GameFlow gameManager;
    public MenuActions menuActions;

    public event Action playerTurnEnd;
    public event Action<int, bool> menuDisplay; // id and whether to show or not
    public event Action<int> applyBurn;
    public event Action<int> applySlow;
    public event Action<int> applyDamageDebuff;
    public event Action<float> dealDamage;

    public EnemyAction enemyAction;

    public PlayerState currentPlayer;
    private int currentID;

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

    public void addTriggers(int id)
    {
        enemyAction.enemyDealDamage += GameSession.gs.partyMembers[id].takeDamage;
    }

    public void removeTriggers()
    {
        dealDamage = null;
        applyBurn = null;
        applySlow = null;
        applyDamageDebuff = null;
    }

    private void myTurnInitialization(int id)
    {
        if (id >= 10) // if enemy turn
        {
            return;
        }

        // make the menu screen pop up in the first place

        currentID = id;
        currentPlayer = GameSession.gs.partyMembers[currentID];

        menuDisplay?.Invoke(id, true);
    }

    private void myTurnFinalization(string action)
    {
        // a menu action has been chosen

        if (action == "attack")
        {
            float totalDamage = currentPlayer.calculateBasicAttack();

            dealDamage?.Invoke(totalDamage);
            print("Player " + currentID + " dealt " + totalDamage + " damage with a basic attack.");
            // add a pause or something for attack animation to play
        }
        else if (currentPlayer.playerStats.spellInfo.Keys.ToList().Contains(action))
        {
            float totalDamage = currentPlayer.calculateSpellAttack(action);

            dealDamage?.Invoke(totalDamage);
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

        // make items work later

        enemyAction.enemyDealDamage -= GameSession.gs.partyMembers[currentID].takeDamage;
        playerTurnEnd?.Invoke();
    }
}
