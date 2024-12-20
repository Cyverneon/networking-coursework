using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class MoveLine : Mover
{
    [Tooltip("Amount of time (seconds) it takes to move from startPos to endPos")]
    [SerializeField] float _travelTime;
    [Tooltip("First of postions the objec moves back and fourth")]
    [SerializeField] Vector3 _startPos;
    [Tooltip("Second of positions the object moves back and fourth")]
    [SerializeField] Vector3 _endPos;

    float _timer = 0f;

    private bool _goingForward = true;
    public override void OnNetworkSpawn()
    {
        SyncPositionServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void SyncPositionServerRpc()
    {
        SyncPositionClientRpc(transform.position, _goingForward);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SyncPositionClientRpc(Vector3 pos, bool goingForward)
    {
        transform.position = pos;
        _goingForward = goingForward;
    }

    override protected Vector3 GetNextPos()
    {
        _timer += Time.deltaTime / _travelTime;

        Vector3 nextPos;
        if (_goingForward)
            nextPos = Vector3.Lerp(_startPos, _endPos, _timer);
        else
            nextPos = Vector3.Lerp(_endPos, _startPos, _timer);

        if (_timer >= 1f)
        {
            _timer = 0f;
            _goingForward = !_goingForward;
            SyncPositionServerRpc();
        }

        return nextPos;
    }

}
