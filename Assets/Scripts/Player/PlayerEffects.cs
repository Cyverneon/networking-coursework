using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;


public class PlayerEffects : NetworkBehaviour
{
    [Header("Audio clips")]
    [Tooltip("Audio clips that might be played as a footstep")]
    [SerializeField] private AudioClip[] _clipFootsteps;
    [Tooltip("Audio clip to be played when damage is taken")]
    [SerializeField] private AudioClip _clipDamage;

    [Tooltip("Time in seconds between playing footstep")]
    [SerializeField] private float _footstepDelay;
    private float _footstepTimer = 0f;

    [SerializeField] private float _invulnerableFlashingRate;
    private float _invulnTimer = 0f;

    private bool _isWalking;
    private bool _isInvulnerable;

    private Player _player;
    private PlayerMovement _playerMovement;
    private AudioSource _audioSource;
    private SpriteRenderer _spriteRenderer;
    private TextMeshProUGUI _healthText;

    private void OwnerCheckStates()
    {
        if (IsOwner)
        {
            // PlayerMovement only runs movement code if it's the player belonging to this client
            // So for other players IsWalking() will always return false
            // Each client will check if its player is walking every frame, and only update this scripts value over the network if it should actually change
            if (_playerMovement.IsWalking() != _isWalking)
            {
                UpdateWalkingRpc(_playerMovement.IsWalking());
            }

            if (_player.IsInvulnerable() != _isInvulnerable)
            {
                UpdateInvulnerableRpc(_player.IsInvulnerable());
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateWalkingRpc(bool walking)
    {
        _isWalking = walking;
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateInvulnerableRpc(bool invulnerable)
    {
        _isInvulnerable = invulnerable;
    }

    private void CheckFootsteps()
    {
        if (_isWalking)
        {
            _footstepTimer -= Time.deltaTime;
            if (_footstepTimer <= 0f)
            {
                _footstepTimer = _footstepDelay;
                PlayClip(_clipFootsteps[Random.Range(0, _clipFootsteps.Length)]);
            }
        }
        else
        {
            _footstepTimer = 0f;
        }
    }

    private void CheckInvulnerable()
    {
        if (_isInvulnerable)
        {
            _invulnTimer -= Time.deltaTime;
            if (_invulnTimer <= 0f)
            {
                _spriteRenderer.enabled = !_spriteRenderer.enabled;
                _invulnTimer = _invulnerableFlashingRate;
            }
        }
        else
        {
            _spriteRenderer.enabled = true;
        }
    }

    private void PlayClip(AudioClip clip)
    {
        _audioSource.Stop();
        _audioSource.clip = clip;
        _audioSource.Play();
    }

    [Rpc(SendTo.Everyone)]
    public void UpdateHealthbarRpc(int health)
    {
        _healthText.text = "Player Name\nHP: " + health.ToString();
    }

    [Rpc(SendTo.Everyone)]
    public void PlayDamageSoundRpc()
    {
        PlayClip(_clipDamage);
    }

    private void GetComponentRefs()
    {
        _player = GetComponent<Player>();
        _playerMovement = GetComponent<PlayerMovement>();
        _audioSource = GetComponent<AudioSource>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _healthText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Awake()
    {
        GetComponentRefs();
    }

    void Update()
    {
        OwnerCheckStates();
        CheckFootsteps();
        CheckInvulnerable();
    }
}
