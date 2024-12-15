using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MoveCircle : MonoBehaviour
{
    [SerializeField] private float _rotSpeed = 5f;
    [SerializeField] private float _radius = 1f;
    [SerializeField] private Vector3 _startPos = Vector3.zero;

    private float _currentAngle = 0f;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = _startPos;
    }

    // Update is called once per frame
    void Update()
    {
        _currentAngle += _rotSpeed * Time.deltaTime;
        _currentAngle = _currentAngle % 360;

        Vector3 newPos = transform.position;
        newPos.x = _startPos.x + (Mathf.Sin(_currentAngle)*_radius);
        newPos.y = _startPos.y + (Mathf.Cos(_currentAngle)*_radius);
        transform.position = newPos;
    }
}
