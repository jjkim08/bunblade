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

    public int currentMana;

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

    public void takeDamage(float damage)
    {
        // shields absorb 100% damage but aren't affected by damage reduction, ex 30 damage will deal 30 damage to shield
        // todo: make a shield bar

        if (currentShield >= 0)
        {
            currentShield -= damage;
            if (currentShield < 0)
            {
                damage = -currentShield; // remaining damage
                currentShield = 0;
            } else
            {
                damage = 0;
            }
        }

        currentHealth -= (int)(damage * 0.01 * (100 - damageReduction(playerStats.baseDefense)));
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

    public int calculateManaGainOnAttack()
    {
        return 1; // gains 1 mana per attack
    }

    public int calculateManaGainPassive()
    {
        return 1; // gains 1 mana at the start of their turn
    }

    public void updateHealth()
    {
        onHealthChanged?.Invoke((float)currentHealth / (float)playerStats.baseMaxHealth);
    }
}