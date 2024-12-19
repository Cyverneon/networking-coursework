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

    [Tooltip("Rate that player will flash while invulnerable after getting hit")]
    [SerializeField] private float _invulnerableFlashingRate;
    private float _invulnTimer = 0f;

    [Tooltip("Material to use for the player sprite")]
    [SerializeField] Material _material;

    private bool _walking;
    private bool _invulnerable;

    private Player _player;
    private PlayerMovement _playerMovement;
    private AudioSource _audioSource;
    private SpriteRenderer _spriteRenderer;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _healthText;

    private string _playerName;
    private Color _playerColor;

    private void OwnerCheckStates()
    {
        if (IsOwner)
        {
            // PlayerMovement only runs movement code if it's the player belonging to this client
            // for players not owned by this client IsWalking() will always be false
            // each client checks its own player and updates the value over the network if it's changed
            if (_playerMovement.IsWalking() != _walking)
            {
                UpdateWalkingRpc(_playerMovement.IsWalking());
            }

            if (_player.IsInvulnerable() != _invulnerable)
            {
                UpdateInvulnerableRpc(_player.IsInvulnerable());
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateWalkingRpc(bool walking)
    {
        _walking = walking;
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateInvulnerableRpc(bool invulnerable)
    {
        _invulnerable = invulnerable;
    }

    private void CheckFootsteps()
    {
        if (_walking && !_invulnerable)
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
        if (_invulnerable)
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
    public void UpdateNameRpc(string name)
    {
        _nameText.text = name;
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateSpriteMatColorRpc(Color color)
    {
        _spriteRenderer.material.color = color;
    }

    [Rpc(SendTo.Everyone)]
    public void UpdateHealthbarRpc(int health)
    {
        _healthText.text = "HP: " + health.ToString();
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

        _nameText = gameObject.transform.Find("Canvas/NameText").GetComponent<TextMeshProUGUI>();
        _healthText = gameObject.transform.Find("Canvas/HPText").GetComponent<TextMeshProUGUI>();
    }

    [Rpc(SendTo.Owner)]
    private void SetNameAndColorOwnerRpc()
    {
        UpdateNameRpc(PlayerInfoSingleton.instance.playerName);
        UpdateSpriteMatColorRpc(PlayerInfoSingleton.instance.playerColor);
    }

    public override void OnNetworkSpawn()
    {
        SetNameAndColorOwnerRpc();
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
