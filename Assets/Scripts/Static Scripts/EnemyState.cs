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
    public List<(int, int)> currentBurnStacks = new List<(int, int)>();
    public List<(int, int)> currentSlowStacks = new List<(int, int)>();
    public event Action<float> onHealthChanged;

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

    public void takeDamage(float damage)
    {
        // shields absorb 100% damage but aren't affected by damage reduction, ex 30 damage will deal 30 damage to shield
        // todo: make a shield bar

        currentHealth -= (float)(damage * 0.01f * (100f - damageReduction(enemyStats.baseDefense)));
        onHealthChanged?.Invoke((float)currentHealth / (float)enemyStats.baseMaxHealth);

        Debug.Log("Enemy " + enemyStats.id + " took " + damage + " damage." +
            " Current Health: " + currentHealth + "/" + enemyStats.baseMaxHealth);
    }

    public void takeBurn(int burnStacks)
    {
        currentBurnStacks.Add((burnStacks, 3)); // 3 turns duration
    }

    public int getTotalBurn()
    {
        int totalBurn = 0;
        foreach (var (stacks, duration) in currentBurnStacks)
        {
            totalBurn += stacks;
        }
        return totalBurn;
    }

    public float calculateBurnDamage()
    {
        return (float)(enemyStats.baseMaxHealth * getTotalBurn() * 0.005);
    }

    public void takeSlow(int slowStacks)
    {
        currentSlowStacks.Add((slowStacks, 2)); // 2 turns duration
    }

    public int getTotalSlow()
    {
        int totalSlow = 0;
        foreach (var (stacks, duration) in currentSlowStacks)
        {
            totalSlow += stacks;
        }
        return totalSlow;
    }

    public float calculateSlowModifier()
    {
        return 1f - (float)(getTotalSlow() * 0.01);
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

    public void updateHealth()
    {
        onHealthChanged?.Invoke((float)currentHealth / (float)enemyStats.baseMaxHealth);
    }
}