using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using battleEnum;
using System.Collections.Generic;
using System.Linq;

// actual gameflow handling of the game itself
public class GameFlow : MonoBehaviour
{

    public BattleState currentState = BattleState.playerTurn;
    public PlayerAction playerAction;
    public EnemyAction enemyAction;

    public event Action<int> turnChanged;

    private int currentTurnActor = -1;
    private bool battleEnded = false;

    // using a struct to support customized sorting of the turn order, this allows for more complex turn order logic in the future if desired, such as actors that can act multiple times in a row or have their turn delayed
    private struct TurnEntry : IComparable<TurnEntry>
    {
        public int time;
        public int actor;

        public int CompareTo(TurnEntry other)
        {
            int timeCompare = time.CompareTo(other.time);
            if (timeCompare != 0) return timeCompare;
            return actor.CompareTo(other.actor);
        }
    }

    private Heap<TurnEntry> turnOrder = new Heap<TurnEntry>();

    private int determineSpeedCoefficient(int speed)
    {
        if (speed <= 0) return 10000;
        return (int)((float)1 / speed * 10000);
    }

    // speed is invesely related to the time it takes for a character to take a turn, so higher speed means lower time, this function converts the speed stat into a time value that can be used in the turn order calculations
    private int getActorTime(int actor)
    {
        if (actor == 0)
        {
            var player = GameSession.gs != null ? GameSession.gs.playerMember : null;
            if (player != null)
            {
                int baseTime = determineSpeedCoefficient(player.playerStats.baseSpeed);
                int adjusted = Mathf.CeilToInt(baseTime * player.getSpeedTimeMultiplier());
                return adjusted > 0 ? adjusted : 1;
            }
        }
        else
        {
            var enemy = GameSession.gs != null ? GameSession.gs.enemyMember : null;
            if (enemy != null)
            {
                int baseTime = determineSpeedCoefficient(enemy.enemyStats.baseSpeed);
                int adjusted = Mathf.CeilToInt(baseTime * enemy.getSpeedTimeMultiplier());
                return adjusted > 0 ? adjusted : 1;
            }
        }

        return 1;
    }

    void Start()
    {
        if (GameSession.gs == null || GameSession.gs.playerMember == null || GameSession.gs.enemyMember == null) return;
        if (playerAction == null || enemyAction == null) return;

        int playerTime = getActorTime(0);
        int enemyTime = getActorTime(1);

        turnOrder.insert(new TurnEntry { time = playerTime, actor = 0 });
        turnOrder.insert(new TurnEntry { time = enemyTime, actor = 1 });


        playerAction.addTriggers();
        enemyAction.addTriggers();

        playerAction.playerTurnEnd += continueGameFlow;
        enemyAction.enemyTurnEnd += continueGameFlow;

        continueGameFlow();
    }

    void OnDisable()
    {
        if (playerAction != null)
        {
            playerAction.playerTurnEnd -= continueGameFlow;
        }
        if (enemyAction != null)
        {
            enemyAction.enemyTurnEnd -= continueGameFlow;
        }
    }


    private void cleanupTriggers()
    {
        if (playerAction != null)
        {
            playerAction.removeTriggers();
        }
        if (enemyAction != null)
        {
            enemyAction.removeTriggers();
        }
    }


    // gets called when the player action or enemy action is done their turn
    private void continueGameFlow()
    {
        if (battleEnded)
        {
            return;
        }

        if (GameSession.gs == null || GameSession.gs.playerMember == null || GameSession.gs.enemyMember == null)
        {
            EndBattle();
            return;
        }

        if (GameSession.gs.playerMember.currentHealth <= 0 || GameSession.gs.enemyMember.currentHealth <= 0)
        {
            EndBattle();
            return;
        }


        bool currentActorHasIcons = false;

        if (currentTurnActor == 0)
        {
            currentActorHasIcons = GameSession.gs.playerMember.hasPushTurnIcons();
        }
        else if (currentTurnActor == 1)
        {
            currentActorHasIcons = GameSession.gs.enemyMember.hasPushTurnIcons();
        }

        if (currentActorHasIcons)
        {
            turnChanged?.Invoke(currentTurnActor);
            return;
        }


        if (turnOrder.Count == 0)
        {

            int playerTime = getActorTime(0);
            int enemyTime = getActorTime(1);
            turnOrder.insert(new TurnEntry { time = playerTime, actor = 0 });
            turnOrder.insert(new TurnEntry { time = enemyTime, actor = 1 });
        }

        var next = turnOrder.remove();
        int time = next.time;
        int actor = next.actor;

        currentTurnActor = actor;

        if (actor == 0)
        {
            GameSession.gs.playerMember.initializePushTurnIcons();


            var pState = GameSession.gs.playerMember;
            if (pState.pendingBonusTurnIcons > 0)
            {
                int bonus = pState.pendingBonusTurnIcons;
                pState.addPushTurnHalves(bonus * 2);
                pState.pendingBonusTurnIcons = 0;
            }
        }
        else
        {
            GameSession.gs.enemyMember.initializePushTurnIcons();
        }


        turnOrder.insert(new TurnEntry { time = time + getActorTime(actor), actor = actor });

        turnChanged?.Invoke(actor);


        PrintTurnInfo(actor);
    }

