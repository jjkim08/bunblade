using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Profiling;
using System.Runtime.InteropServices;
using battleEnum;

// handles enemy logic, this is the static baseline statistics updated through the game
public class EnemyState
{

    // current values of integers
    public float currentHealth;
    public event Action<float> onHealthChanged;


    public int pushTurnHalves = 0;
    public event Action<int> onPushTurnHalvesChanged;


    public int currentBurnStacks = 0;
    public int burnTurnsRemaining = 0;


    public int currentSlowStacks = 0;
    public int slowTurnsRemaining = 0;


    public int currentDamageDebuffStacks = 0;
    public int damageDebuffTurnsRemaining = 0;

    public EnemyStats enemyStats;


    public EnemyState(EnemyStats stats)
    {
        enemyStats = stats;
    }

    // elemental debuff and turn cost icon calculations
    private float damageReduction(int defe)
    {
        return (float)(95.0 * (1.0 - Math.Pow(Math.E, -0.02f * defe)));
    }

    
    private float elementalMultiplier(Element attackElement)
    {
        if (attackElement == Element.None) return 1f;
        if (enemyStats.weaknesses.Contains(attackElement)) return 1.5f;
        if (enemyStats.resistances.Contains(attackElement)) return 0.5f;
        return 1f;
    }

    
    public int calculateIconCost(Element attackElement)
    {
        PlayerState player = GameSession.gs != null ? GameSession.gs.playerMember : null;
        if (player != null && player.playerStats != null && attackElement != Element.None && player.playerStats.weaknesses.Contains(attackElement)) return 1;
        if (player != null && player.playerStats != null && attackElement != Element.None && player.playerStats.resistances.Contains(attackElement)) return 3;
        return 2;
    }

    
    public void takeDamage(float damage, Element element)
    {
        damage *= elementalMultiplier(element);

        currentHealth -= (float)(damage * 0.01f * (100f - damageReduction(enemyStats.baseDefense)));
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        onHealthChanged?.Invoke((float)currentHealth / (float)enemyStats.baseMaxHealth);

    }

    public void takeSlow(int slowStacks)
    {
        currentSlowStacks += slowStacks;
        slowTurnsRemaining = 2;
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


    public void takeDamageDebuff(int debuffStacks)
    {
        currentDamageDebuffStacks += debuffStacks;
        damageDebuffTurnsRemaining = 2;
    }

    
    private float getDamageDebuffMultiplier()
    {
        if (currentDamageDebuffStacks <= 0) return 1f;
        float basePerStack = 0.10f;
        float k = 0.5f;
        float effectiveReduction = basePerStack * currentDamageDebuffStacks / (1f + k * currentDamageDebuffStacks);
        float multiplier = 1f - effectiveReduction;
        return Mathf.Clamp(multiplier, 0.4f, 1f);
    }

    
    public void tickDamageDebuffDuration()
    {
        if (damageDebuffTurnsRemaining > 0)
        {
            damageDebuffTurnsRemaining--;
            if (damageDebuffTurnsRemaining <= 0)
            {
                currentDamageDebuffStacks = 0;
            }
        }
    }

    // basic attack damage calculation, includes randomness and debuffs but not buffs or player defense
    public float calculateAttack()
    {
        float totalDamage = enemyStats.baseAttackDamage;

        if (UnityEngine.Random.value < enemyStats.baseluck * 0.01f)
        {
            totalDamage *= (float)1.5;
        }


        float damageMultiplier = getDamageDebuffMultiplier();
        totalDamage *= damageMultiplier;

        return totalDamage;
    }


    // takes burn damage
    public void takeBurn(int burnStacks)
    {
        currentBurnStacks += burnStacks;
        burnTurnsRemaining = 3;
    }

    public float calculateBurnDamage()
    {
        if (currentBurnStacks <= 0) return 0f;
        float baseDamage = (float)(enemyStats.baseMaxHealth * currentBurnStacks * 0.005);
        return baseDamage * elementalMultiplier(Element.Fire);
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

    public void updateHealth()
    {
        onHealthChanged?.Invoke((float)currentHealth / (float)enemyStats.baseMaxHealth);
    }

    public void initializePushTurnIcons()
    {
        pushTurnHalves = 6;
        onPushTurnHalvesChanged?.Invoke(pushTurnHalves);
    }

    public bool hasPushTurnIcons()
    {
        return pushTurnHalves > 0;
    }

    // push turn icons work in halves
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