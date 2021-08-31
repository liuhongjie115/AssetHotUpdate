using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class GameStart : MonoBehaviour
{
    private static GameStart _instance;

    public static GameStart Instance { get => _instance;}

    private void Awake()
    {
#if UNITY_EDITOR
        if (!Directory.Exists(Application.persistentDataPath + "/Res"))
        {
            Debug.Log("±à¼­Æ÷´´½¨");
            Directory.CreateDirectory(Application.persistentDataPath + "/Res");
        }
#endif
        _instance = this;
        Assets.Scripts.HotUpdate.AssetUpdate.StartUpdate();
    }

    private void Start()
    {
        Assets.Scripts.HotUpdate.AssetUpdate.StartUpdate();
    }
}
