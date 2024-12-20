using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    [Tooltip("Damage player recieves when colliding with this enemy")]
    [SerializeField] private int _damage;

    [SerializeField] bool _destroyOnCollide = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Player>() != null)
        {
            collision.gameObject.GetComponent<Player>().TakeDamage(_damage, (collision.transform.position - transform.position).normalized);
        }
        if (_destroyOnCollide)
        {
            DestroyServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void DestroyServerRpc()
    {
        Destroy(this);
    }
}
