using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private int _maxHealth = 100;

    private int _health;

    private PlayerEffects _playerEffects;
    private PlayerMovement _playerMovement;


    public void TakeDamage(int damage, Vector2 direction)
    {
        if (IsOwner)
        {
            _health -= damage;
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

    [Rpc(SendTo.Owner)]
    private void UpdateHealthbarOwnerRpc()
    {
        _playerEffects.UpdateHealthbarRpc(_health);
    }

    private void Awake()
    {
        _playerEffects = GetComponent<PlayerEffects>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    public override void OnNetworkSpawn()
    {
        _health = _maxHealth;
        UpdateHealthbarOwnerRpc();
    }

    void Update()
    {
        
    }
}
