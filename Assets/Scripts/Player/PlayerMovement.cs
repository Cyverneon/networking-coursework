using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using System;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Speed")]
    [Tooltip("Player speed in units per second")]
    [SerializeField] private float speed = 2f;
    [Tooltip("Rate of acceleration in units*speed per second (i.e, if speed is 4, accel of 1 will accelerate at 4 units per second)")]
    [SerializeField] private float accel = 1f;
    [Tooltip("Rate of acceleration in the air")]
    [SerializeField] private float airAccel = 0.5f;

    [Header("Jumping")]
    [Tooltip("Jump height in units")]
    [SerializeField] private float jumpHeight = 50f;
    [Tooltip("Time in seconds player can press jump while about to hit the ground and still jump")]
    [SerializeField] private float jumpLeniency = 0.1f;
    [Tooltip("Layers which player can jump off if they are detected to stand on")]
    [SerializeField] private LayerMask groundCollisionLayers;

    [Header("Gravity")]
    [Tooltip("Force of gravity in units")]
    [SerializeField] private float gravity = 9.8f;
    [Tooltip("Maximum falling speed")]
    [SerializeField] private float terminalVelocity = 50f;

    [Header("Controls")]
    [SerializeField] private KeyCode keyLeft = KeyCode.A;
    [SerializeField] private KeyCode keyRight = KeyCode.D;
    [SerializeField] private KeyCode keyJump = KeyCode.J;

    private Rigidbody2D rigidbody2d;
    private Collider2D collider2d;

    private Vector3 velocity = new Vector3(0f, 0f, 0f);

    private Vector3 feetOffsetLeft = new Vector3(0f, 0f, 0f);
    private Vector3 feetOffsetRight = new Vector3(0f, 0f, 0f);

    private float jumpTimer = 0f;
    private bool cueJump = false;
    private bool onGround = true;

    private void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<Collider2D>();

        feetOffsetLeft.y = -(collider2d.bounds.extents.y);
        feetOffsetLeft.x = -(collider2d.bounds.extents.x) + 0.05f;

        feetOffsetRight.y = -(collider2d.bounds.extents.y);
        feetOffsetRight.x = (collider2d.bounds.extents.x) - 0.05f;
    }

    private void CheckJumpInput()
    {
        if (Input.GetKeyDown(keyJump))
        {
            cueJump = true;
            jumpTimer = jumpLeniency;
        }
        if (jumpTimer > 0)
        {
            jumpTimer -= Time.deltaTime;
            if (jumpTimer < 0)
            {
                cueJump = false;
            }
        }
    }

    private bool CheckOnGround()
    {
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position + feetOffsetLeft, -Vector2.up, 0.1f, groundCollisionLayers);
        Debug.DrawRay(transform.position + feetOffsetLeft, -Vector2.up, Color.red, 0.1f);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position + feetOffsetRight, -Vector2.up, 0.1f, groundCollisionLayers);
        Debug.DrawRay(transform.position + feetOffsetRight, -Vector2.up, Color.red, 0.1f);

        return (hitLeft || hitRight);
    }

    private void Move(float delta)
    {
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.layerMask = groundCollisionLayers;
        List<RaycastHit2D> collisionsList = new List<RaycastHit2D>();
        
        int collisionCount = rigidbody2d.Cast(velocity.normalized, contactFilter, collisionsList, (velocity * delta).magnitude);
        if (collisionCount > 0)
        {
            Vector2 newVelocity = new Vector2(velocity.x, 0f);
            collisionCount = rigidbody2d.Cast(newVelocity.normalized, contactFilter, collisionsList, (newVelocity * delta).magnitude);
            if (collisionCount == 0)
            {
                velocity = newVelocity;
            }
            else
            {
                newVelocity = new Vector2(0f, velocity.y);
                collisionCount = rigidbody2d.Cast(newVelocity.normalized, contactFilter, collisionsList, (newVelocity * delta).magnitude);
                if (collisionCount == 0)
                {
                    velocity = newVelocity;
                }
                else
                {
                    velocity.x = 0f;
                    velocity.y = 0f;
                }
            }
        }

        rigidbody2d.MovePosition(transform.position + (velocity * delta));
    }

    private void CalculateVelocity(float delta)
    {
        CheckOnGround();
        onGround = CheckOnGround();

        float acceleration = onGround ? accel*speed*delta : airAccel*speed*delta;

        if (Input.GetKey(keyLeft) ^ Input.GetKey(keyRight))
        {
            if (Input.GetKey(keyLeft))
            {
                velocity.x = Mathf.Lerp(velocity.x, -speed, acceleration);
            }
            if (Input.GetKey(keyRight))
            {
                velocity.x = Mathf.Lerp(velocity.x, speed, acceleration);
            }
        }
        else
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, acceleration);
        }

        if (onGround)
        {
            velocity.y = 0f;
        }
        else
        {
            velocity.y -= gravity;
            velocity.y = Math.Max(velocity.y, -terminalVelocity);
        }

        if (cueJump)
        {
            if (onGround)
            {
                velocity.y = jumpHeight;
            }
        }

        Move(delta);
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            CalculateVelocity(Time.fixedDeltaTime);
        }

    }

    private void Update()
    {
        if (IsOwner)
        {
            CheckJumpInput();
        }
    }
}
