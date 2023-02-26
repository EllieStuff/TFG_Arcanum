using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceEnemy : BaseEnemyScript
{
    [Header("DistanceEnemy")]
    [SerializeField] float attackDistance;
    [SerializeField] float baseAttackTimer;
    [SerializeField] float attackAnimationTime;
    [SerializeField] float attackDamage;
    [SerializeField] Animator enemyAnimator;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] List<Light> pointLights;
    [SerializeField] Transform shootPoint;

    enum AttackType { NORMAL_THROW, CIRCLE_ATTACK, FOUR_PROJECTILES, THREE_PROJECTILES }

    [SerializeField] AttackType attackStyle;

    float attackTimer;

    const int CIRCLE_ITERATIONS = 24;
    const int CIRCLE_MULTIPLIER = 50;

    internal override void Start_Call() { base.Start_Call(); attackTimer = baseAttackTimer; }

    internal override void Update_Call() { base.Update_Call(); }

    internal override void FixedUpdate_Call() { base.FixedUpdate_Call(); }


    internal override void IdleUpdate()
    {
        enemyAnimator.SetFloat("state", 0);
        base.IdleUpdate();
    }
    internal override void MoveToTargetUpdate()
    {
        enemyAnimator.SetFloat("state", 1);
        base.MoveToTargetUpdate();
    }
    internal override void DamageUpdate()
    {
        enemyAnimator.SetFloat("state", 2);
        base.DamageUpdate();
    }
    internal override void AttackUpdate()
    {
        base.AttackUpdate();

        if (Vector3.Distance(player.position, transform.position) > attackDistance)
        {
            enemyAnimator.SetFloat("state", 1);
            Vector3 targetMoveDir = (player.position - transform.position).normalized;
            MoveRB(targetMoveDir, actualMoveSpeed * speedMultiplier);
        }
        else
        {
            if (!state.Equals(States.DAMAGE))
                rb.velocity = new Vector3(0, rb.velocity.y, 0);

            enemyAnimator.SetFloat("state", 0);
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0)
            {
                StartCoroutine(AttackCorroutine());
                attackTimer = baseAttackTimer;
            }
        }
    }
    internal override void DeathUpdate()
    {
        base.DeathUpdate();
        if (pointLights.Count > 0)
        {
            for (int i = 0; i < pointLights.Count; i++)
            {
                pointLights[i].intensity -= Time.deltaTime;
                if (pointLights[i].intensity <= 0f)
                {
                    pointLights.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    IEnumerator AttackCorroutine()
    {
        //place shoot animation here

        yield return new WaitForSeconds(attackAnimationTime);
        RaycastHit hit;
        if (Physics.Raycast(shootPoint.position, (player.position - shootPoint.position).normalized, out hit))
        {
            if (!hit.transform.CompareTag("Wall"))
                AttackStateMachine(attackStyle);
        }
    }

    void AttackStateMachine(AttackType type)
    {
        ProjectileData projectile;
        switch (type)
        {
            case AttackType.FOUR_PROJECTILES:
                for (int knifeDirState = 0; knifeDirState < 4; knifeDirState++)
                {
                    projectile = Instantiate(projectilePrefab, transform).GetComponent<ProjectileData>();
                    projectile.Init(transform);
                    projectile.transform.SetParent(null);
                    switch (knifeDirState)
                    {
                        case 0:
                            projectile.moveDir = new Vector3(1, 0, 0);
                            break;
                        case 1:
                            projectile.moveDir = new Vector3(-1, 0, 0);
                            break;
                        case 2:
                            projectile.moveDir = new Vector3(0, 0, 1);
                            break;
                        case 3:
                            projectile.moveDir = new Vector3(0, 0, -1);
                            break;
                    }
                }
                break;
            case AttackType.CIRCLE_ATTACK:
                float circleY = 0.1f;
                float circleX = 0.1f;
                float auxTimer = 0;
                for (int i = 0; i < CIRCLE_ITERATIONS; i++)
                {
                    circleY = Mathf.Sin(auxTimer);
                    circleX = Mathf.Cos(auxTimer);
                    auxTimer += Time.deltaTime * CIRCLE_MULTIPLIER;
                    KnifeThrown knife_2 = Instantiate(projectilePrefab, transform).GetComponent<KnifeThrown>();
                    knife_2.knifeDir = new Vector3(circleX, 0, circleY);
                    knife_2.SetOwnerTransform(transform);
                }
                break;
            case AttackType.THREE_PROJECTILES:
                for (int direction = 0; direction < 3; direction++)
                {
                    KnifeThrown knife_3 = Instantiate(projectilePrefab, transform).GetComponent<KnifeThrown>();
                    switch (direction)
                    {
                        case 0:
                            knife_3.knifeDir = new Vector3(1, 0, 1);
                            break;
                        case 1:
                            knife_3.knifeDir = new Vector3(-1, 0, 1);
                            break;
                        case 2:
                            knife_3.knifeDir = new Vector3(0, 0, 1);
                            break;
                    }
                    knife_3.SetOwnerTransform(transform);
                    knife_3.localDir = true;
                    knife_3.entityThrowingIt = transform;
                }
                break;
            case AttackType.NORMAL_THROW:
                projectile = Instantiate(projectilePrefab, shootPoint).GetComponent<ProjectileData>();
                projectile.Init(transform);
                projectile.transform.SetParent(null);
                //projectile.moveDir = (player.position - transform.position).normalized;
                break;
        }
    }

    internal override void IdleStart() { base.IdleStart(); }
    internal override void MoveToTargetStart() { base.MoveToTargetStart(); }
    internal override void AttackStart() { base.AttackStart(); }


    internal override void IdleExit() { base.IdleExit(); }
    internal override void MoveToTargetExit() { base.MoveToTargetExit(); }
    internal override void AttackExit() { base.AttackExit(); }


}
