using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CamFollowPlayer : MonoBehaviour
{
    GameObject _player;
    // Start is called before the first frame update
    void Start()
    {
        _player = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(_player.transform.position.x, _player.transform.position.y, transform.position.z);
    }
}
