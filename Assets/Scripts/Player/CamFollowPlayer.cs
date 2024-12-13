using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CamFollowPlayer : MonoBehaviour
{
    private GameObject _player;
    private Vector3 _targetPos;

    [SerializeField] private float _speed;

    // Start is called before the first frame update
    void Start()
    {
        _player = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        _targetPos.z = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        _targetPos.x = _player.transform.position.x;
        _targetPos.y = _player.transform.position.y;

        transform.position = Vector3.Lerp(transform.position, _targetPos, _speed * Time.deltaTime);
    }
}
