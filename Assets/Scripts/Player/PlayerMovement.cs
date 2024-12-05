using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using System;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [Tooltip("Player speed in units per second")]
    [SerializeField] private float speed = 2f;
    [Tooltip("Rate of acceleration in units*speed per second (i.e, if speed is 4, accel of 1 will accelerate at 4 units per second)")]
    [SerializeField] private float accel = 1f;
    [Tooltip("Rate of acceleration in the air")]
    [SerializeField] private float airAccel = 0.5f;
    [Tooltip("Jump height in units")]
    [SerializeField] private float jumpHeight = 50f;
    [Tooltip("Force of gravity in units")]
    [SerializeField] private float gravity = 9.8f;
    [Tooltip("Maximum falling speed")]
    [SerializeField] private float terminalVelocity = 50f;
    [Tooltip("Time in seconds player can press jump while about to hit the ground and still jump")]
    [SerializeField] private float jumpLeniency = 0.1f;

    [Tooltip("Varience from upwards angle that is still considered \"ground\" (can jump off) ")]
    [SerializeField] private float groundAngle = 30;

    [SerializeField] private LayerMask collisionLayerMask;

    [Header("Controls")]
    [SerializeField] private KeyCode keyLeft = KeyCode.A;
    [SerializeField] private KeyCode keyRight = KeyCode.D;
    [SerializeField] private KeyCode keyJump = KeyCode.J;

    private float jumpTimer = 0f;
    private bool cueJump = false;
    private bool onGround = true;

    [Header("References")]
    [SerializeField] private Collider2D collider2d;
    [SerializeField] private Rigidbody2D rigidBody;

    private Vector2 velocity = new Vector2(0f, 0f);

    private void checkJumpInput()
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

    private void Movement(float delta)
    {
        onGround = collider2d.IsTouchingLayers(collisionLayerMask);

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

        transform.position += new Vector3(velocity.x, velocity.y, 0f) * delta;


        /*        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, 200.0f, layerMask);
                if (hit)
                {
                    Debug.Log("raycast hit");
                }
                else
                {
                    Debug.Log("raycast not hit");
                }*/
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision entered");
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            Movement(Time.deltaTime);
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            checkJumpInput();
        }
    }
}
