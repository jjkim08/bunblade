using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls parry timing windows and downtime logic.
// Encapsulates input handling (Space key by default) and classifies parry results.
public class ParryController : MonoBehaviour
{
    [Header("Parry Timing")]
    [Tooltip("Cooldown after an early parry attempt (seconds)")]
    public float downtimeSeconds = 0.75f;

    [Tooltip("Extra time after window to classify 'Late' instead of 'Fail' (seconds)")]
    public float postWindowLateThreshold = 0.08f;

    private float downtimeUntil = 0f;
    private bool allHitsParried;

    public bool IsInDowntime => Time.time < downtimeUntil;

    [Header("Parry Visuals (optional)")]
    public SpriteRenderer playerSprite;
    public Color idleColor = Color.white;
    public Color windupColor = new Color(1f, 0.9f, 0f); // soft yellow
    public Color parryWindowColor = Color.green;
    public Color cooldownColor = new Color(1f, 0.3f, 0.3f); // soft red

    private void SetSpriteColor(Color c)
    {
        if (playerSprite != null) playerSprite.color = c;
    }

    // Reset per-attack tracking
    public void BeginAttackSequence(AttackData attack)
    {
        allHitsParried = true;
        SetSpriteColor(idleColor);
    }

    // Coroutine that steps through each hit, opens parry windows, listens for input, and resolves damage multipliers
    public IEnumerator ResolveEnemyAttackWithParries(
        AttackData attack,
        Action<ResolvedHit> onHitResolved,
        Action<AttackResolution> onAttackResolved)
    {
        BeginAttackSequence(attack);

        foreach (var hit in attack.hits)
        {
            // Phase 1: Windup (allow early input detection)
            float t = 0f;
            bool early = false;
            SetSpriteColor(windupColor);
            while (t < hit.windupSeconds)
            {
                if (!early)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        // Early parry attempt: start downtime, classify early
                        early = true;
                        downtimeUntil = Time.time + downtimeSeconds;
                        SetSpriteColor(cooldownColor);
                    }
                }
                t += Time.deltaTime;
                yield return null;
            }

            // Phase 2: Parry window
            bool success = false;
            float windowT = 0f;
            SetSpriteColor(IsInDowntime ? cooldownColor : parryWindowColor);
            while (windowT < hit.parryWindowSeconds)
            {
                if (!success && hit.parryable && !IsInDowntime)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        success = true;
                        SetSpriteColor(parryWindowColor); // keep green for feedback
                    }
                }
                // Inputs during downtime have no effect
                windowT += Time.deltaTime;
                yield return null;
            }

            // Phase 3: After window small interval to classify 'Late'
            bool late = false;
            float lateT = 0f;
            SetSpriteColor(IsInDowntime ? cooldownColor : idleColor);
            while (!success && !early && lateT < postWindowLateThreshold)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    late = true; // pressed but too late
                    break;
                }
                lateT += Time.deltaTime;
                yield return null;
            }

            // Determine result and damage multiplier
            ParryResult result;
            float damageMultiplier = 1f;
            bool grantMana = false;

            if (success)
            {
                result = ParryResult.Success;
                damageMultiplier = 0f; // negate damage
                grantMana = true;      // grant +1 mana on successful parry
            }
            else if (early)
            {
                result = ParryResult.Early; // hit lands normally
                damageMultiplier = IsInDowntime ? 1.25f : 1f; // if downtime still active when hit lands, punish
            }
            else if (late)
            {
                result = ParryResult.Late; // treat as fail for damage
                damageMultiplier = IsInDowntime ? 1.25f : 1f;
            }
            else
            {
                result = ParryResult.Fail;
                damageMultiplier = IsInDowntime ? 1.25f : 1f;
            }

            // Track full-parry state across the move
            if (result != ParryResult.Success)
            {
                allHitsParried = false;
            }

            var resolved = new ResolvedHit
            {
                finalDamage = hit.baseDamage * damageMultiplier,
                element = attack.element,
                result = result,
                damageMultiplier = damageMultiplier,
                grantMana = grantMana
            };

            onHitResolved?.Invoke(resolved);

            // After resolving a hit, revert visuals appropriately
            SetSpriteColor(IsInDowntime ? cooldownColor : idleColor);
        }

        var attackRes = new AttackResolution
        {
            allHitsParried = allHitsParried,
            grantFullTurnIconNextTurn = allHitsParried && attack.isParryable
        };

        onAttackResolved?.Invoke(attackRes);

        // Reset visuals at end of sequence
        SetSpriteColor(idleColor);
    }
}