    // basic debug function to pring to the logs to see character statistics
    private void PrintTurnInfo(int turnOwner)
    {
        if (GameSession.gs == null || GameSession.gs.playerMember == null || GameSession.gs.enemyMember == null)
            return;

        PlayerState player = GameSession.gs.playerMember;
        EnemyState enemy = GameSession.gs.enemyMember;
        string turnOwnerName = turnOwner == 0 ? "PLAYER" : "ENEMY";

        Debug.Log($"═══ {turnOwnerName} TURN ═══");


        string playerInfo = $"PLAYER: HP {player.currentHealth:F0}/{player.playerStats.baseMaxHealth}";
        if (player.currentShield > 0)
            playerInfo += $" | Shield {player.currentShield:F0}";
        playerInfo += $" | Mana {player.currentMana}/{PlayerState.MAX_MANA} | Icons {player.pushTurnHalves / 2}.{player.pushTurnHalves % 2}";
        if (player.currentBurnStacks > 0)
            playerInfo += $" | Burn {player.currentBurnStacks}({player.burnTurnsRemaining})";
        if (player.currentSlowStacks > 0)
            playerInfo += $" | Slow {player.currentSlowStacks}({player.slowTurnsRemaining})";
        playerInfo += $" | ATK {player.currentAttackDamage} DEF {player.currentDefense} AP {player.currentAbilityPower}";
        Debug.Log(playerInfo);


        string enemyInfo = $"ENEMY: HP {enemy.currentHealth:F0}/{enemy.enemyStats.baseMaxHealth} | Icons {enemy.pushTurnHalves / 2}.{enemy.pushTurnHalves % 2}";
        if (enemy.currentBurnStacks > 0)
            enemyInfo += $" | Burn {enemy.currentBurnStacks}({enemy.burnTurnsRemaining})";
        if (enemy.currentSlowStacks > 0)
            enemyInfo += $" | Slow {enemy.currentSlowStacks}({enemy.slowTurnsRemaining})";
        if (enemy.currentDamageDebuffStacks > 0)
            enemyInfo += $" | DmgDebuff {enemy.currentDamageDebuffStacks}({enemy.damageDebuffTurnsRemaining})";
        enemyInfo += $" | ATK {enemy.enemyStats.baseAttackDamage} DEF {enemy.enemyStats.baseDefense} AP {enemy.enemyStats.baseAbilityPower}";
        Debug.Log(enemyInfo);

        if (GameSession.gs.currentGold > 0)

            Debug.Log($"Gold: {GameSession.gs.currentGold}");
    }


    private void EndBattle()
    {
        if (battleEnded)
        {
            return;
        }

        battleEnded = true;

        currentState = BattleState.gameOver;


        if (GameSession.gs != null && GameSession.gs.enemyMember != null && GameSession.gs.enemyMember.currentHealth <= 0)
        {
            int goldReward = GameSession.gs.enemyMember.enemyStats.goldDropAmount;
            GameSession.gs.AddGold(goldReward);

            cleanupTriggers();


            StartCoroutine(LoadShopAfterDelay());
        }
        else if (GameSession.gs != null && GameSession.gs.playerMember != null && GameSession.gs.playerMember.currentHealth <= 0)
        {

            cleanupTriggers();
            SceneManager.LoadScene("Lost");
        }
    }

    private IEnumerator LoadShopAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene("Shop");
    }


}

