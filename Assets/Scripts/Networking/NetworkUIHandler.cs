using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkUIHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField _ipInput;
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _joinButton;

    public void Start()
    {
        _ipInput.text = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address;
        _ipInput.onValueChanged.AddListener(delegate { UpdateIP(); });

        _hostButton.onClick.AddListener(delegate { StartHost(); });
        _joinButton.onClick.AddListener(delegate { StartClient(); });
    }

    void UpdateIP()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = _ipInput.text;
    }

    void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.SceneManager.LoadScene("TestScene", LoadSceneMode.Single);
    }

    void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("TestScene", LoadSceneMode.Single);
    }
}
