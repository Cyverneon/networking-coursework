using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;


public class PlayerEffects : NetworkBehaviour
{
    [Tooltip("Audio clips that might be played as a footstep")]
    [SerializeField] private AudioClip[] _footstepClips;

    [Tooltip("Time in seconds between playing footstep")]
    [SerializeField] private float _footstepDelay;

    private PlayerMovement _playerMovement;
    private AudioSource _audioSource;
    private TextMeshProUGUI _healthText;

    private bool _isWalking;

    private float _footstepTimer = 0f;

    private void CheckFootsteps()
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
        }

        if (_isWalking)
        {
            _footstepTimer -= Time.deltaTime;
            if (_footstepTimer <= 0f)
            {
                _footstepTimer = _footstepDelay;
                PlayFootstep();
            }
        }
        else
        {
            _footstepTimer = 0f;
        }
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateWalkingRpc(bool walking)
    {
        _isWalking = walking;
    }

    private void PlayFootstep()
    {
        _audioSource.Stop();
        _audioSource.clip = _footstepClips[Random.Range(0, _footstepClips.Length)];
        _audioSource.Play();
    }

    public void UpdateHealthbar(int health)
    {
        _healthText.text = "Player Name\nHP: " + health.ToString();
    }


    void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _audioSource = GetComponent<AudioSource>();
        _healthText = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Update()
    {
        CheckFootsteps();
    }
}
