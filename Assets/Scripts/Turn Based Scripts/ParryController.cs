using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParryController : MonoBehaviour // parry controller meant for parries
{
    public float downtimeSeconds = 0.75f;

    public float postWindowLateThreshold = 0.08f;

    [Header("Block Sprite")]
    public SpriteRenderer playerSprite;
    public Sprite blockSprite;
    public float blockSpriteDisplaySeconds = 0.1f;
    public Animator playerAnimator;

    private float downtimeUntil = 0f;
    private bool allHitsParried;

    public bool IsInDowntime => Time.time < downtimeUntil;

    // runs an enumerator to go through the stages of a parry, this is called from the game flow when an attack is being resolved, it will go through each hit and check for parry input, then at the end it will return the results of the parry to be used in the attack resolution
    public IEnumerator parryStages(AttackData attack, Action<int> onWindupStart, Action<ResolvedHit> onHitResolved, Action onReturnStart, Action<AttackResolution> onAttackResolved)
    {

        allHitsParried = true;

        int hitIndex = 0;
        // loop to iterate through all the hits
        foreach (var hit in attack.hits)
        {

            onWindupStart?.Invoke(hitIndex);
            hitIndex++;

            float t = 0f;
            bool early = false;
            while (t < hit.windupSeconds) // determines whether it hits in the early, perfect, or late stages of the parry and determines the damage multiplier
            {
                if (!early)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        early = true;
                        downtimeUntil = Time.time + downtimeSeconds;
                        StartCoroutine(FlashBlockSprite());
                    }
                }
                t += Time.deltaTime;
                yield return null;
            }

            bool success = false;
            float windowT = 0f;

            while (windowT < hit.parryWindowSeconds)
            {
                if (!success && hit.parryable && !IsInDowntime)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        success = true;
                        StartCoroutine(FlashBlockSprite());
                    }
                }
                windowT += Time.deltaTime;
                yield return null;
            }

            bool late = false;
            float lateT = 0f;
            while (!success && !early && lateT < postWindowLateThreshold)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    late = true;
                    StartCoroutine(FlashBlockSprite());
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


            onReturnStart?.Invoke();
            float downtimeT = 0f;
            while (downtimeT < hit.downtimeSeconds)
            {
                downtimeT += Time.deltaTime;
                yield return null;
            }
        }

        var attackRes = new AttackResolution
        {
            allHitsParried = allHitsParried,
            grantFullTurnIconNextTurn = allHitsParried && attack.isParryable
        };

        onAttackResolved?.Invoke(attackRes);
    }

    // this turns the sprite blue when it blocks
    private IEnumerator FlashBlockSprite()
    {
        if (playerSprite != null && blockSprite != null)
        {

            Sprite spriteBeforeBlock = playerSprite.sprite;
            Vector3 originalScale = playerSprite.transform.localScale;


            bool wasAnimatorEnabled = false;
            if (playerAnimator != null)
            {
                wasAnimatorEnabled = playerAnimator.enabled;
                playerAnimator.enabled = false;
            }


            playerSprite.sprite = blockSprite;
            playerSprite.transform.localScale = originalScale * 4f;

            yield return new WaitForSeconds(blockSpriteDisplaySeconds);


            playerSprite.sprite = spriteBeforeBlock;
            playerSprite.transform.localScale = originalScale;


            if (playerAnimator != null && wasAnimatorEnabled)
            {
                playerAnimator.enabled = true;
            }
        }
    }
}

