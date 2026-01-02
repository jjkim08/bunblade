using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Global/Player Data")]
public class GlobalPlayerData : ScriptableObject
{
    [Header("Health")]
    public float maxHealth = 10f;
    public float currentHealth = 10f;

    [Header("Inventory")] // Placeholder for platformer-to-battle item sync
    public List<string> items = new List<string>();
}
