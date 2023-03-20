using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    enum AnimState { NONE = -1, IDLE = 0, MOVING = 1, CHANGE_ELEMENT = 2, DIE = 3, ATTACKING = 4 }

    [SerializeField] Animator playerAnimator;

    PlayerMovement playerMovement;
    PlayerAttack playerAttack;
    ElementsManager elementsManager;
    LifeSystem playerLife;

    // Start is called before the first frame update
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAttack = GetComponent<PlayerAttack>();
        elementsManager = GetComponent<ElementsManager>();
        playerLife = GetComponent<LifeSystem>();

    }

    // Update is called once per frame
    void Update()
    {
        AnimationsStateMachine();

    }


    void AnimationsStateMachine()
    {
        if (playerLife.isDead)
        {
            SetAnimation(AnimState.DIE, 1f);
        }
        else if (playerMovement.Moving)
        {
            SetAnimation(AnimState.MOVING, 1f);
        }
        else if (elementsManager.ChangingElement)
        {
            SetAnimation(AnimState.CHANGE_ELEMENT, 1f);
        }
        else if (playerAttack.ShouldPlayAttackAnim())
        {
            SetAnimation(AnimState.ATTACKING, 1.4f);
        }
        else
        {
            SetAnimation(AnimState.IDLE, 1f);
        }
    }


    void SetAnimation(AnimState _animState, float _animSpeed)
    {
        playerAnimator.speed = _animSpeed;
        playerAnimator.SetInteger("prevState", playerAnimator.GetInteger("state"));
        playerAnimator.SetInteger("state", (int)_animState);
    }

}