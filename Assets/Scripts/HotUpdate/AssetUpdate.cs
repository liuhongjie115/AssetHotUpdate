using Asset.Scripts.Util;
using Assets.Scripts.ResLoader.VO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.HotUpdate
{


    public class AssetUpdate
    {
        public static FileMd5 remoteFileMd5;
        public static List<WebLoaderVO> failLoadVos = new List<WebLoaderVO>();

        public static void StartUpdate()
        {
            failLoadVos.Clear();
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
                List<WebLoaderVO> webLoaderVOs = new List<WebLoaderVO>();
                FileMd5 fileMd5 = JsonUtility.FromJson<FileMd5>(request.downloadHandler.text);
                remoteFileMd5 = fileMd5;
                DeleteOtherBundles(fileMd5);

                List<MD5Message> mD5Messages = fileMd5.files;
                string md5, file, path;
                float length;
                for(int i=0;i<mD5Messages.Count;i++)
                {
                    MD5Message mD5Message = mD5Messages[i];
                    WebLoaderVO vo = new WebLoaderVO(mD5Message);
                    //path = PathUrl(mD5Message.file);
                    //string localMD5 = SystemConfig.GetMD5HashFromFile(path);
                    //if (string.IsNullOrEmpty(localMD5) || localMD5 != mD5Message.md5)  //本地不存在文件或者文件更新
                    if (!vo.CheckMd5Local())  //本地不存在文件或者文件更新
                    {
                        webLoaderVOs.Add(vo);
                        vo.onComplete = SingleResLoaderFinish;
                        length = float.Parse(mD5Message.fileLength);
                        allFilesLength += length;
                    }
                }
                if(webLoaderVOs.Count>0)
                {
                    Debug.Log("开始尝试更新");
                    GameStart.Instance.StartCoroutine(DownLoadBundleFiles(webLoaderVOs,AllResLoaderFinish));
                }
            }
        }

        private static IEnumerator DownLoadBundleFiles(List<WebLoaderVO> webLoaderVOs, Action<bool> CallBack = null)
        {
            int num = 0;
            string dir;
            for(int i=0;i< webLoaderVOs.Count;i++)
            {
                WebLoaderVO webLoaderVO = webLoaderVOs[i];
                //UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(info.Url);
                //yield return request.SendWebRequest();
                var headRequest = UnityWebRequest.Head(webLoaderVO.url);
                yield return headRequest.SendWebRequest();
                long totalLength = long.Parse(headRequest.GetResponseHeader("Content-Length"));   //获得包头长度
                webLoaderVO.TotalSize = totalLength;
                Debug.Log(webLoaderVO.url + "长度：" + totalLength);
                CreateFile(webLoaderVO.tempFileName);
                using (FileStream fs = new FileStream(webLoaderVO.tempFileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    long fileLength = fs.Length;
                    if(fileLength<totalLength)
                    {
                        fs.Seek(fileLength, SeekOrigin.Begin);

                        UnityWebRequest request = UnityWebRequest.Get(webLoaderVO.url);
                        request.SetRequestHeader("Range", "bytes=" + fileLength + "-" + totalLength);  //数据范围设置
                        request.SendWebRequest();

                        int index = 0;
                        while(!request.isDone)
                        {
                            byte[] buff = request.downloadHandler.data;
                            if(buff!=null)
                            {
                                int length = buff.Length - index;
                                Debug.Log("从：" + index + " 写入长度：" + length);
                                fs.Write(buff, index, length);
                                index += length;
                                fileLength += length;
                                webLoaderVO.downFileLgth += buff.Length;
                                Debug.Log("下载总量：" + webLoaderVO.downFileLgth);
                            }
                            yield return null;
                        }
                        num++;
                        fs.Close();
                        fs.Dispose();
                    }
                }
                webLoaderVO.Complete();
            }
            if (CallBack!=null)
            {
                CallBack.Invoke(num == webLoaderVOs.Count);
            }
        }

        private static void DeleteOtherBundles(FileMd5 fileMd5)
        {
            Debug.LogError("---------开始删除----------");
            if (!File.Exists(SystemConfig.JSON_PATH))
            {
                DeleteFolder(SystemConfig.PACK_OUT_RES_PATH);
                Debug.Log("不存在remoteJson.json文件");
                return;
            }
            FileMd5 localMD5File = JsonUtility.FromJson<FileMd5>(File.ReadAllText(SystemConfig.JSON_PATH));
            foreach(MD5Message mD5Message in localMD5File.files)
            {
                bool needDel = true;
                foreach(MD5Message remoteMd5 in fileMd5.files)
                {
                    if(mD5Message.md5==remoteMd5.md5)
                    {
                        needDel = false;
                    }
                }
                if(needDel)
                {
                    string path = SystemConfig.PACK_OUT_RES_PATH + "/" + mD5Message.file;
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        Debug.Log(path + "删除");
                    }
                    else
                    {
                        Debug.Log(path + "不存在文件");
                    }
                }
            }
            Debug.LogError("---------结束删除----------");
        }

        private static bool FindNameInFileMD5(FileMd5 fileMd5,string name)
        {
            foreach(MD5Message mD5Message in fileMd5.files)
            {
                if(mD5Message.file== name)
                {
                    return true;
                }
            }
            return false;
        }

        public static void SingleResLoaderFinish(bool success,WebLoaderVO webLoaderVO)
        {
            if(!success)
            {
                failLoadVos.Add(webLoaderVO);
                Debug.Log("资源：" + webLoaderVO.fileName + "更新错误");
            }
            else
            {
                if(webLoaderVO.DownLoadAfterCheckMd5())
                {
                    Debug.Log("资源：" + webLoaderVO.fileName + "更新完毕");
                    CreateFile(webLoaderVO.fileName);
                    File.WriteAllBytes(webLoaderVO.fileName, File.ReadAllBytes(webLoaderVO.tempFileName));
                    File.Delete(webLoaderVO.tempFileName);
                }
                else
                {
                    failLoadVos.Add(webLoaderVO);
                    Debug.Log("资源：" + webLoaderVO.fileName + "更新后MD5不一致");
                }
            }
        }

        public static void AllResLoaderFinish(bool success)
        {
            if (!success)
            {
                Debug.Log("全部资源更新有误");
            }
            else
            {
                Debug.Log("全部资源更新完毕");
                CreateFile(SystemConfig.JSON_PATH);
                if(remoteFileMd5!=null)
                {
                    for(int i=0;i<failLoadVos.Count;i++)
                    {
                        foreach(MD5Message mD5Message in remoteFileMd5.files)
                        {
                            if(mD5Message.file == failLoadVos[i].file)
                            {
                                remoteFileMd5.files.Remove(mD5Message);
                                break;
                            }
                        }
                    }
                    File.WriteAllText(SystemConfig.JSON_PATH, JsonUtility.ToJson(remoteFileMd5));
                }
            }
        }

        private static void DeleteFolder(string dir)
        {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                {
                    try
                    {
                        FileInfo fi = new FileInfo(d);
                        if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                            fi.Attributes = FileAttributes.Normal;
                        File.Delete(d);//直接删除其中的文件 
                    }
                    catch
                    {

                    }
                }
                else
                {
                    try
                    {
                        DirectoryInfo d1 = new DirectoryInfo(d);
                        if (d1.GetFiles().Length != 0)
                        {
                            DeleteFolder(d1.FullName);////递归删除子文件夹
                        }
                        Directory.Delete(d);
                    }
                    catch
                    {

                    }
                }
            }
        }

        private static void CreateFile(string path)
        {
            string dir = Path.GetDirectoryName(path);
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if(!File.Exists(path))
            {
                File.Create(path).Dispose();
            }
        }
    }
}
