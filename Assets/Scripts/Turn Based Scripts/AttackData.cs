using System.Collections.Generic;
using battleEnum;

// this file contains the data structures for attacks and their resolution, this is used to store the data for attacks and to resolve them when they are executed
public class AttackHitData
{
    public float baseDamage;
    public bool parryable = true;
    public float windupSeconds = 0.3f;
    public float parryWindowSeconds = 0.2f;
    public float downtimeSeconds = 0.5f;
}

public class AttackData
{
    public string attackId;
    public Element element = Element.None;
    public List<AttackHitData> hits = new List<AttackHitData>();
    public bool isParryable = true;

    public int iconCostHalves = 0;
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

