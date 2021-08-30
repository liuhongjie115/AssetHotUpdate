using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    private static GameStart _instance;

    public static GameStart Instance { get => _instance;}

    private void Awake()
    {
        _instance = this;
        Assets.Scripts.HotUpdate.AssetUpdate.StartUpdate();
    }
}
