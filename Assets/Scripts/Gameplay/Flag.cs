using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;

public class Flag : NetworkBehaviour
{
    private TextMeshProUGUI _playerCounterText;

    private int _players = 0;

    void Awake()
    {
        _playerCounterText = gameObject.transform.Find("Canvas/PlayerCounterText").GetComponent<TextMeshProUGUI>();
    }

    override public void OnNetworkSpawn()
    {
        CheckTotalPlayersServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void CheckTotalPlayersServerRpc()
    {
        CheckTotalPlayers();
    }

    private void CheckTotalPlayers()
    {
        int totalPlayers = NetworkManager.Singleton.ConnectedClientsList.Count;
        if (_players == totalPlayers)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("WinMenu", LoadSceneMode.Single);
        }
        UpdateCounterDisplayRpc(_players, totalPlayers);

    }

    [Rpc(SendTo.Everyone)]
    private void UpdateCounterDisplayRpc(int players, int totalPlayers)
    {
        _playerCounterText.text = "Players: " + players + "/" + totalPlayers;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServer)
        {
            _players++;
            CheckTotalPlayers();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (IsServer)
        {
            _players--;
            CheckTotalPlayers();
        }
    }
}
