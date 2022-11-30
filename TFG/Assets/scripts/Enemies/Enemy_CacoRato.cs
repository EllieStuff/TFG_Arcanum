using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_CacoRato : BaseEnemyScript
{
    //[Header("CacoRato")]
    [SerializeField] float attackDistance;
    [SerializeField] float baseAttackTimer;
    [SerializeField] float attackAnimationTime;
    [SerializeField] float attackDamage;
    [SerializeField] EnemyWeaponHand handWeapon;
    [SerializeField] Animator enemyAnimator;

    float attackTimer;

    LifeSystem playerLife;

    //PROVISIONAL
    [SerializeField] Animation swordAnim;
    //______________________________

    internal override void Start_Call() { base.Start_Call(); playerLife = player.GetComponent<LifeSystem>(); }

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
    internal override void AttackUpdate()
    {
        base.AttackUpdate();

        if(Vector3.Distance(player.position, transform.position) > attackDistance)
        {
            enemyAnimator.SetFloat("state", 1);
            Vector3 targetMoveDir = (player.position - transform.position).normalized;
            MoveRB(targetMoveDir, actualMoveSpeed * speedMultiplier);
        }
        else
        {
            enemyAnimator.SetFloat("state", 0);
            attackTimer -= Time.deltaTime;

            if(attackTimer <= 0)
            {
                StartCoroutine(AttackCorroutine());
                attackTimer = baseAttackTimer;
            }
        }
    }

    IEnumerator AttackCorroutine()
    {
        swordAnim.Play();

        yield return new WaitForSeconds(attackAnimationTime);

        if (handWeapon.isTouchingPlayer)
            playerLife.Damage(attackDamage, playerLife.healthState);
    }


    internal override void IdleStart() { base.IdleStart(); }
    internal override void MoveToTargetStart() { base.MoveToTargetStart(); }
    internal override void AttackStart() { base.AttackStart(); }


    internal override void IdleExit() { base.IdleExit(); }
    internal override void MoveToTargetExit() { base.MoveToTargetExit(); }
    internal override void AttackExit() { base.AttackExit(); }


}
