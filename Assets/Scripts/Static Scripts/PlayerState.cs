using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Profiling;

// similar to enemy state holds the current active values for the player throughout combat
public class PlayerState


{


    public float currentHealth;
    public float currentShield = 0;
    public event Action<float> onHealthChanged;

    public const int MAX_MANA = 10;
    public int currentMana = 0;
    public event Action<int, int> onManaChanged;


    public int currentAttackDamage;
    public int currentAbilityPower;
    public int currentDefense;


    public int pushTurnHalves = 0;
    public event Action<int> onPushTurnHalvesChanged;

    public int pendingBonusTurnIcons = 0;


    public int currentBurnStacks = 0;
    public int burnTurnsRemaining = 0;


    public int currentSlowStacks = 0;
    public int slowTurnsRemaining = 0;

    public PlayerStats playerStats;


    public PlayerState(PlayerStats stats)
    {
        playerStats = stats;


        currentAttackDamage = stats.baseAttackDamage;
        currentAbilityPower = stats.baseAbilityPower;
        currentDefense = stats.baseDefense;
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


        if (currentShield > 0)
        {
            currentShield -= damage;
            if (currentShield < 0)
            {
                damage = -currentShield;
                currentShield = 0;
            }
            else
            {
                damage = 0;
            }
        }

        currentHealth -= (int)(damage * 0.01 * (100 - damageReduction(currentDefense)));
        if (currentHealth <= 0)
        {
            currentHealth = 0;
        }
        onHealthChanged?.Invoke((float)currentHealth / (float)playerStats.baseMaxHealth);
    }


    public float calculateBasicAttack()
    {
        float totalDamage = currentAttackDamage;

        if (UnityEngine.Random.value < playerStats.baseluck * 0.01f)
        {
            totalDamage *= (float)1.5;
        }

        return totalDamage;
    }

    public float calculateSpellAttack(string spellName)
    {
        float totalDamage = currentAbilityPower;

        totalDamage *= playerStats.spellInfo[spellName].multiplier;

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
        currentShield += shieldAmount;
    }

    public int calculateShieldAmount()
    {
        return playerStats.baseAbilityPower;
    }

    public int calculateHealAmount()
    {
        return (int)(playerStats.baseAbilityPower * 1.5f);
    }



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

    // mana usage for handling spells
    public void consumeMana(int amount)
    {
        UnityEngine.Debug.Log($"Consuming {amount} mana. Current: {currentMana}");
        currentMana -= amount;
        if (currentMana < 0) currentMana = 0;
        UnityEngine.Debug.Log($"After consumption: {currentMana}");
        onManaChanged?.Invoke(currentMana, MAX_MANA);
    }

    public int calculateManaGainPassive()
    {
        return 0;
    }


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


    public void takeSlow(int slowStacks)
    {
        currentSlowStacks += slowStacks;
        slowTurnsRemaining = 3;
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
                currentSlowStacks = 0;
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