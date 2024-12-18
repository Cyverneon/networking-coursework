using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    // each moving platform will handle the player on its own client instance 

    [SerializeField] private Collider2D _playerCaptureTrigger;

    private Collider2D _targetPlayer;
    private Mover _mover;

    private bool _capturedPlayer = false;

    private void Start()
    {
        _mover = GetComponent<Mover>();
        _targetPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (_capturedPlayer)
        {
            _targetPlayer.GetComponent<PlayerMovement>()._additionalVel = _mover.GetMovement();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_playerCaptureTrigger.IsTouching(_targetPlayer))
        {
            _capturedPlayer = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!_playerCaptureTrigger.IsTouching(_targetPlayer))
        {
            _capturedPlayer = false;
            _targetPlayer.GetComponent<PlayerMovement>()._additionalVel = Vector2.zero;
        }
    }
}
