using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MoveCircle : MonoBehaviour
{
    [Tooltip("Speed of movement in degrees per second")]
    [SerializeField] private float _speed = 180f;
    [Tooltip("Distance from orbit pos that the object will circle at")]
    [SerializeField] private float _radius = 1f;
    [Tooltip("Center of the circle that the object will move around")]
    [SerializeField] private Vector3 _orbitPos = Vector3.zero;
    [Tooltip("Move clockwise or anticlockwise?")]
    [SerializeField] private bool _clockwise = true;

    private float _currentAngle = 0f;
    private float _currentAngleRad = 0f;

    Vector3 GetNextPos()
    {
        _currentAngle += (_clockwise ? _speed : -_speed) * Time.deltaTime;
        _currentAngle = _currentAngle % 360;

        _currentAngleRad = (_currentAngle * Mathf.PI) / 180;

        Vector3 newPos = transform.position;
        newPos.x = _orbitPos.x + (Mathf.Sin(_currentAngleRad) * _radius);
        newPos.y = _orbitPos.y + (Mathf.Cos(_currentAngleRad) * _radius);
        return newPos;
    }

    void Update()
    {
        transform.position = GetNextPos();
    }
}
