using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private Button _quitButton;

    void Start()
    {
        _quitButton.onClick.AddListener(delegate { Quit(); });
    }

    private void Quit()
    {
        Application.Quit();
    }
}
