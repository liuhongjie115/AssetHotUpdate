using Asset.Scripts.Util;
using Assets.Scripts.ResLoader.VO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.ResLoader
{
    public class AssetLoader
    {
        private static Dictionary<string, AssetVO> assetDict = new Dictionary<string, AssetVO>();


        static AssetLoader()
        {

        }

        public static void LoadNormal(string path, Action<AssetVO> loadComplete)
        {
            AssetVO assetVO = null;
#if UNITY_EDITOR
            if (!SystemConfig.isLoadAbRes)
            {
                if (assetDict.ContainsKey(path))
                {
                    assetVO = assetDict[path];
                }
                else
                {
                    string[] contents = Directory.GetFiles(Application.dataPath + "/OriginalResource/" + Path.GetDirectoryName(path));
                    foreach (string name in contents)
                    {
                        if (name.Contains(Path.GetFileName(path)) && !name.EndsWith(".meta"))
                        {
                            string dir = name;
                            dir = dir.Replace('\\', '/');
                            dir = dir.Substring(Application.dataPath.Length - 6);
                            UnityEngine.Object o = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dir);
                            assetVO = new AssetVO(o);
                            assetDict.Add(path, assetVO);
                        }
                    }
                }
                loadComplete(assetVO);
            }
            else
            {
#endif
                if (assetDict.ContainsKey(path))
                {
                    assetVO = assetDict[path];
                }
                else
                {
                    assetVO = new AssetVO(path);
                    assetDict.Add(path, assetVO);
                }
                loadComplete(assetVO);
            }
#if UNITY_EDITOR
        }
#endif
    }
}

