using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.ResLoader.VO
{
    public class AssetVO
    {
        private Object prefab;
        private List<Object> pool;

        public AssetVO(Object prefab)
        {
            this.prefab = prefab;
        }

        public AssetVO(string path)
        {
            AssetBundle ab = AssetBundleLoader.LoadAssetBundle(path);
            prefab = ab.LoadAsset<Object>(Path.GetFileName(path));
        }

        public Object GetInstance()
        {
            return Object.Instantiate(prefab);
        }
    }
}

