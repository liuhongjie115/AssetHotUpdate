using Asset.Scripts.Util;
using Assets.Scripts.Core;
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
        private static Action<bool> _updateComplete;

        static long allFilesLength = 0;

        public static void StartUpdate(Action<bool> updateComplete =null)
        {
            if(SystemConfig.isLoadAbRes)
            {
                _updateComplete = updateComplete;
                failLoadVos.Clear();
                GameStart.Instance.StartCoroutine(VersionUpdate());
            }
            else
            {
                updateComplete?.Invoke(true);
            }
        }

        private static IEnumerator VersionUpdate()
        {
            //string uri = @"http://localhost/Json/remoteJson.json";
            string uri = @"http://assetbundlelhj.oss-accelerate.aliyuncs.com/Json/remoteJson.json";
            UnityWebRequest request = UnityWebRequest.Get(uri);
            yield return request.SendWebRequest();
            allFilesLength = 0;
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
                    //if (string.IsNullOrEmpty(localMD5) || localMD5 != mD5Message.md5)  //���ز������ļ������ļ�����
                    if (!vo.CheckMd5Local())  //���ز������ļ������ļ�����
                    {
                        webLoaderVOs.Add(vo);
                        vo.onComplete = SingleResLoaderFinish;
                        length = float.Parse(mD5Message.fileLength);
                        allFilesLength += (long)length;
                    }
                }
                if(webLoaderVOs.Count>0)
                {
                    Debug.Log("��ʼ���Ը���");
                    GameStart.Instance.StartCoroutine(DownLoadBundleFiles(webLoaderVOs,AllResLoaderFinish,(long down)=> { LoadPanel.Instance.SetDown(allFilesLength, down); }));
                }
                else
                {
                    //_updateComplete?.Invoke(true);
                    AllResLoaderFinish(true);
                }
            }
        }

        private static IEnumerator DownLoadBundleFiles(List<WebLoaderVO> webLoaderVOs, Action<bool> CallBack = null, Action<long> LoopCallBack = null)
        {
            int num = 0;
            string dir;
            long downProcess = 0;
            for(int i=0;i< webLoaderVOs.Count;i++)
            {
                WebLoaderVO webLoaderVO = webLoaderVOs[i];
                //UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(info.Url);
                //yield return request.SendWebRequest();
                var headRequest = UnityWebRequest.Head(webLoaderVO.url);
                yield return headRequest.SendWebRequest();
                long totalLength = long.Parse(headRequest.GetResponseHeader("Content-Length"));   //��ð�ͷ����
                Debug.Log(webLoaderVO.url + "���ȣ�" + totalLength);
                CreateFile(webLoaderVO.tempFileName);
                using (FileStream fs = new FileStream(webLoaderVO.tempFileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    long fileLength = fs.Length;
                    downProcess += fileLength;
                    if (fileLength<totalLength)
                    {
                        fs.Seek(fileLength, SeekOrigin.Begin);

                        UnityWebRequest request = UnityWebRequest.Get(webLoaderVO.url);
                        request.SetRequestHeader("Range", "bytes=" + fileLength + "-" + totalLength);  //���ݷ�Χ����
                        yield return request.SendWebRequest();

                        int index = 0;
                        if(request.isDone)
                        {
                            if(request.downloadHandler.data != null)
                            {
                                webLoaderVO.TotalSize = request.downloadHandler.data.Length;
                                int length = request.downloadHandler.data.Length - index;
                                downProcess += length;
                                Debug.Log("�ӣ�" + index + " д�볤�ȣ�" + length);
                                fs.Write(request.downloadHandler.data, index, length);
                                index += length;
                                fileLength += length;
                                webLoaderVO.downFileLgth += request.downloadHandler.data.Length;
                                Debug.Log("����������" + webLoaderVO.downFileLgth);
                                LoopCallBack?.Invoke(downProcess/1024);
                                yield return null;
                            }
                        }
                        fs.Close();
                        fs.Dispose();
                    }
                    LoopCallBack?.Invoke(downProcess / 1024);
                    num++;
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
            ALog.Info("---------��ʼɾ��----------");
            if (!File.Exists(SystemConfig.JSON_PATH))
            {
                DeleteFolder(SystemConfig.PACK_OUT_RES_PATH);
                Debug.Log("������remoteJson.json�ļ�");
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
                        Debug.Log(path + "ɾ��");
                    }
                    else
                    {
                        Debug.Log(path + "�������ļ�");
                    }
                    path = SystemConfig.PACK_OUT_RES_DOWN_TEMP_PATH + "/" + mD5Message.file;
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        Debug.Log(path + "ɾ��");
                    }
                    else
                    {
                        Debug.Log(path + "�������ļ�");
                    }
                }
            }
            ALog.Info("---------����ɾ��----------");
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
                Debug.Log("��Դ��" + webLoaderVO.fileName + "���´���");
            }
            else
            {
                if(webLoaderVO.DownLoadAfterCheckMd5())
                {
                    Debug.Log("��Դ��" + webLoaderVO.fileName + "�������");
                    CreateFile(webLoaderVO.fileName);
                    File.WriteAllBytes(webLoaderVO.fileName, File.ReadAllBytes(webLoaderVO.tempFileName));
                }
                else
                {
                    failLoadVos.Add(webLoaderVO);
                    Debug.Log("��Դ��" + webLoaderVO.fileName + "���º�MD5��һ��");
                }
            }
        }

        public static void AllResLoaderFinish(bool success)
        {
            if (failLoadVos.Count > 0)
            {
                for (int i = 0; i < failLoadVos.Count; i++)
                {
                    File.Delete(failLoadVos[i].tempFileName);
                    Debug.Log("ȫ����Դ��������");
                }
            }
            else if (!success)
            {
                Debug.Log("ȫ����Դ��������");
            }
            else
            {
                Debug.Log("ȫ����Դ�������");
                CreateFile(SystemConfig.JSON_PATH);
                if (remoteFileMd5 != null)
                {
                    for (int i = 0; i < failLoadVos.Count; i++)
                    {
                        foreach (MD5Message mD5Message in remoteFileMd5.files)
                        {
                            if (mD5Message.file == failLoadVos[i].file)
                            {
                                remoteFileMd5.files.Remove(mD5Message);
                                break;
                            }
                        }
                    }
                    File.WriteAllText(SystemConfig.JSON_PATH, JsonUtility.ToJson(remoteFileMd5));
                }
                DeleteFolder(SystemConfig.PACK_OUT_RES_DOWN_TEMP_PATH);
            }
            _updateComplete?.Invoke(success);
        }

        public static void DeleteFolder(string dir)
        {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                string p = d;
                p = p.Replace('\\', '/');
                if (File.Exists(p))
                {
                    try
                    {
                        FileInfo fi = new FileInfo(d);
                        if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                            fi.Attributes = FileAttributes.Normal;
                        File.Delete(p);//ֱ��ɾ�����е��ļ� 
                    }
                    catch
                    {

                    }
                }
                else
                {
                    try
                    {
                        string[] content = Directory.GetFiles(p, "*", SearchOption.AllDirectories);
                        if (content.Length != 0)
                        {
                            DeleteFolder(p);////�ݹ�ɾ�����ļ���
                        }
                        Directory.Delete(p);
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
