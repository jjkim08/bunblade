using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Spell Stats")]
public class Spell : ScriptableObject
{
    public string spellName;
    public string description;

    public int manaCost;

    public int appliedStacks; // for DoT or debuffs

    public float multiplier; // how much ability power is multiplied by

    Action spellEffect;

    public Spell(string sN, string d, int mC, float m)
    {
        spellName = sN;
        description = d;
        manaCost = mC;
        multiplier = m;
    }
}