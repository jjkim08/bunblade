using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Profiling;
using System.Runtime.InteropServices;

public class EnemyState
{

    // a enemy has (each party member will have their own EnemyState component)
    // stats
    // animations

    // actively 
    // currentHealth
    // onHealthChanged

    public float currentHealth;
    public int currentShield = 0;
    public event Action<float> onHealthChanged;

    // Push Turn Icon System (halves only, 2 halves = 1 full)
    public int pushTurnHalves = 0;
    public event Action<int> onPushTurnHalvesChanged;

    // Burn tracking: total stacks and remaining turns
    public int currentBurnStacks = 0;
    public int burnTurnsRemaining = 0;
    public Element burnElement = Element.None; // track element of burn for damage calculation

    // Slow tracking: total stacks and remaining turns
    public int currentSlowStacks = 0;
    public int slowTurnsRemaining = 0;

    public EnemyStats enemyStats;

    // make one for animations

    public EnemyState(EnemyStats stats)
    {
        enemyStats = stats;
    }


    private float damageReduction(int defe)
    {
        return (float)(95.0 * (1.0 - Math.Pow(Math.E, -0.02f * defe)));
    }

    private float GetElementalMultiplier(Element attackElement)
    {
        if (attackElement == Element.None) return 1f;
        if (enemyStats.weaknesses.Contains(attackElement)) return 1.5f;
        if (enemyStats.resistances.Contains(attackElement)) return 0.5f;
        return 1f;
    }

    public int CalculateIconCost(Element attackElement)
    {
        // Costs measured in halves: neutral=2, weakness=1, resistance=3
        PlayerState player = GameSession.gs.playerMember;
        if (attackElement != Element.None && player.playerStats.weaknesses.Contains(attackElement)) return 1;
        if (attackElement != Element.None && player.playerStats.resistances.Contains(attackElement)) return 3;
        return 2; // neutral or no element
    }

    public void takeDamage(float damage, Element element = Element.None)
    {
        // Apply elemental multiplier
        damage *= GetElementalMultiplier(element);

        // shields absorb 100% damage but aren't affected by damage reduction, ex 30 damage will deal 30 damage to shield
        // todo: make a shield bar

        currentHealth -= (float)(damage * 0.01f * (100f - damageReduction(enemyStats.baseDefense)));
        onHealthChanged?.Invoke((float)currentHealth / (float)enemyStats.baseMaxHealth);

        Debug.Log("Enemy " + enemyStats.id + " took " + damage + " damage." +
            " Current Health: " + currentHealth + "/" + enemyStats.baseMaxHealth);
    }

    public void takeSlow(int slowStacks)
    {
        currentSlowStacks += slowStacks;
        slowTurnsRemaining = 3; // refresh to 3 enemy turns
    }

    public float GetSpeedTimeMultiplier()
    {
        if (currentSlowStacks <= 0) return 1f;
        // Hyperbolic diminishing returns: base*s / (1 + k*s)
        float basePerStack = 0.10f;
        float k = 0.5f;
        float effectiveSlow = basePerStack * currentSlowStacks / (1f + k * currentSlowStacks);
        return 1f + effectiveSlow; // multiply base turn time by this
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

    public float calculateAttack()
    {
        float totalDamage = enemyStats.baseAttackDamage;

        if (UnityEngine.Random.value < enemyStats.baseluck * 0.01f)
        {
            totalDamage *= (float)1.5;
        }

        return totalDamage;
    }

    // Burn system: adds stacks and refreshes duration to 3 turns
    public void takeBurn(int burnStacks, Element element = Element.Fire)
    {
        currentBurnStacks += burnStacks;
        burnTurnsRemaining = 3; // refresh to 3 enemy turns
        burnElement = element; // track the element for damage calculation
    }

    public float calculateBurnDamage()
    {
        if (currentBurnStacks <= 0) return 0f;
        float baseDamage = (float)(enemyStats.baseMaxHealth * currentBurnStacks * 0.005); // 0.5% max health per stack
        return baseDamage * GetElementalMultiplier(burnElement); // apply elemental multiplier
    }

    public void tickBurnDuration()
    {
        if (burnTurnsRemaining > 0)
        {
            burnTurnsRemaining--;
            if (burnTurnsRemaining <= 0)
            {
                currentBurnStacks = 0; // clear stacks when duration expires
                burnElement = Element.None; // clear element
            }
        }
    }

    public void updateHealth()
    {
        onHealthChanged?.Invoke((float)currentHealth / (float)enemyStats.baseMaxHealth);
    }

    // Push Turn Icon System Methods (halves)
    public void InitializePushTurnIcons()
    {
        pushTurnHalves = 6; // 6 halves = 3 full equivalents
        onPushTurnHalvesChanged?.Invoke(pushTurnHalves);
    }

    public bool HasPushTurnIcons()
    {
        return pushTurnHalves > 0;
    }

    public void ConsumePushTurnIcons(int iconCost)
    {
        if (iconCost <= 0) return;

        pushTurnHalves -= iconCost;

        // Allow negative to signal depletion; GameFlow checks HasPushTurnIcons on next loop.
        onPushTurnHalvesChanged?.Invoke(pushTurnHalves);
    }
}