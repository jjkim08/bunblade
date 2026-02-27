using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

// meant to store the players baseline statistic values which are not changed throughout the game, these are used to initialize the player state at the start of combat
[CreateAssetMenu(menuName = "Player Stats")]
public class PlayerStats : ScriptableObject
{
    public int id;
    public int baseMaxHealth;
    public int baseAttackDamage;
    public int baseAbilityPower;
    public int baseDefense;

    public int baseSpeed;
    public int baseluck;

    [Header("Elemental Affinities")]
    public List<Element> weaknesses = new List<Element>();
    public List<Element> resistances = new List<Element>();

    public int maxMana = 10;
    public List<Spell> serializableSpellInfo;
    public Dictionary<string, Spell> spellInfo;

    
    private void OnEnable()
    {
        spellInfo = new Dictionary<string, Spell>();
        for (int i = 0; i < serializableSpellInfo.Count; i++)
        {
            spellInfo.Add(serializableSpellInfo[i].name, serializableSpellInfo[i]);
        }

    }
}