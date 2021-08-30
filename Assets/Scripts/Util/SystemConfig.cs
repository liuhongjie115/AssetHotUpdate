using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Asset.Scripts.Util
{
    public class SystemConfig
    {
        public static string remoteResJson = Application.dataPath + "/json/remoteJson.json";

        public static string abResPath = Application.dataPath + "/ABres/";

        public static string localRes = Application.dataPath + "/OriginalResource/";

        public static FileMd5 localFileMd5;

        public static string GetMD5HashCodeInJson(string file)
        {
            if(localFileMd5 == null)
            {
                localFileMd5 = JsonUtility.FromJson<FileMd5>(remoteResJson);
            }
            foreach(MD5Message mD5Message in localFileMd5.files)
            {
                if(mD5Message.file == file)
                {
                    return mD5Message.md5;
                }
            }
            return null;
        }
    }
}

