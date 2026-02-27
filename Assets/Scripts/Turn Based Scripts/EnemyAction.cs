using System;
using System.Collections;
using System.Collections.Generic;
using battleEnum;
using UnityEngine;

// active enemy logic
public class EnemyAction : MonoBehaviour
{
    public GameFlow gameManager;
    public event Action enemyTurnEnd;
    public event Action<float, Element> enemyDealDamage;
    public event Action<AttackData> enemyAttackDeclared;
    public event Action<EnemyState> onEnemyInitialized;
    public PlayerAction playerAction;

    [Header("Animation")]
    public Animator enemyAnimator;
    public string attackTriggerName = "Attack";
    public bool useAttackIdTrigger = false;

    [Header("Attack Movement")]
    public bool useAttackMove = true;
    public Transform enemyRoot;
    public Vector3 attackLocalOffset = new Vector3(-5f, 0f, 0f);
    public float attackMoveSeconds = 0.08f;
    public float attackHoldSeconds = 0.02f;
    public float attackReturnSeconds = 0.1f;
    public bool playAnimationAlso = false;

    private Coroutine attackMoveRoutine;
    private Vector3 originalLocalPosition;
    private bool originalPositionCaptured = false;

    public EnemyState currentEnemy;

    void Start()
    {
        if (gameManager != null)
        {
            gameManager.turnChanged += myTurnStart;
        }
    }

    void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.turnChanged -= myTurnStart;
        }
    }

    
    public void addTriggers() // add and remove triggers to keep code cleanliness
    {
        if (playerAction == null || GameSession.gs == null || GameSession.gs.enemyMember == null) return;

        playerAction.dealDamage += GameSession.gs.enemyMember.takeDamage;
        playerAction.applyBurn += GameSession.gs.enemyMember.takeBurn;
        playerAction.applySlow += GameSession.gs.enemyMember.takeSlow;
        playerAction.applyDamageDebuff += GameSession.gs.enemyMember.takeDamageDebuff;
    }

    
    public void removeTriggers()
    {
        if (playerAction == null || GameSession.gs == null || GameSession.gs.enemyMember == null) return;

        playerAction.dealDamage -= GameSession.gs.enemyMember.takeDamage;
        playerAction.applyBurn -= GameSession.gs.enemyMember.takeBurn;
        playerAction.applySlow -= GameSession.gs.enemyMember.takeSlow;
        playerAction.applyDamageDebuff -= GameSession.gs.enemyMember.takeDamageDebuff;
    }

    
    private void myTurnStart(int turnOwner) // turn start and execute turn are different in the sense that turn start is before the enemy moves, and execute turn is during
    {
        if (turnOwner == 0) return;

        if (GameSession.gs == null || GameSession.gs.enemyMember == null) return;

        currentEnemy = GameSession.gs.enemyMember;

        onEnemyInitialized?.Invoke(currentEnemy);
        ExecuteTurn();
    }

    
    private void ExecuteTurn()
    {
        if (currentEnemy == null) return;

        ApplyBurnDamage();


        if (currentEnemy.enemyStats == null || currentEnemy.enemyStats.attackPatterns == null || currentEnemy.enemyStats.attackPatterns.Count == 0)
        {
            return;
        }

        EnemyStats.AttackPattern selectedPattern = currentEnemy.enemyStats.attackPatterns[UnityEngine.Random.Range(0, currentEnemy.enemyStats.attackPatterns.Count)];


        AttackData attack = currentEnemy.enemyStats.InstantiateAttack(selectedPattern, currentEnemy.calculateAttack());
        attack.iconCostHalves = currentEnemy.calculateIconCost(attack.element);


        enemyAttackDeclared?.Invoke(attack);
    }


    public void RaiseEnemyDealDamage(float damage, Element element)
    {
        enemyDealDamage?.Invoke(damage, element);
    }


    // animation as well as attacking
    public void TriggerAttackAnimation(AttackData attack, int hitIndex)
    {
        if (useAttackMove)
        {
            if (attackMoveRoutine != null)
            {
                StopCoroutine(attackMoveRoutine);
            }

            Transform root = enemyRoot != null ? enemyRoot : transform;


            if (!originalPositionCaptured)
            {
                originalLocalPosition = root.localPosition;
                originalPositionCaptured = true;
            }

            attackMoveRoutine = StartCoroutine(AttackMoveForwardRoutine(root));
        }

        if (playAnimationAlso && enemyAnimator != null)
        {
            string triggerName = attackTriggerName;
            if (useAttackIdTrigger && attack != null && !string.IsNullOrWhiteSpace(attack.attackId))
            {
                triggerName = attack.attackId;
            }

            enemyAnimator.SetTrigger(triggerName);
        }
    }

    
    public void TriggerReturnMovement()
    {
        if (useAttackMove)
        {
            if (attackMoveRoutine != null)
            {
                StopCoroutine(attackMoveRoutine);
            }

            Transform root = enemyRoot != null ? enemyRoot : transform;
            attackMoveRoutine = StartCoroutine(ReturnToOriginalRoutine(root));
        }
    }

    // used an IEnumerator for the attack movement to allow for smooth movement and timing, this is called when the enemy attacks and moves them forward, then after a short delay moves them back to the original position

    
    private IEnumerator AttackMoveForwardRoutine(Transform root)
    {
        if (root == null) yield break;


        Vector3 start = root.localPosition;
        Vector3 target = originalLocalPosition + attackLocalOffset;

        float t = 0f;
        float moveDur = Mathf.Max(0.001f, attackMoveSeconds);
        while (t < moveDur)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / moveDur);
            root.localPosition = Vector3.Lerp(start, target, lerp);
            yield return null;
        }

        root.localPosition = target;

        if (attackHoldSeconds > 0f)
        {
            yield return new WaitForSeconds(attackHoldSeconds);
        }

        attackMoveRoutine = null;
    }

    
    private IEnumerator ReturnToOriginalRoutine(Transform root)
    {
        if (root == null) yield break;

        Vector3 start = root.localPosition;
        Vector3 target = originalLocalPosition;

        float t = 0f;
        float returnDur = Mathf.Max(0.001f, attackReturnSeconds);
        while (t < returnDur)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / returnDur);
            root.localPosition = Vector3.Lerp(start, target, lerp);
            yield return null;
        }

        root.localPosition = originalLocalPosition;
        attackMoveRoutine = null;
    }


    
    public void FinalizeAttack(AttackData attack)
    {
        var enemy = GameSession.gs.enemyMember;
        if (enemy == null) return;

        int iconCost = attack != null ? attack.iconCostHalves : 0;

        enemy.consumePushTurnIcons(iconCost);
        enemy.tickBurnDuration();
        enemy.tickSlowDuration();
        enemy.tickDamageDebuffDuration();
        enemyTurnEnd?.Invoke();
    }

    
    private void ApplyBurnDamage()
    {
        if (currentEnemy != null && currentEnemy.currentBurnStacks > 0)
        {
            float burnDamage = currentEnemy.calculateBurnDamage();
            currentEnemy.takeDamage(burnDamage, Element.Fire);
        }
    }
}
