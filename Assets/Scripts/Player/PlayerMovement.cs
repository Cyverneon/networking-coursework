using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [Tooltip("Player speed in units per second")]
    [SerializeField] private float speed = 2f;
    [Tooltip("Jump height in units")]
    [SerializeField] private float jumpHeight = 50f;
    [Tooltip("Time in seconds player can press jump while about to hit the ground and still jump")]
    [SerializeField] private float jumpLeniency = 0.1f;

    [Tooltip("Varience from upwards angle that is still considered \"ground\" (can jump off) ")]
    [SerializeField] private float groundAngle = 30;

    private float jumpTimer = 0f;
    private bool cueJump = false;
    private bool onGround = true;

    [Header("References")]
    [SerializeField] private Collider2D collider2d;
    [SerializeField] private Rigidbody2D rigidBody;

    private void Movement(float delta)
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= new Vector3(speed * delta, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += new Vector3(speed * delta, 0f, 0f);
        }

        if (cueJump)
        {
            if (onGround)
            {
                rigidBody.AddForce(Vector2.up * jumpHeight);
                cueJump = false;
            }
        }

        LayerMask layerMask = LayerMask.GetMask("Environment");

        onGround = collider2d.IsTouchingLayers(layerMask);
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
            if (Input.GetKeyDown(KeyCode.J))
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
    }
}
