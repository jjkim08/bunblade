using System;
using System.Collections;
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
    public ParryController parryController; // handles parry timings
    public SpriteRenderer playerSprite; // assign in inspector

    public PlayerState currentPlayer;

    void Start()
    {
        // adds the event listeners
        if (gameManager != null)
        {
            gameManager.turnChanged += myTurnInitialization;
        }
        if (menuActions != null)
        {
            menuActions.playerActionFired += myTurnFinalization;
        }
        if (enemyAction != null)
        {
            enemyAction.enemyAttackDeclared += OnEnemyAttackDeclared;
        }

        // Wire player sprite to parry controller for color feedback
        if (parryController != null)
        {
            parryController.playerSprite = playerSprite;
        }
    }

    void OnDisable()
    {
        // so that it doesn't add multiple times
        if (gameManager != null)
        {
            gameManager.turnChanged -= myTurnInitialization;
        }
        if (menuActions != null)
        {
            menuActions.playerActionFired -= myTurnFinalization;
        }
        if (enemyAction != null)
        {
            enemyAction.enemyAttackDeclared -= OnEnemyAttackDeclared;
        }
    }

    public void addTriggers()
    {
        if (enemyAction == null || GameSession.gs == null || GameSession.gs.playerMember == null) return;

        enemyAction.enemyDealDamage += GameSession.gs.playerMember.takeDamage;
    }

    public void removeTriggers()
    {
        if (enemyAction == null || GameSession.gs == null || GameSession.gs.playerMember == null) return;

        enemyAction.enemyDealDamage -= GameSession.gs.playerMember.takeDamage;
    }

    private void myTurnInitialization(int turnOwner)
    {
        if (turnOwner == 1) return; // enemy turn
        print("my turn");

        if (GameSession.gs == null || GameSession.gs.playerMember == null) return;

        currentPlayer = GameSession.gs.playerMember;
        if (currentPlayer == null) return;

        applyBurnDamage();

        menuDisplay?.Invoke(true);
    }

    private void applyBurnDamage()
    {
        if (currentPlayer != null && currentPlayer.currentBurnStacks > 0)
        {
            float burnDamage = currentPlayer.calculateBurnDamage();
            currentPlayer.takeDamage(burnDamage, Element.Fire);
        }
    }

    // Enemy declared an attack; resolve parry windows and apply damage/mana, then finalize enemy turn
    private void OnEnemyAttackDeclared(AttackData attack)
    {
        if (attack == null)
        {
            enemyAction?.FinalizeAttack(null);
            return;
        }
        StartCoroutine(HandleEnemyAttack(attack));
    }

    private IEnumerator HandleEnemyAttack(AttackData attack)
    {
        if (parryController == null)
        {
            if (attack != null && attack.hits != null)
            {
                foreach (var hit in attack.hits)
                {
                    enemyAction?.RaiseEnemyDealDamage(hit.baseDamage, attack.element);
                }
            }
            enemyAction?.FinalizeAttack(attack);
            yield break;
        }

        yield return StartCoroutine(parryController.parryStages(
            attack,
            onHitResolved: (ResolvedHit rh) =>
            {
                if (rh.grantMana)
                {
                    GameSession.gs.playerMember.gainMana(1);
                }
                enemyAction.RaiseEnemyDealDamage(rh.finalDamage, rh.element);
            },
            onAttackResolved: (AttackResolution res) =>
            {
                if (res.grantFullTurnIconNextTurn)
                {
                    GameSession.gs.playerMember.pendingBonusTurnIcons += 1;
                }
            }
        ));

        // After attack sequence: consume enemy icons and tick debuffs
        enemyAction.FinalizeAttack(attack);
    }

    private void myTurnFinalization(string action)
    {
        menuDisplay?.Invoke(false);
        // a menu action has been chosen
        Element actionElement = Element.None;

        if (currentPlayer == null)
        {
            playerTurnEnd?.Invoke();
            return;
        }

        if (action == "attack")
        {
            float totalDamage = currentPlayer.calculateBasicAttack();
            actionElement = Element.None;

            dealDamage?.Invoke(totalDamage, actionElement);
            currentPlayer.gainMana(1);
            // add a pause or something for attack animation to play
        }
        else if (currentPlayer.playerStats.spellInfo.Keys.ToList().Contains(action))
        {
            // Check if player has enough mana
            if (!currentPlayer.canCastSpell(action))
            {
                return;
            }

            int manaCost = currentPlayer.playerStats.spellInfo[action].manaCost;
            actionElement = currentPlayer.playerStats.spellInfo[action].element;
            float totalDamage = currentPlayer.calculateSpellAttack(action);

            dealDamage?.Invoke(totalDamage, actionElement);
            currentPlayer.consumeMana(manaCost);

            // add a pause or something for spell animation to play

            // also, apply spell effects here
            if (action == "Ignia")
            {
                applyBurn?.Invoke(currentPlayer.playerStats.spellInfo[action].appliedStacks);
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

        // make items work later

        // Tick debuff durations at end of turn
        currentPlayer.tickBurnDuration();
        currentPlayer.tickSlowDuration();

        playerTurnEnd?.Invoke();
    }
}

