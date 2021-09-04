using Asset.Scripts.Util;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Manager;
using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    private static GameStart _instance;

    public static GameStart Instance { get => _instance;}

    private void Awake()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Res"))
        {
            Debug.Log("编辑器创建");
            Directory.CreateDirectory(Application.persistentDataPath + "/Res");
        }
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
            LoadPanel.Instance.gameObject.SetActive(false);
        }
        else
        {
            LoadPanel.Instance.lblProcess.text = "更新失败，请重新启动";
        }
    }

    private void Update()
    {
        TimerManager.Update();
    }
}
