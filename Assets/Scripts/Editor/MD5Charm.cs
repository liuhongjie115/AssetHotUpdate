using Asset.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace Asset.Scripts.Util
{
    public class MD5Charm
    {
        private static Dictionary<string, string> m_BundleMD5Map = new Dictionary<string, string>();

        static string GetRemoteName()
        {
            return "E:/MyServer/Chief";
        }

        [MenuItem("MD5校验器/开始")]
        static void BuildReleaseBundle()
        {
            BuildBundleStart();
        }

        static void BuildBundleStart()
        {
            Caching.ClearCache();
            DeleteTempBundles();   //删除旧版MD5
            CreateBundleVersinNumber();
            AssetDatabase.Refresh();
        }


        /// <summary>
        /// 删除本地远程文件
        /// </summary>
        /// <param name="path"></param>
        static void DeleteTempBundles()
        {
            string directStr = SystemConfig.remoteResJson.Substring(0,SystemConfig.remoteResJson.LastIndexOf('/'));
            Debug.Log(directStr);
            if (!Directory.Exists(SystemConfig.remoteResJson.Substring(0,SystemConfig.remoteResJson.LastIndexOf('/'))))
            {
                Directory.CreateDirectory(SystemConfig.remoteResJson.Substring(0,SystemConfig.remoteResJson.LastIndexOf('/')));
            }
            if (!File.Exists(SystemConfig.remoteResJson))
            {
                File.WriteAllText(SystemConfig.remoteResJson,string.Empty);
            }
            File.Delete(SystemConfig.remoteResJson);
            //string[] bundleFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            //foreach (string s in bundleFiles)
            //{
            //    if (s == "Bundle.json")
            //    {
            //        File.Delete(s);
            //    }
        }

        static void CreateBundleVersinNumber()
        {
            if (!File.Exists(SystemConfig.remoteResJson))
            {
                File.WriteAllText(SystemConfig.remoteResJson, string.Empty);
            }
            FileMd5 _file = new FileMd5();
            string[] contents = Directory.GetFiles(SystemConfig.abResPath, "*.*", SearchOption.AllDirectories);
            string extension = "";
            string fileName = "";
            string fileMD5 = "";
            long allLength = 0;
            int fileLen = 0;
            m_BundleMD5Map.Clear();
            for(int i=0;i<contents.Length;i++)
            {
                if(!contents[i].EndsWith("meta"))
                {
                    fileName = contents[i].Replace(SystemConfig.abResPath, "").Replace("\\", "/");
                    MD5Message mD5Message = new MD5Message();
                    mD5Message.file = fileName;
                    mD5Message.md5 = GetMD5HashFromFile(contents[i]);
                    FileInfo fileInfo = new FileInfo(contents[i]);
                    mD5Message.fileLength = (fileInfo.Length / 1024f).ToString("0.0");
                    _file.files.Add(mD5Message);
                    _file.length = (float.Parse(_file.length) + float.Parse(mD5Message.fileLength)).ToString("0.0");
                }
            }
            File.WriteAllText(SystemConfig.remoteResJson, JsonUtility.ToJson(_file));
        }

        /// 获取文件的MD5码  
        /// </summary>  
        /// <param name="fileName">传入的文件名（含路径及后缀名）</param>  
        /// <returns></returns>  
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, System.IO.FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
    }
}

