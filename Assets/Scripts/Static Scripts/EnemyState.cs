using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Profiling;
using System.Runtime.InteropServices;
using battleEnum;

public class EnemyState
{

    // a enemy has (each party member will have their own EnemyState component)
    // stats
    // animations

    // actively 
    // currentHealth
    // onHealthChanged

    public float currentHealth;
    public event Action<float> onHealthChanged;

    // Push Turn Icon System (halves only, 2 halves = 1 full)
    public int pushTurnHalves = 0;
    public event Action<int> onPushTurnHalvesChanged;

    // Burn tracking: total stacks and remaining turns
    public int currentBurnStacks = 0;
    public int burnTurnsRemaining = 0;

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

    private float elementalMultiplier(Element attackElement)
    {
        if (attackElement == Element.None) return 1f;
        if (enemyStats.weaknesses.Contains(attackElement)) return 1.5f;
        if (enemyStats.resistances.Contains(attackElement)) return 0.5f;
        return 1f;
    }

    public int calculateIconCost(Element attackElement)
    {
        PlayerState player = GameSession.gs.playerMember;
        if (attackElement != Element.None && player.playerStats.weaknesses.Contains(attackElement)) return 1;
        if (attackElement != Element.None && player.playerStats.resistances.Contains(attackElement)) return 3;
        return 2;
    }

    public void takeDamage(float damage, Element element)
    {
        damage *= elementalMultiplier(element);

        currentHealth -= (float)(damage * 0.01f * (100f - damageReduction(enemyStats.baseDefense)));
        onHealthChanged?.Invoke((float)currentHealth / (float)enemyStats.baseMaxHealth);

    }

    public void takeSlow(int slowStacks)
    {
        currentSlowStacks += slowStacks;
        slowTurnsRemaining = 2; // refresh to 2 enemy turns
    }

    public float getSpeedTimeMultiplier()
    {
        if (currentSlowStacks <= 0) return 1f;
        float basePerStack = 0.10f;
        float k = 0.5f;
        float effectiveSlow = basePerStack * currentSlowStacks / (1f + k * currentSlowStacks);

        // base stacks * current stacks / (1 + k * current stacks) for diminishing returns
        // desmos link: https://www.desmos.com/calculator/9ks9wf2avn

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
    public void takeBurn(int burnStacks)
    {
        currentBurnStacks += burnStacks;
        burnTurnsRemaining = 3; // refresh to 3 enemy turns
    }

    public float calculateBurnDamage()
    {
        if (currentBurnStacks <= 0) return 0f;
        float baseDamage = (float)(enemyStats.baseMaxHealth * currentBurnStacks * 0.005); // 0.5% max health per stack
        return baseDamage * elementalMultiplier(Element.Fire); // apply elemental multiplier
    }

    public void tickBurnDuration()
    {
        if (burnTurnsRemaining > 0)
        {
            burnTurnsRemaining--;
            if (burnTurnsRemaining <= 0)
            {
                currentBurnStacks = 0; // clear stacks when duration expires
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

    public void consumePushTurnIcons(int iconCost)
    {
        if (iconCost <= 0) return;

        pushTurnHalves -= iconCost;

        onPushTurnHalvesChanged?.Invoke(pushTurnHalves);
    }
}