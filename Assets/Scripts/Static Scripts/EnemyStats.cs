using System;
using System.Collections.Generic;
using UnityEngine;
using battleEnum;

// Serializable hit data for attack patterns
[System.Serializable]
public class AttackHitInfo
{
    public float windupSeconds = 0.25f;
    public float parryWindowSeconds = 0.18f;
    public bool parryable = true;
}

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

    [Header("Gold Rewards")]
    public int goldDropAmount = 10;

    [Header("Elemental Affinities")]
    public List<Element> weaknesses = new List<Element>(); // 1.5x damage taken
    public List<Element> resistances = new List<Element>(); // 0.5x damage taken

    // Attack Patterns
    [System.Serializable]
    public class AttackPattern
    {
        public string attackName = "BasicAttack";
        public Element element = Element.None;
        public bool isParryable = true;
        public List<AttackHitInfo> hits = new List<AttackHitInfo>();
    }

    public List<AttackPattern> attackPatterns = new List<AttackPattern>();

    // Instantiate an AttackData from a stored attack pattern
    public AttackData InstantiateAttack(AttackPattern pattern, float totalAttackDamage)
    {
        var attackData = new AttackData
        {
            attackId = pattern.attackName,
            element = pattern.element,
            isParryable = pattern.isParryable,
            hits = new List<AttackHitData>()
        };

        // Divide total attack damage across all hits
        float damagePerHit = totalAttackDamage / Mathf.Max(1f, pattern.hits.Count);

        foreach (var hit in pattern.hits)
        {
            attackData.hits.Add(new AttackHitData
            {
                baseDamage = damagePerHit,
                parryable = hit.parryable,
                windupSeconds = hit.windupSeconds,
                parryWindowSeconds = hit.parryWindowSeconds
            });
        }

        return attackData;
    }

    // make one for animations
}