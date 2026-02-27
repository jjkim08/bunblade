using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Spell Stats")]
public class Spell : ScriptableObject
{
    public string spellName;
    public string description;

    public int manaCost;
    public Element element = Element.None;

    public int appliedStacks;

    public float multiplier;

    public Spell(string sN, string d, int mC, float m)
    {
        spellName = sN;
        description = d;
        manaCost = mC;
        multiplier = m;
    }
}