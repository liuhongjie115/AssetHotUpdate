using Asset.Scripts.Util;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Manager;
using LuaInterface;
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
    }

    private void Start()
    {
        TimerManager.Init();
        ALog.Init();
        LuaFileUtils.Instance.beZip = SystemConfig.isLoadAbLua;
        Assets.Scripts.HotUpdate.AssetUpdate.StartUpdate(GameUpdateAfter);
    }

    private void GameUpdateAfter(bool success)
    {
        if(success)
        {
            LuaEngine.Instance.InitEngine(this);
        }
    }

    private void Update()
    {
        TimerManager.Update();
    }
}
