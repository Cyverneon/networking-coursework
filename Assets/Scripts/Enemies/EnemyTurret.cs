using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class EnemyTurret : Enemy
{
    [Tooltip("Amount of time before turret can try to shoot again")]
    [SerializeField] private float _projectileCooldown;
    [Tooltip("Range that turret will shoot at player if they are within it")]
    [SerializeField] private float _maxShootDistance;

    [SerializeField] private GameObject _bulletPrefab;

    private bool _canShoot = true;

    private IEnumerator Cooldown()
    {
        _canShoot = false;
        yield return new WaitForSeconds(_projectileCooldown);
        _canShoot = true;
    }

    private void CheckProjectile()
    {
        if (_canShoot)
        {
            Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
            List<float> distances = new List<float>();
            foreach (Player player in players)
            {
                distances.Add(Math.Abs((player.transform.position - transform.position).magnitude));
            }

            int playerIndex = 0;
            float minDist = distances[0];
            for (int i = 1; i < distances.Count; i++)
            {
                if (distances[i] < minDist)
                {
                    minDist = distances[i];
                    playerIndex = i;
                }
            }

            if (minDist <= _maxShootDistance)
            {
                ShootProjectile(players[playerIndex].transform.position);
            }
            StartCoroutine(Cooldown());
        }
    }

    private void ShootProjectile(Vector3 targetPos)
    {
        GameObject projectile = Instantiate(_bulletPrefab, transform.position, Quaternion.identity);
        projectile.GetComponent<NetworkObject>().Spawn(true);
        projectile.GetComponent<Bullet>().SetDirectionRpc((targetPos - transform.position).normalized);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            CheckProjectile();
        }
    }
}
