using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float _speed = 6f;
    [SerializeField] private float _maxLifeTime = 4f;

    [SerializeField] private int _damage = 10;

    private Vector3 _direction;

    private float _lifeTime;

    public override void OnNetworkSpawn()
    {
        SetDirectionServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void SetDirectionServerRpc()
    {
        SetDirectionRpc(_direction);
    }

    [Rpc(SendTo.Everyone)]
    public void SetDirectionRpc(Vector3 direction)
    {
        _direction = direction;
    }

    [Rpc(SendTo.Server)]
    private void DestroyRpc()
    {
        GetComponent<NetworkObject>().Despawn(true);
        Destroy(this);
    }
    
    void Update()
    {
        transform.position += _speed * _direction * Time.deltaTime;
        _lifeTime += Time.deltaTime;

        if ( _lifeTime > _maxLifeTime )
        {
            DestroyRpc();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Player>() != null)
        {
            collision.gameObject.GetComponent<Player>().TakeDamage(_damage, (collision.transform.position - transform.position).normalized);
            DestroyRpc();
        }
    }
}
