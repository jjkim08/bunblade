using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    public int id;
    public int baseMaxHealth;
    public int baseAttackDamage;
    public int baseAbilityPower;
    public int baseDefense; // defensive scaling will be an equation, x is the amount of defense, y is the % blocked
    // more on it here https://www.desmos.com/calculator/0mxuknlzr1
    public int baseSpeed;
    public int baseluck; // luck = crit chance

    [Header("Elemental Affinities")]
    public List<Element> weaknesses = new List<Element>(); // 1.5x damage taken
    public List<Element> resistances = new List<Element>(); // 0.5x damage taken

    // make one for animations
}