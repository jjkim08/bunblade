using System;
using System.Collections.Generic;
using UnityEngine;
using battleEnum;

// the baseline stats for an enemy which is not modified throughout the game, consists of starting values
[System.Serializable]
public class AttackHitInfo
{
    public float windupSeconds = 0.25f;
    public float parryWindowSeconds = 0.18f;
    public bool parryable = true;
    public float downtimeSeconds = 0.5f;
}

[CreateAssetMenu(menuName = "Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    public int id;
    public int baseMaxHealth;
    public int baseAttackDamage;
    public int baseAbilityPower;
    public int baseDefense;

    public int baseSpeed;
    public int baseluck;

    [Header("Gold Rewards")]
    public int goldDropAmount = 10;

    [Header("Elemental Affinities")]
    public List<Element> weaknesses = new List<Element>();
    public List<Element> resistances = new List<Element>();


    [System.Serializable] // attack patterns are changed in the inspector where there are timings to land a parry, so they need to be serializable
    public class AttackPattern
    {
        public string attackName = "BasicAttack";
        public Element element = Element.None;
        public bool isParryable = true;
        public List<AttackHitInfo> hits = new List<AttackHitInfo>();
    }

    public List<AttackPattern> attackPatterns = new List<AttackPattern>();


    
    public AttackData InstantiateAttack(AttackPattern pattern, float totalAttackDamage)
    {
        var attackData = new AttackData
        {
            attackId = pattern.attackName,
            element = pattern.element,
            isParryable = pattern.isParryable,
            hits = new List<AttackHitData>()
        };


        float damagePerHit = totalAttackDamage / Mathf.Max(1f, pattern.hits.Count);

        foreach (var hit in pattern.hits)
        {
            attackData.hits.Add(new AttackHitData
            {
                baseDamage = damagePerHit,
                parryable = hit.parryable,
                windupSeconds = hit.windupSeconds,
                parryWindowSeconds = hit.parryWindowSeconds,
                downtimeSeconds = hit.downtimeSeconds
            });
        }

        return attackData;
    }


}