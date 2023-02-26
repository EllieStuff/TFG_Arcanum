using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemyScript : MonoBehaviour
{
    public enum States { IDLE, RANDOM_MOVEMENT, MOVE_TO_TARGET, ATTACK, REST, DAMAGE }
    public enum EnemyType { PLANT, BAT, RAT, GHOST }

    const float DEFAULT_SPEED_REDUCTION = 1.4f;
    const float PLAYER_HIT_DISTANCE_SWORD = 3;
    const float THRESHOLD = 1f;
    const int ENEMY_LAYER = 7;


    [Header("BaseEnemy")]
    [SerializeField] internal EnemyType enemyType;
    [SerializeField] internal float baseRotSpeed = 4;
    [SerializeField] internal float playerDetectionDistance = 8f, playerStopDetectionDistance = 15f;
    [SerializeField] internal float enemyStartAttackDistance, enemyStopAttackDistance;
    [SerializeField] internal bool isAttacking = false;
    [SerializeField] internal float baseMoveSpeed, playerFoundSpeed;
    [SerializeField] bool attacksTargetWOSeeingIt = false;  // WO == Without
    [SerializeField] bool movesToTargetWOSeeingIt = false;
    [SerializeField] internal float baseDamageTimer;
    [SerializeField] internal float baseDeathTime;
    [SerializeField] protected bool movesToTarget = true;
    [SerializeField] bool needsToRest = false;
    [SerializeField] Vector2 idleWait = new Vector2(0.6f, 2.0f);
    [SerializeField] Vector2 restWait = new Vector2(3.0f, 3.5f);
    [SerializeField] int numOfRndMoves = 0;

    internal ZoneScript zoneSystem;
    internal float damageTimer = 0;
    float idleWaitTimer = 0f;
    int rndMovesDone = 0;
    Vector3 rndTarget;
    float restTimer = 0f;
    PlayerSword playerSword;
    LifeSystem playerLife;
    LifeSystem enemyLife;
    PlayerMovement playerMovement;
    bool SwordTouching;
    internal bool deadNPC = false;

    readonly internal Vector3 
        baseMinVelocity = new Vector3(-10, -10, -10), 
        baseMaxVelocity = new Vector3(10, 10, 10);

    internal States state = States.IDLE;
    internal Rigidbody rb;
    internal Transform player;
    internal float actualMoveSpeed;
    internal float actualRotSpeed;
    internal float speedMultiplier = 0.5f;
    internal Vector3 actualMinVelocity, actualMaxVelocity;
    [HideInInspector] public Vector3 moveDir = Vector3.zero;
    internal bool canMove = true, canRotate = true, canAttack = true;
    internal Quaternion targetRot;
    protected bool endAttackFlag = true;
    protected bool canEnterDamageState = true;

    bool MakesRandomMoves { get { return numOfRndMoves != 0; } }
    bool HaveRandomMovesAvailable { get { return numOfRndMoves < 0 || rndMovesDone < numOfRndMoves; } }

    //PLACEHOLDER
    [SerializeField] SkinnedMeshRenderer enemyMesh;
    [SerializeField] Material transparentMat;
    //_________________________________________

    //private WeaponStats playerWeaponStats;


    // Start is called before the first frame update
    void Start()
    {
        Start_Call();
    }
    internal virtual void Start_Call()
    {
        rb = GetComponent<Rigidbody>();
        enemyLife = GetComponent<LifeSystem>();
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        player = playerGO.transform;
        playerLife = playerGO.GetComponent<LifeSystem>();
        playerSword = playerGO.GetComponent<PlayerSword>();
        playerMovement = playerGO.GetComponent<PlayerMovement>();
        enemyMesh.material = new Material(enemyMesh.material);

        ResetSpeed();
    }

    private void Update()
    {
        Update_Call();
    }
    internal virtual void Update_Call() { }

    void FixedUpdate()
    {
        FixedUpdate_Call();
    }
    internal virtual void FixedUpdate_Call()
    {
        UpdateStateMachine();

        LimitVelocity();

        if (canRotate)
        {
            if (!deadNPC && moveDir != Vector3.zero)
            {
                Vector3 rotDir = new Vector3(rb.velocity.x, 0f, rb.velocity.z).normalized;
                targetRot = Quaternion.LookRotation(rotDir, Vector3.up);
                //targetRot = Quaternion.LookRotation(rb.velocity.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, actualRotSpeed * speedMultiplier * Time.deltaTime);
            }
            else if (!deadNPC)
            {
                targetRot = Quaternion.LookRotation((player.position - transform.position).normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, actualRotSpeed * speedMultiplier * Time.deltaTime);
                Vector3 auxRot = transform.eulerAngles;
                transform.rotation = Quaternion.Euler(0, auxRot.y, 0);
            }
        }
    }

    internal virtual void UpdateStateMachine()
    {
        switch (state)
        {
            case States.IDLE:
                //patrol
                IdleUpdate();
                break;

            case States.RANDOM_MOVEMENT:
                RandomMovementUpdate();
                break;

            case States.MOVE_TO_TARGET:
                //approach to player
                MoveToTargetUpdate();
                break;

            case States.ATTACK:
                //attack
                AttackUpdate();
                break;

            case States.REST:
                RestUpdate();
                break;

            case States.DAMAGE:
                //receive damage
                DamageUpdate();
                break;

            default:
                Debug.LogWarning("State not found");
                break;
        }
    }

    internal virtual void DamageUpdate()
    {
        //if (!canAttack) ChangeState(States.IDLE);

        damageTimer -= Time.deltaTime;

        if(enemyLife.currLife <= 0 && !deadNPC)
        {
            if (Vector3.Distance(transform.position, player.position) <= PLAYER_HIT_DISTANCE_SWORD)
                playerSword.mustAttack = false;

            damageTimer = baseDeathTime;
            deadNPC = true;
        }

        if(deadNPC && enemyMesh.material.color.a > 0)
        {
            if(enemyMesh.material.color.a >= 1f)
                enemyMesh.material = transparentMat;
            enemyMesh.material.color -= new Color(0, 0, 0, Time.deltaTime);
        }

        if (zoneSystem != null && deadNPC)
        {
            zoneSystem.enemiesQuantity -= 1;
            zoneSystem = null;
        }

        if (damageTimer <= 0 && deadNPC)
        {
            //if (zoneSystem != null)
            //    zoneSystem.enemiesQuantity -= 1;

            Destroy(gameObject);
        }
        else if (damageTimer <= 0)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            if (distToPlayer <= enemyStartAttackDistance) ChangeState(States.ATTACK);
            else if (distToPlayer <= playerDetectionDistance) ChangeState(States.MOVE_TO_TARGET);
            else ChangeState(States.IDLE);
        }
    }
    internal virtual void IdleUpdate()
    {
        idleWaitTimer -= Time.deltaTime;
        if (idleWaitTimer > 0) return;
        else idleWaitTimer = Random.Range(idleWait.x, idleWait.y);

        Debug.Log("Dbg Idle");
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (movesToTarget)
        {
            Debug.Log("Debug 1.1");
            if (canMove && distToPlayer <= playerDetectionDistance)
            {
                if (movesToTargetWOSeeingIt)
                    ChangeState(States.MOVE_TO_TARGET);
                else
                {
                    Debug.Log("Debug 1.2");
                    RaycastHit hit;
                    bool hitCollided = Physics.Raycast(transform.position, (player.position - transform.position).normalized, out hit, distToPlayer, ENEMY_LAYER);
                    if (hitCollided && hit.transform.CompareTag("Player"))
                    {
                        Debug.Log("Debug 1.3");
                        if (distToPlayer > enemyStartAttackDistance)
                        {
                            Debug.Log("Debug 1.4");
                            ChangeState(States.MOVE_TO_TARGET);
                            return;
                        }
                    }
                    //else if (MakesRandomMoves && HaveRandomMovesAvailable)
                    //    ChangeState(States.RANDOM_MOVEMENT);
                }
            }
        }
        
        if (canAttack && distToPlayer <= enemyStartAttackDistance)
        {
            if (attacksTargetWOSeeingIt)
                ChangeState(States.ATTACK);
            else
            {
                Debug.Log("Debug 2.1");
                RaycastHit hit;
                bool hitCollided = Physics.Raycast(transform.position, (player.position - transform.position).normalized, out hit, distToPlayer, ENEMY_LAYER);
                if (hitCollided && hit.transform.CompareTag("Player"))
                {
                    Debug.Log("Debug 2.2");
                    if (Vector3.Angle(transform.forward, player.position - transform.position) < 1)
                        ChangeState(States.ATTACK);
                    else idleWaitTimer = -1;
                    return;
                }
                else if (hitCollided) Debug.Log("Debug 2.Content: " + hit.transform.name);
            }
        }
        
        if (MakesRandomMoves && HaveRandomMovesAvailable)
        {
            Debug.Log("Debug 3.1");
            ChangeState(States.RANDOM_MOVEMENT);
        }

    }
    internal virtual void RandomMovementUpdate()
    {
        moveDir = (rndTarget - transform.position).normalized;
        MoveRB(moveDir, ((actualMoveSpeed * 3f) / 4f) * speedMultiplier);

        if(Vector3.Distance(transform.position, rndTarget) < THRESHOLD)
        {
            ChangeState(States.IDLE);
        }
    }
    internal virtual void MoveToTargetUpdate()
    {
        rb.useGravity = false;

        Vector3 targetMoveDir = (player.position - transform.position).normalized;
        MoveRB(targetMoveDir, actualMoveSpeed * speedMultiplier);

        if (Vector3.Distance(transform.position, player.position) > playerStopDetectionDistance)
            ChangeState(States.IDLE);

        if (!movesToTargetWOSeeingIt)
        {
            RaycastHit hit;
            bool hitCollided = Physics.Raycast(transform.position, (player.position - transform.position).normalized, out hit, playerStopDetectionDistance, ENEMY_LAYER);
            if (!hitCollided || !hit.transform.CompareTag("Player"))
                ChangeState(States.IDLE);
        }

        if (canAttack && Vector3.Distance(transform.position, player.position) <= enemyStartAttackDistance)
            ChangeState(States.ATTACK);
    }
    internal virtual void AttackUpdate()
    {
        if (!canAttack) { ChangeState(States.IDLE); return; }

        Debug.Log("Dbg Attacking");
        float playerDistance = Vector3.Distance(transform.position, player.position);
        if(playerDistance <= PLAYER_HIT_DISTANCE_SWORD)
        {
            rb.useGravity = true;
            playerMovement.attackDir = transform.position;

            if(playerLife.currLife > 0)
                playerSword.mustAttack = true;
        }

        if (endAttackFlag)
        {
            if (needsToRest) ChangeState(States.REST);
            else ChangeState(States.IDLE);
        }

        //if (!isAttacking && playerDistance > enemyStopAttackDistance)
        //{
        //    if (movesToTarget) ChangeState(States.MOVE_TO_TARGET);
        //    else ChangeState(States.IDLE);
        //}
    }
    internal virtual void RestUpdate()
    {
        restTimer -= Time.deltaTime;
        Debug.Log("Dbg Resting");
        if (restTimer <= 0f) ChangeState(States.IDLE);
    }
    
    internal virtual void IdleStart() { StopRB(5.0f); idleWaitTimer = Random.Range(idleWait.x, idleWait.y); }
    internal virtual void RandomMovementStart()
    {
        float rndFactor = 4f;
        bool collided = true;
        int currTrials = 0, maxTrials = 10;
        while (collided && currTrials <= maxTrials)
        {
            rndTarget = transform.position + new Vector3(Random.Range(-rndFactor, rndFactor), 0f, Random.Range(-rndFactor, rndFactor));
            collided = Physics.BoxCast(transform.position, Vector3.one, 
                (rndTarget - transform.position).normalized, Quaternion.identity, Vector3.Distance(transform.position, rndTarget), 7);
            currTrials++;
        }
        //Debug.Log("Num of Trials: " + currTrials);
        actualMoveSpeed = baseMoveSpeed;
        if (collided) ChangeState(States.IDLE);
    }
    internal virtual void MoveToTargetStart() { if (numOfRndMoves > 0) rndMovesDone = 0; actualMoveSpeed = playerFoundSpeed; }
    internal virtual void AttackStart() { if (numOfRndMoves > 0) rndMovesDone = 0; }
    internal virtual void RestStart() { restTimer = Random.Range(restWait.x, restWait.y); canMove = canRotate = false; }
    internal virtual void DamageStart() { damageTimer = baseDamageTimer; }
    internal virtual void IdleExit() { }
    internal virtual void RandomMovementExit() { if (numOfRndMoves > 0) rndMovesDone++; }
    internal virtual void MoveToTargetExit() { }
    internal virtual void AttackExit() { }
    internal virtual void RestExit() { canMove = canRotate = true; }
    internal virtual void DamageExit() { }

    public virtual void ChangeState(States _state)
    {
        if (!canEnterDamageState && _state == States.DAMAGE) return;

        switch (state)
        {
            case States.IDLE:
                IdleExit();
                break;
            case States.RANDOM_MOVEMENT:
                RandomMovementExit();
                break;
            case States.MOVE_TO_TARGET:
                MoveToTargetExit();
                break;
            case States.ATTACK:
                AttackExit();
                break;
            case States.REST:
                RestExit();
                break;
            case States.DAMAGE:
                DamageExit();
                break;

            default:
                Debug.LogWarning("State not found");
                break;
        }

        state = _state;

        switch (state)
        {
            case States.IDLE:
                IdleStart();
                break;
            case States.RANDOM_MOVEMENT:
                RandomMovementStart();
                break;
            case States.MOVE_TO_TARGET:
                MoveToTargetStart();
                break;
            case States.ATTACK:
                AttackStart();
                break;
            case States.REST:
                RestStart();
                break;
            case States.DAMAGE:
                DamageStart();
                break;

            default:
                Debug.LogWarning("State not found");
                break;
        }
    }


    internal Vector3 ClampVector(Vector3 _originalVec, Vector3 _minVec, Vector3 _maxVec)
    {
        return new Vector3(
            Mathf.Clamp(_originalVec.x, _minVec.x, _maxVec.x),
            Mathf.Clamp(_originalVec.y, _minVec.y, _maxVec.y),
            Mathf.Clamp(_originalVec.z, _minVec.z, _maxVec.z)
        );
    }

    internal void MoveRB(Vector3 _moveDir, float _moveForce, ForceMode _forceMode = ForceMode.Force)
    {
        if (canMove)
            rb.velocity = _moveDir * _moveForce;
    }
    internal void StopRB(float _speedReduction = DEFAULT_SPEED_REDUCTION)
    {
        rb.velocity = new Vector3(rb.velocity.x / _speedReduction, rb.velocity.y, rb.velocity.z / _speedReduction);
    }
    internal void SetVelocityLimit(Vector3 _minSpeed, Vector3 _maxSpeed)
    {
        actualMinVelocity = _minSpeed;
        actualMaxVelocity = _maxSpeed;
    }
    void LimitVelocity()
    {
        rb.velocity = ClampVector(rb.velocity, actualMinVelocity * speedMultiplier, actualMaxVelocity * speedMultiplier);
    }
    public void ChangeSpeed(float _moveForce, float _rotSpeed, Vector3 _minSpeed, Vector3 _maxSpeed)
    {
        actualMoveSpeed = _moveForce;
        actualRotSpeed = _rotSpeed;
        actualMinVelocity = _minSpeed;
        actualMaxVelocity = _maxSpeed;
    }
    public void ResetSpeed()
    {
        actualMoveSpeed = baseMoveSpeed;
        actualRotSpeed = baseRotSpeed;
        actualMinVelocity = baseMinVelocity;
        actualMaxVelocity = baseMaxVelocity;
    }


    internal Vector3 NormalizeDirection(Vector3 moveDir)
    {
        if (moveDir.x > 0.5f)
            moveDir = new Vector3(1, moveDir.y, moveDir.z);
        if (moveDir.x < -0.5f)
            moveDir = new Vector3(-1, moveDir.y, moveDir.z);

        if (moveDir.z > 0.5f)
            moveDir = new Vector3(moveDir.x, moveDir.y, 1);
        if (moveDir.z < -0.5f)
            moveDir = new Vector3(moveDir.x, moveDir.y, -1);

        return moveDir;
    }


    void OnCollisionEnter(Collision col)
    {
        CollisionEnterEvent(col);
    }
    internal virtual void CollisionEnterEvent(Collision col)
    {
        if (col.gameObject.CompareTag("floor"))
            rb.useGravity = false;

    }

    void OnCollisionExit(Collision col)
    {
        CollisionExitEvent(col);
    }
    internal virtual void CollisionExitEvent(Collision col)
    {
        if (col.gameObject.CompareTag("floor"))
            rb.useGravity = true;

    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.tag.Equals("SwordRegion") || other.tag.Equals("Weapon"))
        //{
        //    SwordTouching = true;
        //    WeaponStats weaponStats = other.GetComponent<WeaponStats>();
        //    enemyLife.Damage(weaponStats.weaponDamage, HealthState.GetHealthStateByEffect(weaponStats.weaponEffect, enemyLife));
        //    damageTimer = baseDamageTimer;
        //    ChangeState(States.DAMAGE);
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        //if (other.tag.Equals("SwordRegion") || other.tag.Equals("Weapon"))
        //{
        //    SwordTouching = false;
        //}
    }
}
