using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkUIHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private List<Button> _colourButtons;
    [SerializeField] private Image _colourSelectedImg;

    [SerializeField] private TMP_InputField _ipInput;
    [SerializeField] private Button _joinButton;

    [SerializeField] private List<Button> _levelButtons;
    [SerializeField] private List<string> _levelSceneNames;
    [SerializeField] private Button _hostButton;

    private string _sceneName = "TestScene";

    private string _playerName = "Player";
    private Color _playerColor = Color.red;

    public void Start()
    {
        // Player Settings
        _nameInput.text = "Player";
        UpdatePlayerName();
        _nameInput.onValueChanged.AddListener(delegate { UpdatePlayerName(); });

        if (_colourButtons.Count == 0)
        {
            Debug.LogError("No colour buttons");
        }
        else
        {
            for (int i = 0; i < _colourButtons.Count; i++)
            {
                int levelIndex = i;
                _colourButtons[i].onClick.AddListener(delegate { UpdateColour(levelIndex); });
            }
            UpdateColour(0);
        }

        // Hosting
        _hostButton.onClick.AddListener(delegate { StartHost(); });

        if (_levelButtons.Count != _levelSceneNames.Count)
        {
            Debug.LogError("Level buttons must correspond to level scene names (mismatching lengths");
        }
        else
        {
            for (int i = 0; i < _levelButtons.Count; i++)
            {
                int levelIndex = i;
                _levelButtons[i].onClick.AddListener(delegate { UpdateLevelName(levelIndex); });
            }
        }

        // Joining
        _ipInput.text = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address;
        _ipInput.onValueChanged.AddListener(delegate { UpdateIP(); });
        _joinButton.onClick.AddListener(delegate { StartClient(); });
    }

    private void UpdateLevelName(int i)
    {
        _sceneName = _levelSceneNames[i];
    }

    private void UpdatePlayerName()
    {
        GameManager.instance._localPlayerName = _nameInput.text;
    }

    private void UpdateColour(int i)
    {
        GameManager.instance._localPlayerColor = _colourButtons[i].image.color;
        _colourSelectedImg.transform.SetParent(_colourButtons[i].transform, false);
    }

    private void UpdateIP()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = _ipInput.text;
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.SceneManager.LoadScene(_sceneName, LoadSceneMode.Single);
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene(_sceneName, LoadSceneMode.Single);
    }
}
