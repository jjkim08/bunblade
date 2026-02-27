using System;
using System.Collections;
using System.Linq;
using battleEnum;
using UnityEngine;

public class PlayerAction : MonoBehaviour // handles the actual player action itself
{
    public GameFlow gameManager;
    public MenuActions menuActions;

    public event Action playerTurnEnd;
    public event Action<bool> menuDisplay;
    public event Action<int> applyBurn;
    public event Action<int> applySlow;
    public event Action<int> applyDamageDebuff;
    public event Action<float, Element> dealDamage;

    public EnemyAction enemyAction;
    public ParryController parryController;
    public SpriteRenderer playerSprite;
    public Animator playerAnimator;

    [Header("Visual Feedback")]
    public Color damageFlashColor = new Color(1f, 0.3f, 0.3f, 1f);
    public Color parryFlashColor = new Color(0.3f, 0.5f, 1f, 1f); // visuals
    public float flashDuration = 0.15f;

    private Coroutine colorFlashRoutine;

    public PlayerState currentPlayer;

    void Start()
    {

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


        if (parryController != null && playerSprite != null)
        {
            parryController.playerSprite = playerSprite;
            parryController.playerAnimator = playerAnimator;
        }
    }

    void OnDisable()
    {

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

    
    private void myTurnInitialization(int turnOwner) // turn initialize means before the turn has been finalized
    {
        if (turnOwner == 1) return;

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


    
    private void OnEnemyAttackDeclared(AttackData attack)
    {
        if (attack == null)
        {
            enemyAction?.FinalizeAttack(null);
            return;
        }
        StartCoroutine(HandleEnemyAttack(attack)); // starts the parry routine
    }

    
    private IEnumerator HandleEnemyAttack(AttackData attack) // this is a function meant to finalize the parries that the player does
    {
        if (parryController == null)
        {
            if (attack != null && attack.hits != null)
            {
                int hitIndex = 0;
                foreach (var hit in attack.hits)
                {
                    enemyAction?.TriggerAttackAnimation(attack, hitIndex);
                    hitIndex++;
                    enemyAction?.RaiseEnemyDealDamage(hit.baseDamage, attack.element);
                }
            }
            enemyAction?.FinalizeAttack(attack);
            yield break;
        }

        yield return StartCoroutine(parryController.parryStages(
            attack,
            onWindupStart: (int hitIndex) =>
            {

                enemyAction?.TriggerAttackAnimation(attack, hitIndex);
            },
            onHitResolved: (ResolvedHit rh) =>
            {
                if (rh.grantMana)
                {
                    GameSession.gs.playerMember.gainMana(1);
                }


                if (rh.result == ParryResult.Success)
                {
                    FlashPlayerColor(parryFlashColor);
                }
                else if (rh.finalDamage > 0)
                {
                    FlashPlayerColor(damageFlashColor);
                }

                enemyAction.RaiseEnemyDealDamage(rh.finalDamage, rh.element);
            },
            onReturnStart: () =>
            {

                enemyAction?.TriggerReturnMovement();
            },
            onAttackResolved: (AttackResolution res) =>
            {
                if (res.grantFullTurnIconNextTurn)
                {
                    GameSession.gs.playerMember.pendingBonusTurnIcons += 1;
                }
            }
        ));


        enemyAction.FinalizeAttack(attack);
    }

    private void myTurnFinalization(string action) // finalizes the actions
    {
        StartCoroutine(ExecutePlayerAction(action));
    }

    
    private IEnumerator ExecutePlayerAction(string action)
    {
        menuDisplay?.Invoke(false);

        Element actionElement = Element.None;

        if (currentPlayer == null)
        {
            playerTurnEnd?.Invoke();
            yield break;
        }


        Vector3 originalScale = playerSprite.transform.localScale;

        if (action == "attack")
        {

            playerAnimator.SetTrigger("Attack");


            playerSprite.transform.localScale = originalScale * 1.5f;

            yield return new WaitForSeconds(0.4f);


            float totalDamage = currentPlayer.calculateBasicAttack();
            actionElement = Element.None;

            dealDamage?.Invoke(totalDamage, actionElement);
            currentPlayer.gainMana(1);

            yield return new WaitForSeconds(0.5f);


            playerSprite.transform.localScale = originalScale;
        }
        else if (currentPlayer.playerStats.spellInfo.Keys.ToList().Contains(action))
        {

            if (!currentPlayer.canCastSpell(action))
            {
                yield break;
            }

            int manaCost = currentPlayer.playerStats.spellInfo[action].manaCost;
            actionElement = currentPlayer.playerStats.spellInfo[action].element;
            float totalDamage = currentPlayer.calculateSpellAttack(action);

            Debug.Log($"Casting spell: {action}, Mana cost: {manaCost}");
            dealDamage?.Invoke(totalDamage, actionElement);
            currentPlayer.consumeMana(manaCost);


            if (action == "Ignia")
            {
                applyBurn?.Invoke(currentPlayer.playerStats.spellInfo[action].appliedStacks);
            }
            else if (action == "Glacia")
            {
                applySlow?.Invoke(currentPlayer.playerStats.spellInfo[action].appliedStacks);
            }
            else if (action == "Tearre")
            {
                currentPlayer.giveShield(currentPlayer.calculateShieldAmount());
            }
            else if (action == "Curatia")
            {
                currentPlayer.heal(currentPlayer.calculateHealAmount());
            }
            else if (action == "Aquis")
            {
                applyDamageDebuff?.Invoke(currentPlayer.playerStats.spellInfo[action].appliedStacks);
            }
        }

        int iconCostHalves = currentPlayer.calculateIconCost(actionElement);
        currentPlayer.consumePushTurnIcons(iconCostHalves);


        currentPlayer.tickBurnDuration();
        currentPlayer.tickSlowDuration();

        playerTurnEnd?.Invoke();
    }

    
    public void FlashPlayerColor(Color flashColor)
    {
        if (colorFlashRoutine != null)
        {
            StopCoroutine(colorFlashRoutine);
        }
        colorFlashRoutine = StartCoroutine(ColorFlashRoutine(flashColor));
    }

    
    private IEnumerator ColorFlashRoutine(Color flashColor)
    {
        if (playerSprite == null) yield break;

        Color originalColor = playerSprite.color;
        playerSprite.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        playerSprite.color = originalColor;
        colorFlashRoutine = null;
    }
}

