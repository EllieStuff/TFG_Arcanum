using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageData : MonoBehaviour
{
    [SerializeField] internal Transform ownerTransform;
    [SerializeField] internal float weaponDamage;
    [SerializeField] internal HealthState.Effect weaponEffect = HealthState.Effect.NORMAL;
    [SerializeField] internal bool alwaysAttacking = false;
    [SerializeField] bool isACardEffect = false;

    internal HealthState customHealthState = null;


    public bool IsAttacking
    {
        get
        {
            if (isACardEffect || alwaysAttacking) return true;
            if (ownerTransform == null) return false;

            if(ownerTransform.GetComponent<LifeSystem>().entityType == LifeSystem.EntityType.PLAYER)
            {
                return ownerTransform.GetComponent<PlayerSword>().isAttacking;
            }
            else
            {
                return ownerTransform.GetComponent<BaseEnemyScript>().isAttacking;
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!IsAttacking) return;
        if (!isACardEffect && (ownerTransform == null || other.transform == ownerTransform)) return;


        if (!isACardEffect && other.CompareTag("Player"))
        {
            LifeSystem lifeSystem = other.GetComponent<LifeSystem>();
            ApplyDamage(lifeSystem);
            other.GetComponent<PlayerMovement>().DamageStartCorroutine();
        }

        if (other.CompareTag("Enemy"))
        {
            if (isACardEffect)
                Debug.Log("enter");
            LifeSystem lifeSystem = other.GetComponent<LifeSystem>();
            ApplyDamage(lifeSystem);
            other.GetComponent<BaseEnemyScript>().ChangeState(BaseEnemyScript.States.DAMAGE);
        }

    }


    void ApplyDamage(LifeSystem _lifeSystem)
    {
        if (customHealthState == null)
        {
            _lifeSystem.Damage(weaponDamage, HealthState.GetHealthStateByEffect(weaponEffect, _lifeSystem));
        }
        else
        {
            _lifeSystem.Damage(weaponDamage, customHealthState);
        }
    }

}
