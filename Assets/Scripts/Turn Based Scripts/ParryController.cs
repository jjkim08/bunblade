using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParryController : MonoBehaviour
{
    public float downtimeSeconds = 0.75f; // time spent after the player parries, to prevent spam

    public float postWindowLateThreshold = 0.08f; // 

    private float downtimeUntil = 0f;
    private bool allHitsParried;

    public bool IsInDowntime => Time.time < downtimeUntil;

    // this part is for testing
    public SpriteRenderer playerSprite;
    
    public Color idleColor = Color.white;
    public Color windupColor = new Color(1f, 0.9f, 0f); // soft yellow
    public Color parryWindowColor = Color.green;
    
    public Color cooldownColor = new Color(1f, 0.3f, 0.3f); // soft red

    private void setSpriteColor(Color c)
    {
        if (playerSprite != null) playerSprite.color = c;
    }

    public IEnumerator parryStages(AttackData attack, Action<ResolvedHit> onHitResolved, Action<AttackResolution> onAttackResolved) {
    
        allHitsParried = true;
        setSpriteColor(idleColor);

        foreach (var hit in attack.hits)
        {
            float t = 0f;
            bool early = false;
            setSpriteColor(windupColor);
            while (t < hit.windupSeconds)
            {
                if (!early)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        early = true;
                        downtimeUntil = Time.time + downtimeSeconds;
                        setSpriteColor(cooldownColor);
                    }
                }
                t += Time.deltaTime;
                yield return null;
            }
            
            bool success = false;
            float windowT = 0f;
            setSpriteColor(IsInDowntime ? cooldownColor : parryWindowColor);
            
            while (windowT < hit.parryWindowSeconds)
            {
                if (!success && hit.parryable && !IsInDowntime)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        success = true;
                        setSpriteColor(parryWindowColor);
                    }
                }
                windowT += Time.deltaTime;
                yield return null;
            }

            bool late = false;
            float lateT = 0f;
            setSpriteColor(IsInDowntime ? cooldownColor : idleColor);
            while (!success && !early && lateT < postWindowLateThreshold)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    late = true;
                    break;
                }
                lateT += Time.deltaTime;
                yield return null;
            }

            ParryResult result;
            float damageMultiplier = 1f;
            bool grantMana = false;

            if (success)
            {
                result = ParryResult.Success;
                damageMultiplier = 0f;
                grantMana = true;
            }
            else if (early)
            {
                result = ParryResult.Early;
                damageMultiplier = IsInDowntime ? 1.25f : 1f;
            }
            else if (late)
            {
                result = ParryResult.Late;
                damageMultiplier = IsInDowntime ? 1.25f : 1f;
            }
            else
            {
                result = ParryResult.Fail;
                damageMultiplier = IsInDowntime ? 1.25f : 1f;
            }

            if (result != ParryResult.Success)
            {
                allHitsParried = false;
            }

            ResolvedHit resolved = new ResolvedHit
            {
                finalDamage = hit.baseDamage * damageMultiplier,
                element = attack.element,
                result = result,
                damageMultiplier = damageMultiplier,
                grantMana = grantMana
            };

            onHitResolved?.Invoke(resolved);

            setSpriteColor(IsInDowntime ? cooldownColor : idleColor);
        }

        var attackRes = new AttackResolution
        {
            allHitsParried = allHitsParried,
            grantFullTurnIconNextTurn = allHitsParried && attack.isParryable
        };

        onAttackResolved?.Invoke(attackRes);

        setSpriteColor(idleColor);
    }
}

