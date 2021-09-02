using Asset.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ResLoader
{
    public class AssetBundleLoader
    {
        private static Dictionary<string, AssetBundle> abDict = new Dictionary<string, AssetBundle>();

        public static AssetBundle LoadAssetBundle(string path)
        {
            LoadAllAssetBundleDepsWithOutMy(path);
            AssetBundle mainAb = null;
            if (!abDict.ContainsKey(path))
            {
                mainAb = AssetBundle.LoadFromFile(SystemConfig.PACK_OUT_RES_PATH + "/" + path);
                abDict.Add(path, mainAb);
            }
            else
            {
                mainAb = abDict[path];
            }
            return mainAb;
        }

        private static void LoadAllAssetBundleDepsWithOutMy(string path)
        {
            AssetBundle manifestAB = AssetBundle.LoadFromFile(SystemConfig.PACK_OUT_RES_PATH + "/ABres");
            AssetBundleManifest manifest = manifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            string[] strs = manifest.GetAllDependencies(path);
            foreach (string name in strs)
            {
                if (!abDict.ContainsKey(name))
                {
                    AssetBundle ab = AssetBundle.LoadFromFile(SystemConfig.PACK_OUT_RES_PATH + "/" + name);
                    abDict.Add(name, ab);
                }
            }
        }
    }
}

