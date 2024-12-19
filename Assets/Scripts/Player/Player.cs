using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : NetworkBehaviour
{
    [Tooltip("How much health the player has")]
    [SerializeField] private int _maxHealth = 100;
    private int _health;

    private PlayerEffects _playerEffects;
    private PlayerMovement _playerMovement;

    [Tooltip("How long (in seconds) to be invulnerable after getting hit")]
    [SerializeField] private float _hitInvulnerability = 0.2f;
    private float _hitInvulnerableTimer = 0f;

    public void TakeDamage(int damage, Vector2 direction)
    {
        if (IsOwner && _hitInvulnerableTimer <= 0f)
        {
            _health -= damage;
            _hitInvulnerableTimer = _hitInvulnerability;
            _playerMovement._cuedKnockback = direction;
            if (_health < 0)
            {
                // die
            }
            else
            {
                _playerEffects.UpdateHealthbarRpc(_health);
                _playerEffects.PlayDamageSoundRpc();
            }
        }
    }

    private void CheckInvulnerableTimer()
    {
        if (_hitInvulnerableTimer > 0f)
        {
            _hitInvulnerableTimer -= Time.deltaTime;
        }
    }

    public bool IsInvulnerable()
    {
        return (_hitInvulnerableTimer > 0f);
    }

    private void Awake()
    {
        _playerEffects = GetComponent<PlayerEffects>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    [Rpc(SendTo.Owner)]
    private void UpdateHealthbarOwnerRpc()
    {
        _playerEffects.UpdateHealthbarRpc(_health);
    }
    public override void OnNetworkSpawn()
    {
        _health = _maxHealth;
        UpdateHealthbarOwnerRpc();
    }

    void Update()
    {
        if (IsOwner)
        {
            CheckInvulnerableTimer();
            Debug.Log("invuln: " + _hitInvulnerableTimer);
        }
    }
}
