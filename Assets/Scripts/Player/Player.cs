using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    [Tooltip("Amount of time the player takes to die (to give anim/sound feedback rather than immediately dying")]
    [SerializeField] private float _timeToDie = 0.3f;

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(_timeToDie);
        GameManager.instance.PlayerDeath();
        Destroy(this.gameObject);
    }

    public void TakeDamage(int damage, Vector2 direction)
    {
        if (IsOwner && _hitInvulnerableTimer <= 0f)
        {
            _health -= damage;
            if (_health <= 0)
            {
                _playerMovement.enabled = false;
                GetComponent<Collider2D>().enabled = false;
                _playerEffects.PlayDeathEffects();
                DieServerRpc();
            }
            else
            {
                _hitInvulnerableTimer = _hitInvulnerability;
                _playerMovement._cuedKnockback = direction;
                _playerEffects.UpdateHealthbarRpc(_health, _maxHealth);
                _playerEffects.PlayDamageSoundRpc();
            }
        }
    }

    // Die() couroutine will destroy the network object
    // this has to be done on the server
    [Rpc(SendTo.Server)]
    private void DieServerRpc()
    {
        StartCoroutine(Die());
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
        _playerEffects.UpdateHealthbarRpc(_health, _maxHealth);
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
        }
    }
}
