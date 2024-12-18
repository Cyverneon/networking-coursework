using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [Tooltip("Whether to apply calculated movement, or just update it")]
    [SerializeField] bool _move = true;

    private Vector3 _movement = Vector3.zero;

    protected virtual Vector3 GetNextPos()
    {
        return transform.position;
    }

    protected void Move()
    {
        _movement = GetNextPos() - transform.position;
        if (_move)
        {
            transform.position += _movement;
        }
    }

    public Vector3 GetMovement()
    {
        return _movement / Time.deltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
}
