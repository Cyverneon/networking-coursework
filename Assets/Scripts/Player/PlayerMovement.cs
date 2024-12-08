using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Speed")]
    [Tooltip("Player speed in units per second")]
    [SerializeField] private float _speed = 2f;
    [Tooltip("Rate of acceleration in units*speed per second (i.e, if speed is 4, accel of 1 will accelerate at 4 units per second)")]
    [SerializeField] private float _accel = 1f;
    [Tooltip("Rate of acceleration in the air")]
    [SerializeField] private float _airAccel = 0.5f;

    [Header("Jumping")]
    [Tooltip("Jump height in units")]
    [SerializeField] private float _jumpHeight = 50f;
    [Tooltip("Time in seconds player can press jump while about to hit the ground and still jump")]
    [SerializeField] private float _jumpBuffer = 0.1f;
    [Tooltip("Layers which player can jump off if they are detected to stand on")]
    [SerializeField] private LayerMask _collisionLayers;

    [Header("Gravity")]
    [Tooltip("Force of gravity in units")]
    [SerializeField] private float _gravity = 9.8f;
    [Tooltip("Maximum falling speed")]
    [SerializeField] private float _terminalVelocity = 50f;

    [SerializeField] private Rigidbody2D.SlideMovement _slideMovement;

    [Header("Controls")]
    [SerializeField] private KeyCode _keyLeft = KeyCode.A;
    [SerializeField] private KeyCode _keyRight = KeyCode.D;
    [SerializeField] private KeyCode _keyJump = KeyCode.J;

    private Rigidbody2D _rigidbody2d;
    private BoxCollider2D _collider2d;

    private Vector2 _velocity = new Vector2(0f, 0f);

    private Vector3 _feetOffsetLeft = new Vector3(0f, 0f, 0f);
    private Vector3 _feetOffsetRight = new Vector3(0f, 0f, 0f);

    private float _jumpTimer = 0f;
    private bool _cuedJump = false;
    private bool _onGround = true;

    public override void OnNetworkSpawn()
    {
        _rigidbody2d = GetComponent<Rigidbody2D>();
        _collider2d = GetComponent<BoxCollider2D>();

        _feetOffsetLeft.y = -(_collider2d.bounds.extents.y);
        _feetOffsetLeft.x = -(_collider2d.bounds.extents.x) + 0.05f;

        _feetOffsetRight.y = -(_collider2d.bounds.extents.y);
        _feetOffsetRight.x = (_collider2d.bounds.extents.x) - 0.05f;

    }

    private void CheckJumpInput()
    {
        if (Input.GetKeyDown(_keyJump))
        {
            _cuedJump = true;
            _jumpTimer = _jumpBuffer;
        }
        if (_jumpTimer > 0)
        {
            _jumpTimer -= Time.deltaTime;
            if (_jumpTimer < 0)
            {
                _cuedJump = false;
            }
        }
    }

    private bool CheckOnGround()
    {
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position + _feetOffsetLeft, -Vector2.up, 0.1f, _collisionLayers);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position + _feetOffsetRight, -Vector2.up, 0.1f, _collisionLayers);
        //Debug.DrawRay(transform.position + feetOffsetLeft, -Vector2.up, Color.red, 0.1f);
        //Debug.DrawRay(transform.position + feetOffsetRight, -Vector2.up, Color.red, 0.1f);

        return (hitLeft || hitRight);
    }

    private void CalculateVelocity(float delta)
    {
        CheckOnGround();
        _onGround = CheckOnGround();

        float acceleration = _onGround ? _accel*_speed*delta : _airAccel*_speed*delta;

        if (Input.GetKey(_keyLeft) ^ Input.GetKey(_keyRight))
        {
            if (Input.GetKey(_keyLeft))
            {
                _velocity.x = Mathf.Lerp(_velocity.x, -_speed, acceleration);
            }
            if (Input.GetKey(_keyRight))
            {
                _velocity.x = Mathf.Lerp(_velocity.x, _speed, acceleration);
            }
        }
        else
        {
            _velocity.x = Mathf.Lerp(_velocity.x, 0, acceleration);
        }

        if (_onGround)
        {
            _velocity.y = 0f;
        }
        else
        {
            _velocity.y -= _gravity;
            _velocity.y = Math.Max(_velocity.y, -_terminalVelocity);
        }

        if (_cuedJump)
        {
            if (_onGround)
            {
                _velocity.y = _jumpHeight;
            }
        }

        //Vector2 targetMovement = CollideAndSlide(velocity * delta, transform.position, 0);
        //rigidbody2d.MovePosition(transform.position + new Vector3(targetMovement.x, targetMovement.y, 0f));
        _rigidbody2d.Slide(_velocity, delta, _slideMovement);

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
