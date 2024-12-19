using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoSingleton : MonoBehaviour 
{
    public static PlayerInfoSingleton instance;

    public string playerName;
    public Color playerColor;

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
