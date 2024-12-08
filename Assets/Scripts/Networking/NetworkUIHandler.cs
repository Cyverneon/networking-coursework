using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkUIHandler : MonoBehaviour
{
    void UpdateIP(string newAddress)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = newAddress;
    }

    void UpdatePort(ushort newPort)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = newPort;
    }

    void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
}
