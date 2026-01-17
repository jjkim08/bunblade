using System.Collections.Generic;
using battleEnum;

// Data structures representing enemy attacks and parry outcomes

public class AttackHitData
{
    public float baseDamage;
    public bool parryable = true;
    public float windupSeconds = 0.3f; // time before the window opens
    public float parryWindowSeconds = 0.2f; // active parry window length
    // Optional: explicit hit timestamp after window; here we assume hit lands right after window
}

public class AttackData
{
    public string attackId;
    public Element element = Element.None;
    public List<AttackHitData> hits = new List<AttackHitData>();
    public bool isParryable = true;

    // Optional metadata used during resolution/turn handling
    public int iconCostHalves = 0; // how many halves to consume for this attack
}

public enum ParryResult
{
    None,
    Success,
    Early,
    Late,
    Fail
}

public class ResolvedHit
{
    public float finalDamage;
    public Element element;
    public ParryResult result;
    public float damageMultiplier;
    public bool grantMana;
}

public class AttackResolution
{
    public bool allHitsParried;
    public bool grantFullTurnIconNextTurn;
}
