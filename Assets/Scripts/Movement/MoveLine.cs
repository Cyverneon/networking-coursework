using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveLine : Mover
{
    [Tooltip("Units per second that object moves")]
    [SerializeField] float _speed;
    [Tooltip("First of postions the objec moves back and fourth")]
    [SerializeField] Vector3 _startPos;
    [Tooltip("Second of positions the object moves back and fourth")]
    [SerializeField] Vector3 _endPos;

    private float _timer = 0f;

    override protected Vector3 GetNextPos()
    {
        _timer += Time.deltaTime;
        return Vector3.zero;
    }

}
