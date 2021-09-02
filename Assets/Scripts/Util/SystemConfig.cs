using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

namespace Asset.Scripts.Util
{
    public class SystemConfig
    {
        public static string packResJson = Application.dataPath + "/json/remoteJson.json";  //AB打包json路径

        public static string abResPath = Application.dataPath + "/ABres/";

        public static string localRes = Application.dataPath + "/OriginalResource/";

        public static string PACK_OUT_RES_PATH = Application.persistentDataPath + "/Res";    //资源真实路径
        public static string PACK_OUT_RES_DOWN_TEMP_PATH = Application.persistentDataPath + "/downTemp";   //资源缓存路径

        public static string PACK_IN_RES_LUA_PATH = Application.dataPath + "/Lua";  //编辑器lua路径
        public static string PACK_OUT_RES_LUA_PATH = Application.persistentDataPath + "/Lua";  //包外lua

        public static string JSON_PATH = Application.persistentDataPath + "/json/remoteJson.json";

        public static string RES_REMOTE_PATH = "http://127.0.0.1/AssetBundles/ABres";   //远端资源

        public static bool isLoadAbRes = true;
        public static bool isLoadAbLua = false;

        public static FileMd5 localFileMd5;

        public static string GetMD5HashCodeInLocal(string file)
        {
            if(!File.Exists(JSON_PATH))
            {
                return null;
            }
            if(localFileMd5 == null)
            {
                localFileMd5 = JsonUtility.FromJson<FileMd5>(File.ReadAllText(JSON_PATH));
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

        /// 获取文件的MD5码  
        /// </summary>  
        /// <param name="fileName">传入的文件名（含路径及后缀名）</param>  
        /// <returns></returns>  
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                DateTime t = DateTime.Now;
                //Debug.Log("开始生成" + fileName + "的MD5 Time：" + t.ToString("yyyMMddhhmmssfff"));
                FileStream file = new FileStream(fileName, System.IO.FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                t = DateTime.Now;
                //Debug.Log("结束生成" + fileName + "的MD5  Time：" + t.ToString("yyyMMddhhmmssfff"));
                return sb.ToString();
            }
            catch (System.Exception ex)
            {
                //Debug.LogError("GetMD5HashFromFile() fail,error:" + ex.Message);
                return null;
            }

        }
    }
}

