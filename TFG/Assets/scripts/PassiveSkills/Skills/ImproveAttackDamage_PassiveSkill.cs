using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImproveAttackDamage_PassiveSkill : PassiveSkill_Base
{
    const float DMG_INCREASE = 25f;

    public ImproveAttackDamage_PassiveSkill()
    {
        skillType = SkillType.IMPROVE_ATTACK_DAMAGE;
        maxLevel = -1;
        appearRatio = 0.8f;
        basePrice = 12000;
        priceInc = 10000;
        name = "More Attack";
        //initialDescription = "Your attacks will deal more damage!";
        initialDescription = "Your attacks will deal 25% more damage!";
        improvementDescription = initialDescription;
    }

    public override void Init(Transform _playerRef)
    {
        base.Init(_playerRef);
        AddLevelEvent();
    }


    public override void UpdateCall()
    {
        base.UpdateCall();
    }


    internal override void AddLevelEvent()
    {
        base.AddLevelEvent();
        PlayerAttack playeAttack = playerRef.GetComponent<PlayerAttack>();
        playeAttack.dmgIncrease += DMG_INCREASE;
    }

}
