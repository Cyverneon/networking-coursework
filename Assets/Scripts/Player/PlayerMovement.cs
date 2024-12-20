using System;
using Unity.Collections.LowLevel.Unsafe;
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
    [Tooltip("Period of time at peak of player's jump where they will 'hover' for a second")]
    [SerializeField] private float _jumpApex = 0.1f;
    [Tooltip("Period of time after leaving the ground which the player can still jump")]
    [SerializeField] private float _coyoteTime = 0.1f;

    [Header("Gravity")]
    [Tooltip("Force of gravity in units")]
    [SerializeField] private float _gravity = 9.8f;
    [Tooltip("Maximum falling speed")]
    [SerializeField] private float _terminalVelocity = 50f;

    [Header("Slide Movement & Collision")]
    [Tooltip("Maximum amount of iterations for Rigidbody2D.Slide() to do when handling collisions")]
    [SerializeField] private int _slideIterations = 5;
    [Tooltip("Maximum angle for a surface to be slid along")]
    [SerializeField] private float _surfaceSlideAngle = 90f;
    [Tooltip("Up direction")]
    [SerializeField] private Vector2 _upDirection = new Vector2(0, 1);
    [Tooltip("Layers which the player will collide with")]
    [SerializeField] private LayerMask _collisionLayers;
    [Tooltip("Amount of knockback player recieves when colliding with a hazard")]
    [SerializeField] private float _knockbackDist;

    [Header("Controls")]
    [SerializeField] private KeyCode _keyLeft = KeyCode.A;
    [SerializeField] private KeyCode _keyRight = KeyCode.D;
    [SerializeField] private KeyCode _keyJump = KeyCode.J;

    private Rigidbody2D _rigidbody2d;
    private BoxCollider2D _collider2d;
    private Animator _animator;

    private Rigidbody2D.SlideMovement _slideMovement;

    private Vector2 _velocity = new Vector2(0f, 0f);

    private Vector3 _feetOffsetLeft = new Vector3(0f, 0f, 0f);
    private Vector3 _feetOffsetRight = new Vector3(0f, 0f, 0f);

    private float _jumpTimer = 0f;
    private float _jumpApexTimer = 0f;
    private float _coyoteTimer = 0f;

    private bool _cuedJump = false;
    private bool _onGround = true;
    private bool _jumping = false;
    private bool _appliedJumpApex = false;

    [HideInInspector] public Vector2 _additionalVel = Vector2.zero;
    [HideInInspector] public Vector2 _cuedKnockback = Vector2.zero;

    private void Awake()
    {
        GetComponentRefs();
    }

    public override void OnNetworkSpawn()
    {
        GetFeetOffset();
        SetSlideMovement();
        Respawn();
    }

    private void GetComponentRefs()
    {
        _rigidbody2d = GetComponent<Rigidbody2D>();
        _collider2d = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
    }

    // this gets offsets from position to points at the bottom left and bottom right of
    // the collider to raycast from when checking if on ground,
    // collider can be adjusted without having to manually change anything else
    private void GetFeetOffset()
    {
        _feetOffsetLeft.y = -(_collider2d.bounds.extents.y);
        _feetOffsetLeft.x = -(_collider2d.bounds.extents.x);

        _feetOffsetRight.y = -(_collider2d.bounds.extents.y);
        _feetOffsetRight.x = (_collider2d.bounds.extents.x);
    }

    private void SetSlideMovement()
    {
        _slideMovement.maxIterations = _slideIterations;
        _slideMovement.surfaceSlideAngle = _surfaceSlideAngle;
        _slideMovement.gravitySlipAngle = 0f;
        _slideMovement.surfaceUp = _upDirection;
        _slideMovement.surfaceAnchor = new Vector2(0, -0.1f);
        _slideMovement.gravity = Vector2.zero;
        _slideMovement.startPosition = Vector2.zero;
        _slideMovement.selectedCollider = _collider2d;
        _slideMovement.layerMask = _collisionLayers;
        _slideMovement.useLayerMask = true;
        _slideMovement.useStartPosition = true;
        _slideMovement.useNoMove = true;
        _slideMovement.useSimulationMove = false;
        _slideMovement.useAttachedTriggers = false;
    }

    private void UpdateAnimatorParams()
    {
        _animator.SetBool("playerGrounded", CheckOnGround());
        _animator.SetFloat("velocityX", _velocity.x);
        _animator.SetFloat("velocityY", _velocity.y);
    }

    public void Respawn()
    {
        transform.position = Vector3.zero;
        _velocity = Vector2.zero;
        _additionalVel = Vector2.zero;
    }

    private void CheckJumpInput()
    {
        // you can cue up a jump a little before actually hitting the ground
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

    public bool IsWalking()
    {
        return CheckOnGround() && (_velocity.x < -1f || _velocity.x > 1f);
    }

    private void CalculateVelocity(float delta)
    {
        CheckOnGround();
        _onGround = CheckOnGround();

        float acceleration = _onGround ? _accel * _speed * delta : _airAccel * _speed * delta;

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

        _velocity.y -= (_jumping && _velocity.y >= 0 && !Input.GetKey(_keyJump)) ? _gravity * 2 : _gravity;
        _velocity.y = Math.Max(_velocity.y, -_terminalVelocity);

        // reset y vel/jumping state/coyote time when on ground
        if (_onGround)
        {
            _velocity.y = 0;
            _jumping = false;
            _coyoteTimer = _coyoteTime;

        }
        else if (_coyoteTimer >= 0)
        {
            _coyoteTimer -= delta;
        }

        // jump
        if (_cuedJump && (_onGround || _coyoteTimer >= 0))
        {
            _jumping = true;
            _coyoteTimer = 0;
            _appliedJumpApex = false;
            _velocity.y = _jumpHeight;
        }

        // jump apex hover
        if (_jumping && _velocity.y <= 0)
        {
            if (!_appliedJumpApex)
            {
                _jumpApexTimer = _jumpApex;
                _appliedJumpApex = true;
            }
            else
            {
                _jumpApexTimer -= delta;
                if (_jumpApexTimer >= 0)
                {
                    _velocity.y = 0;
                }
            }
        }

        if (_cuedKnockback != Vector2.zero)
        {
            _velocity = _cuedKnockback * _knockbackDist;
            _jumping = false;
            _cuedKnockback = Vector2.zero;
        }

        _slideMovement.startPosition = transform.position;

        Rigidbody2D.SlideResults slideResults = _rigidbody2d.Slide(_velocity, delta, _slideMovement);

        // Velocity should respond to the result of trying to move the player
        // E.g., if the player is holding left/right and pushing into a wall, their x vel should be set to 0
        if (slideResults.position.x == transform.position.x)
            _velocity.x = 0;

        if (slideResults.position.y == transform.position.y)
            _velocity.y = 0;

        // Jump apex hover is not desired if the player bonks their head into a ceiling so just act like we already finished applying it
        if (slideResults.slideHit.normal == -_upDirection)
        {
            _appliedJumpApex = true;
            _jumpApexTimer = 0;
        }

        transform.position = slideResults.position;

        UpdatePosAdditionalVel();
    }

    // move the player along with a moving platform. This is a bad solution but the
    // typical approach of parenting the platform to the player is annoying to make work with networking
    private void UpdatePosAdditionalVel()
    {
        transform.position += new Vector3(_additionalVel.x, _additionalVel.y, transform.position.z) * Time.deltaTime;

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
            UpdateAnimatorParams();
        }
    }

}