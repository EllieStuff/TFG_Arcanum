using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    const float INPUT_THRESHOLD = 0.3f;
    const float MIN_FALL_SPEED = 10;
    const float SPEED_REDUCTION = 1.4f;
    const float DIAGONAL_SPEED_REDUCTION = 0.8f;

    [SerializeField] float baseMoveForce = 50;
    [SerializeField] float baseRotSpeed = 300;
    [SerializeField] Vector3 baseMaxSpeed = new Vector3(50, 0, 50);
    [SerializeField] float fallSpeed;

    float actualMoveForce;
    float actualRotSpeed;
    internal float speedMultiplier = 1.0f;
    Vector3 actualMaxSpeed;
    internal bool canMove = true;
    internal bool canRotate = true;
    LifeSystem lifeStatus;

    const float minFallSpeed = 10;


    Rigidbody rb;
    [HideInInspector] public Vector3 moveDir = Vector3.zero;
    bool moving = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lifeStatus = GetComponent<LifeSystem>();

        ResetSpeed();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(horizontalInput) < INPUT_THRESHOLD) horizontalInput = 0;
        float verticalInput = Input.GetAxis("Vertical");
        if (Mathf.Abs(verticalInput) < INPUT_THRESHOLD) verticalInput = 0;
        moveDir = NormalizeDirection(new Vector3(horizontalInput, 0, verticalInput));


        if ((Mathf.Abs(verticalInput) > INPUT_THRESHOLD || Mathf.Abs(horizontalInput) > INPUT_THRESHOLD) && lifeStatus.currLife > 0)
        {
            moving = true;
            if (Mathf.Abs(verticalInput) > INPUT_THRESHOLD && Mathf.Abs(horizontalInput) > INPUT_THRESHOLD)
                moveDir *= DIAGONAL_SPEED_REDUCTION;
            rb.AddForce(moveDir * actualMoveForce * speedMultiplier, ForceMode.Force);
            Vector3 finalVelocity = ClampVector(rb.velocity, -actualMaxSpeed * speedMultiplier, actualMaxSpeed * speedMultiplier) + new Vector3(0, rb.velocity.y, 0);
            rb.velocity = finalVelocity;
        }
        else if (moving)
        {
            moving = false;

            Vector3 reducedVel = rb.velocity;

            if (Mathf.Abs(reducedVel.x) > 0)
                reducedVel = new Vector3(rb.velocity.x / SPEED_REDUCTION, rb.velocity.y, rb.velocity.z);
            if (Mathf.Abs(reducedVel.z) > 0)
                reducedVel = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z / SPEED_REDUCTION);

            rb.velocity = reducedVel;
        }

        rb.velocity = FallSystem(rb.velocity);

        if (canRotate && moveDir != Vector3.zero && lifeStatus.currLife > 0)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, actualRotSpeed * speedMultiplier * Time.deltaTime);
        }
    }

    Vector3 FallSystem(Vector3 actualVelocity)
    {
        if(actualVelocity.y < MIN_FALL_SPEED && rb.useGravity)
            actualVelocity.y -= Time.deltaTime * fallSpeed;

        return actualVelocity;
    }

    Vector3 NormalizeDirection(Vector3 moveDir)
    {
        if (moveDir.x > 0)
            moveDir = new Vector3(1, moveDir.y, moveDir.z);
        if (moveDir.x < 0)
            moveDir = new Vector3(-1, moveDir.y, moveDir.z);

        if (moveDir.z > 0)
            moveDir = new Vector3(moveDir.x, moveDir.y, 1);
        if (moveDir.z < 0)
            moveDir = new Vector3(moveDir.x, moveDir.y, -1);

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


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("floor"))
            rb.useGravity = false;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag.Equals("floor"))
            rb.useGravity = true;
    }
}
