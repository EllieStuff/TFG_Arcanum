using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    const float INPUT_THRESHOLD = 0;
    const float MIN_FALL_SPEED = 10;
    const float MIN_SPEED_WALK = 0.8f;
    const float SPEED_REDUCTION = 1.4f;
    const float DIAGONAL_SPEED_REDUCTION = 0.8f;
    const float SCREEN_WIDTH = 1000;
    const float SCREEN_HEIGHT = 500;
    const float STOP_SPEED = 5;

    [SerializeField] float baseMoveForce = 50;
    [SerializeField] float baseRotSpeed = 300;
    [SerializeField] Vector3 baseMaxSpeed = new Vector3(50, 0, 50);
    [SerializeField] float fallSpeed;
    [SerializeField] float damageAnimationTime;
    [SerializeField] Animator playerAnimator;

    float actualMoveForce;
    float actualRotSpeed;
    float speedMultiplierRot = 10;
    internal float speedMultiplier = 0.2f;
    Vector3 actualMaxSpeed;
    internal bool canMove = true;
    internal bool canRotate = true;
    internal Vector3 targetMousePos;
    internal Vector3 attackDir;
    internal Vector2 mouseLookVec;
    internal bool cardEffect;
    LifeSystem lifeStatus;
    PlayerSword playerSword;
    PlayerDodge playerDodge;

    const float minFallSpeed = 10;
    bool damage;


    Rigidbody rb;
    [HideInInspector] public Vector3 moveDir = Vector3.zero;
    [HideInInspector] public Vector3 lookDir = Vector3.zero;
    bool moving = false;
    Camera mainCam;
    MusicJukebox music;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lifeStatus = GetComponent<LifeSystem>();
        playerSword = GetComponent<PlayerSword>();
        mainCam = GameObject.Find("Main Camera").GetComponent<Camera>();
        playerDodge = GetComponent<PlayerDodge>();
        music = GameObject.Find("Jukebox").GetComponent<MusicJukebox>();

        ResetSpeed();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        mouseLookVec = GetMouseLookVector();
        float horizontalInput = mouseLookVec.x;
        //if (Mathf.Abs(horizontalInput) < INPUT_THRESHOLD) horizontalInput = 0;
        float verticalInput = mouseLookVec.y;
        //if (Mathf.Abs(verticalInput) < INPUT_THRESHOLD) verticalInput = 0;
        lookDir = new Vector3(horizontalInput, 0, verticalInput);
        moveDir = MoveToTargetVector(targetMousePos);

        if (canMove && playerDodge.dodgeRechargeTimer <= playerDodge.dodgeRechargeDelay - 0.2f && !cardEffect && targetMousePos != Vector3.zero && (Mathf.Abs(verticalInput) > INPUT_THRESHOLD || Mathf.Abs(horizontalInput) > INPUT_THRESHOLD) && moveDir != Vector3.zero && lifeStatus.currLife > 0)
        {
            moving = true;

            rb.constraints = (RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation);

            if (rb.velocity.magnitude > MIN_SPEED_WALK)
                playerAnimator.SetFloat("state", 1);
            else
                playerAnimator.SetFloat("state", 0);

            if (Mathf.Abs(verticalInput) > INPUT_THRESHOLD && Mathf.Abs(horizontalInput) > INPUT_THRESHOLD)
                moveDir *= DIAGONAL_SPEED_REDUCTION;
            rb.velocity = moveDir * actualMoveForce * speedMultiplier;
            Vector3 finalVelocity = ClampVector(rb.velocity, -actualMaxSpeed * speedMultiplier, actualMaxSpeed * speedMultiplier) + new Vector3(0, rb.velocity.y, 0);
            rb.velocity = finalVelocity;
        }
        else if (moving)
        {
            moving = false;

            if (!cardEffect && playerDodge.dodgeRechargeTimer <= 0)
                rb.constraints = RigidbodyConstraints.FreezeAll;

            if (!damage)
                playerAnimator.SetFloat("state", 0);

            Vector3 reducedVel = rb.velocity;

            if (Mathf.Abs(reducedVel.x) > 0)
                reducedVel = new Vector3(rb.velocity.x / SPEED_REDUCTION, rb.velocity.y, rb.velocity.z);
            if (Mathf.Abs(reducedVel.z) > 0)
                reducedVel = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z / SPEED_REDUCTION);

            rb.velocity = reducedVel;
        }

        if (cardEffect && rb.velocity.magnitude <= STOP_SPEED)
            cardEffect = false;

        rb.velocity = FallSystem(rb.velocity);

        Quaternion targetRot;

        if (playerSword.mustAttack && rb.velocity.magnitude <= playerSword.minAttackMovespeed)
        {
            targetRot = Quaternion.LookRotation(attackDir - transform.position, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, actualRotSpeed * speedMultiplierRot * Time.deltaTime);
        }
        else if (canRotate && (moveDir == Vector3.zero || Input.GetKey(KeyCode.Mouse1)) && lifeStatus.currLife > 0)
        {
            targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, actualRotSpeed * speedMultiplierRot * Time.deltaTime);
        }
    }

    Vector2 GetMouseLookVector()
    {
        Vector3 MousePosWithPlayer = Input.mousePosition - mainCam.WorldToScreenPoint(transform.position);
        MousePosWithPlayer.z = 5.23f;

        return MousePosWithPlayer;
    }

    Vector3 FallSystem(Vector3 actualVelocity)
    {
        if(actualVelocity.y < MIN_FALL_SPEED && rb.useGravity)
            actualVelocity.y -= Time.deltaTime * fallSpeed;

        return actualVelocity;
    }

    Vector3 MoveToTargetVector(Vector3 mousePositionInWorld)
    {
        if (Vector3.Distance(mousePositionInWorld, transform.position) <= 1)
            return Vector3.zero;

        Vector3 vectorToMove = (mousePositionInWorld - transform.position).normalized;
        vectorToMove.y = 0;

        return vectorToMove;
    }

    Vector3 NormalizeDirection(Vector3 moveDir)
    {
        Vector2 MoveDirWithScreen = new Vector2(moveDir.x / SCREEN_WIDTH, moveDir.z / SCREEN_HEIGHT);

        if (moveDir.x > 0 && MoveDirWithScreen.x > 0.1f)
            moveDir = new Vector3(MIN_SPEED_WALK + (moveDir.x / SCREEN_WIDTH), moveDir.y, moveDir.z);
        else if (moveDir.x < 0 && MoveDirWithScreen.x < -0.1f)
            moveDir = new Vector3(-MIN_SPEED_WALK + (moveDir.x / SCREEN_WIDTH), moveDir.y, moveDir.z);
        else
            moveDir.x = 0;

        if (moveDir.z > 0 && MoveDirWithScreen.y > 0.1f)
            moveDir = new Vector3(moveDir.x, moveDir.y, MIN_SPEED_WALK + (moveDir.z / SCREEN_HEIGHT));
        else if (moveDir.z < 0 && MoveDirWithScreen.y < -0.1f)
            moveDir = new Vector3(moveDir.x, moveDir.y, -MIN_SPEED_WALK + (moveDir.z / SCREEN_HEIGHT));
        else
            moveDir.z = 0;

        return moveDir;
    }

    Vector3 ClampVector(Vector3 _originalVec, Vector3 _minVec, Vector3 _maxVec)
    {
        return new Vector3(
            Mathf.Clamp(_originalVec.x, _minVec.x, _maxVec.x),
            Mathf.Clamp(_originalVec.y, _minVec.y, _maxVec.y),
            Mathf.Clamp(_originalVec.z, _minVec.z, _maxVec.z)
        );
    }

    public void ChangeSpeed(float _moveForce, float _rotSpeed, Vector3 _maxSpeed)
    {
        actualMoveForce = _moveForce;
        actualRotSpeed = _rotSpeed;
        actualMaxSpeed = _maxSpeed;
    }
    public void ResetSpeed()
    {
        actualMoveForce = baseMoveForce;
        actualRotSpeed = baseRotSpeed;
        actualMaxSpeed = baseMaxSpeed;
    }
    public void DamageStartCorroutine()
    {
        music.EnableBattleMode();
        StartCoroutine(DamageCorroutine());
    }
    IEnumerator DamageCorroutine()
    {
        canMove = false;
        canRotate = false;
        damage = true;

        //Play animation here and sound
        playerAnimator.SetFloat("state", 2);
        //_____________________________

        yield return new WaitForSeconds(damageAnimationTime);

        playerAnimator.SetFloat("state", 0);

        damage = false;
        canMove = true;
        canRotate = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (rb.useGravity && collision.gameObject.tag.Equals("floor"))
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezePositionY;
        }
    }
}
