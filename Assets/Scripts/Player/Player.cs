using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 100;

    private int _health;

    private PlayerEffects _playerEffects;

    public void TakeDamage(int damage, Vector2 direction)
    {
        _health -= damage;
        if (_health <= 0)
        {
            // test death;
        }
        else
        {
            _playerEffects.UpdateHealthbar(_health);
            Debug.Log(_health);
        }
    }

    void Start()
    {
        _playerEffects = GetComponent<PlayerEffects>();

        _health = _maxHealth;
    }

    void Update()
    {
        
    }
}
