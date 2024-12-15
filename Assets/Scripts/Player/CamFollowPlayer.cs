using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CamFollowPlayer : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private Tilemap _tilemap;

    private GameObject _player;
    private Vector3 _targetPos;
    private Rect _constraints;

    private void getConstraints()
    {
        _constraints = new Rect(0, 0, 0, 0);
        Camera cam = GetComponent<Camera>();
        _constraints.yMin = _tilemap.localBounds.min.y + cam.orthographicSize;
        _constraints.yMax = _tilemap.localBounds.max.y - cam.orthographicSize;
        _constraints.xMin = _tilemap.localBounds.min.x + (cam.orthographicSize * cam.aspect);
        _constraints.xMax = _tilemap.localBounds.max.x - (cam.orthographicSize * cam.aspect);
    }

    // Start is called before the first frame update
    void Start()
    {
        _player = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        _targetPos.z = transform.position.z;
        getConstraints();
    }

    // Update is called once per frame
    void Update()
    {
        if (_player != null)
        {
            if (_player.transform.position.x >= _constraints.xMin && _player.transform.position.x <= _constraints.xMax)
                _targetPos.x = _player.transform.position.x;
            if (_player.transform.position.y >= _constraints.yMin && _player.transform.position.y <= _constraints.yMax)
                _targetPos.y = _player.transform.position.y;

            transform.position = Vector3.Lerp(transform.position, _targetPos, _speed * Time.deltaTime);
        }
    }
}
