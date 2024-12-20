using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour 
{
    public static GameManager instance;

    public string _localPlayerName;
    public Color _localPlayerColor;

    public bool _localPlayerAlive = true;

    public void PlayerDeath()
    {
        _localPlayerAlive = false;
        Debug.Log("you died!!");
        // query all clients if their player is alive
        // if all dead, disconnect from network and load death screen
        // otherwise, display message and target new player with main camera
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this);
    }
}
