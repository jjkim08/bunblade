using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Profiling;

public class PlayerState // essentially, i want this class to be a getter class only
// it will hold the stats, spells, and animations of a player
// but it will not directly interfere with the game, ex it won't invoke damage to enemy, instead it will give calculations
{

    // a player has (each party member will have their own PlayerState component)
    // stats
    // spells
    // animations

    // actively 
    // currentHealth
    // onHealthChanged

    public float currentHealth;
    public float currentShield = 0;
    public event Action<float> onHealthChanged;

    public const int MAX_MANA = 10;
    public int currentMana = 0;
    public event Action<int, int> onManaChanged; // (current, max)

    // Push Turn Icon System (halves only, 2 halves = 1 full)
    public int pushTurnHalves = 0;
    public event Action<int> onPushTurnHalvesChanged;
    // Bonus full icons awarded (e.g., from perfect parries) to apply next player turn
    public int pendingBonusTurnIcons = 0;

    // Burn tracking: total stacks and remaining turns
    public int currentBurnStacks = 0;
    public int burnTurnsRemaining = 0;


    // Slow tracking: total stacks and remaining turns
    public int currentSlowStacks = 0;
    public int slowTurnsRemaining = 0;

    public PlayerStats playerStats;

    // make one for animations

    public PlayerState(PlayerStats stats)
    {
        playerStats = stats;
    }


    private float damageReduction(int defe)
    {
        return (float)(95.0 * (1.0 - Math.Pow(Math.E, -0.02f * defe)));
    }

    private float getElementalMultiplier(Element attackElement)
    {
        if (attackElement == Element.None) return 1f;
        if (playerStats.weaknesses.Contains(attackElement)) return 1.5f;
        if (playerStats.resistances.Contains(attackElement)) return 0.5f;
        return 1f;
    }

    public int calculateIconCost(Element attackElement)
    {
        EnemyState enemy = GameSession.gs != null ? GameSession.gs.enemyMember : null;
        if (enemy != null && enemy.enemyStats != null && attackElement != Element.None && enemy.enemyStats.weaknesses.Contains(attackElement)) return 1;
        if (enemy != null && enemy.enemyStats != null && attackElement != Element.None && enemy.enemyStats.resistances.Contains(attackElement)) return 3;
        return 2;
    }

    public void takeDamage(float damage, Element element = Element.None)
    {
        damage *= getElementalMultiplier(element);

        // shields absorb 100% damage but aren't affected by damage reduction, ex 30 damage will deal 30 damage to shield
        // todo: make a shield bar

        if (currentShield > 0)
        {
            currentShield -= damage;
            if (currentShield < 0)
            {
                damage = -currentShield; // remaining damage
                currentShield = 0;
            }
            else
            {
                damage = 0;
            }
        }

        currentHealth -= (int)(damage * 0.01 * (100 - damageReduction(playerStats.baseDefense)));
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        onHealthChanged?.Invoke((float)currentHealth / (float)playerStats.baseMaxHealth);
    }

    public float calculateBasicAttack()
    {
        float totalDamage = playerStats.baseAttackDamage;

        if (UnityEngine.Random.value < playerStats.baseluck * 0.01f)
        {
            totalDamage *= (float)1.5;
        }

        return totalDamage;
    }

    public float calculateSpellAttack(string spellName)
    {
        float totalDamage = playerStats.baseAbilityPower;

        totalDamage *= playerStats.spellInfo[spellName].multiplier; // float value

        return totalDamage;
    }

    public void heal(int healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > playerStats.baseMaxHealth)
        {
            currentHealth = playerStats.baseMaxHealth;
        }
        onHealthChanged?.Invoke((float)currentHealth / (float)playerStats.baseMaxHealth);
    }

    public void giveShield(int shieldAmount)
    {
        currentShield += shieldAmount; // shield can stack infinitely
    }

    public int calculateShieldAmount()
    {
        return playerStats.baseAbilityPower; // shields by ability power
    }

    public int calculateHealAmount()
    {
        return (int)(playerStats.baseAbilityPower * 1.5f); // heals ability power * 1.5 health
    }

    // Mana system
    public void gainMana(int amount)
    {
        currentMana += amount;
        if (currentMana > MAX_MANA)
        {
            currentMana = MAX_MANA;
        }
        onManaChanged?.Invoke(currentMana, MAX_MANA);
    }

    public bool canCastSpell(string spellName)
    {
        if (!playerStats.spellInfo.ContainsKey(spellName)) return false;
        int manaCost = playerStats.spellInfo[spellName].manaCost;
        return currentMana >= manaCost;
    }

    public void consumeMana(int amount)
    {
        currentMana -= amount;
        if (currentMana < 0) currentMana = 0;
        onManaChanged?.Invoke(currentMana, MAX_MANA);
    }

    public int calculateManaGainPassive()
    {
        return 0;
    }

    // Burn system: adds stacks and refreshes duration to 3 turns
    public void takeBurn(int burnStacks)
    {
        currentBurnStacks += burnStacks;
        burnTurnsRemaining = 3;
    }

    public float calculateBurnDamage()
    {
        if (currentBurnStacks <= 0) return 0f;
        float baseDamage = (float)(playerStats.baseMaxHealth * currentBurnStacks * 0.005);
        return baseDamage * getElementalMultiplier(Element.Fire);
    }

    public void tickBurnDuration()
    {
        if (burnTurnsRemaining > 0)
        {
            burnTurnsRemaining--;
            if (burnTurnsRemaining <= 0)
            {
                currentBurnStacks = 0;
            }
        }
    }

    // Slow system: adds stacks and refreshes duration to 3 turns
    public void takeSlow(int slowStacks)
    {
        currentSlowStacks += slowStacks;
        slowTurnsRemaining = 3; // refresh to 3 player turns
    }

    public float getSpeedTimeMultiplier()
    {
        if (currentSlowStacks <= 0) return 1f;
        float basePerStack = 0.10f;
        float k = 0.5f;
        float effectiveSlow = basePerStack * currentSlowStacks / (1f + k * currentSlowStacks);
        return 1f + effectiveSlow;
    }

    public void tickSlowDuration()
    {
        if (slowTurnsRemaining > 0)
        {
            slowTurnsRemaining--;
            if (slowTurnsRemaining <= 0)
            {
                currentSlowStacks = 0; // clear stacks when duration expires
            }
        }
    }

    public void updateHealth()
    {
        onHealthChanged?.Invoke((float)currentHealth / (float)playerStats.baseMaxHealth);
    }

    public void initializePushTurnIcons()
    {
        pushTurnHalves = 6;
        onPushTurnHalvesChanged?.Invoke(pushTurnHalves);
    }

    public void addPushTurnHalves(int amount)
    {
        if (amount == 0) return;
        pushTurnHalves += amount;
        if (pushTurnHalves < 0)
        {
            pushTurnHalves = 0;
        }
        onPushTurnHalvesChanged?.Invoke(pushTurnHalves);
    }

    public bool hasPushTurnIcons()
    {
        return pushTurnHalves > 0;
    }

    public void consumePushTurnIcons(int iconCost)
    {
        if (iconCost <= 0) return;

        pushTurnHalves -= iconCost;
        if (pushTurnHalves < 0)
        {
            pushTurnHalves = 0;
        }

        onPushTurnHalvesChanged?.Invoke(pushTurnHalves);
    }
}