using Asset.Scripts.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.HotUpdate
{
    public struct BundleInfo
    {
        public string Path { get; set; }
        public string Url { get; set; }  //远程下载地址
        public override bool Equals(object obj)
        {
            return obj is BundleInfo && Url == ((BundleInfo)obj).Url;
        }
        public override int GetHashCode()
        {
            return Url.GetHashCode();
        }
    }

    public class AssetUpdate
    {
        public static void StartUpdate()
        {
            GameStart.Instance.StartCoroutine(VersionUpdate());
        }

        private static IEnumerator VersionUpdate()
        {
            string uri = @"http://localhost/Json/remoteJson.json";
            UnityWebRequest request = UnityWebRequest.Get(uri);
            yield return request.SendWebRequest();
            float allFilesLength = 0;
            if(request.isDone && string.IsNullOrEmpty(request.error))
            {
                List<BundleInfo> bims = new List<BundleInfo>();
                FileMd5 fileMd5 = JsonUtility.FromJson<FileMd5>(request.downloadHandler.text);
                DeleteOtherBundles(fileMd5);

                List<MD5Message> mD5Messages = fileMd5.files;
                string md5, file, path;
                float length;
                for(int i=0;i<mD5Messages.Count;i++)
                {
                    MD5Message mD5Message = mD5Messages[i];
                    string localMD5 = GetMD5HashFromFile(mD5Message.file);
                    if(string.IsNullOrEmpty(localMD5)||localMD5!=mD5Message.md5)  //本地不存在文件或者文件更新
                    {
                        bims.Add(new BundleInfo()
                        {
                            Url = HttpDownLoadUrl(mD5Message.file),
                            Path = PathUrl(mD5Message.file)
                        });
                        length = float.Parse(mD5Message.fileLength);
                        allFilesLength += length;
                    }
                }
                if(bims.Count>0)
                {
                    Debug.Log("开始尝试更新");
                    GameStart.Instance.StartCoroutine();
                }
            }
        }

        private static IEnumerator DownLoadBundleFiles(List<BundleInfo> infos, Action<float> LoopCallBack = null, Action<bool> CallBack = null)
        {
            int num = 0;
            string dir;
            for(int i=0;i<infos.Count;i++)
            {
                BundleInfo info = infos[i];
                UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(info.Url);
                yield return request.SendWebRequest();
                if(request.isDone&&string.IsNullOrEmpty(request.error))
                {
                    try
                    {
                        string filePath = info.Path;
                        dir = Path.GetDirectoryName(filePath);
                        if(!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.Log("下载失败" + e.Message);
                    }
                }
                else
                {
                    Debug.Log("下载错误" + request.error);
                }
            }
            if(CallBack!=null)
            {
                CallBack.Invoke(num == infos.Count);
            }
        }

        private static void DeleteOtherBundles(FileMd5 fileMd5)
        {
            Debug.LogError("---------开始删除----------");
            string[] bundleFiles = Directory.GetFiles(GetTerracePath(), "*.*", SearchOption.AllDirectories);
            foreach(string idx in bundleFiles)
            {
                if(!FindNameInFileMD5(fileMd5,idx))
                {
                    File.Delete(idx);
                    Debug.LogError(idx + "不存在");
                }
            }
            Debug.LogError("---------结束删除----------");
        }

        private static bool FindNameInFileMD5(FileMd5 fileMd5,string name)
        {
            foreach(MD5Message mD5Message in fileMd5.files)
            {
                if(mD5Message.file==name)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 远程下载地址
        /// </summary>
        /// <param name="_str"></param>
        /// <returns></returns>
        private static string HttpDownLoadUrl(string _str)
        {
            return "http://127.0.0.1/AssetBundles/ABres/" + _str;
        }

        /// <summary>
        /// 单一本地AB资源路径
        /// </summary>
        /// <param name="_str"></param>
        /// <returns></returns>
        private static string PathUrl(string _str)
        {
            return GetTerracePath() + "/" + _str;
        }

        private static string GetTerracePath()
        {
            return Application.persistentDataPath + "/Res/";
        }

        private static string GetMD5HashFromFile(string file)
        {
            return SystemConfig.GetMD5HashCodeInJson(file);
        }
    }
}
