using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;



[CreateAssetMenu(menuName = "Player Stats")]
public class PlayerStats : ScriptableObject
{
    public int id;
    public int baseMaxHealth;
    public int baseAttackDamage;
    public int baseAbilityPower;
    public int baseDefense; // defensive scaling will be an equation, x is the amount of defense, y is the % blocked
    // more on it here https://www.desmos.com/calculator/0mxuknlzr1
    public int baseSpeed;
    public int baseluck; // luck = crit chance

    public int maxMana = 10;
    public List<Spell> serializableSpellInfo;
    public Dictionary<string, Spell> spellInfo;

    private void OnEnable()
    {
        spellInfo = new Dictionary<string, Spell>();
        for (int i = 0; i < serializableSpellInfo.Count; i++)
        {
            spellInfo.Add(serializableSpellInfo[i].name, serializableSpellInfo[i]);
        } // added comment
        
    }
}