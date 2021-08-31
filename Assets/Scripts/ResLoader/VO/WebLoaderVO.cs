using Asset.Scripts.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ResLoader.VO
{
    public class WebLoaderVO
    {
        public string file = "";
        public string fileName = "";
        public string tempFileName = "";
        public string url = "";
        public long downFileLgth;
        private long _totalSize = 0;
        private double _totalSizeKb = 0;
        private double _totalSizeMb = 0;
        public string md5 = "";

        public Action<string, string> onProgress { get; set; }   //下载完/总的
        public Action<bool, WebLoaderVO> onComplete;

        public WebLoaderVO(MD5Message mD5Message)
        {
            this.file = mD5Message.file;
            this.fileName = (SystemConfig.PACK_OUT_RES_PATH + "/" + file).Replace('\\','/');
            this.tempFileName = (SystemConfig.PACK_OUT_RES_DOWN_TEMP_PATH + "/" + file).Replace('\\', '/');
            this.url = (SystemConfig.RES_REMOTE_PATH + "/" + file).Replace('\\', '/');
            this.md5 = mD5Message.md5;
        }

        public long TotalSize
        {
            get
            {
                return _totalSize;
            }
            set
            {
                _totalSize = value;
                _totalSizeKb = Math.Ceiling((double)_totalSize / 1024);
                if (_totalSizeKb > 1024)
                {
                    _totalSizeMb = Math.Ceiling((double)_totalSizeKb / 1024);
                }
            }
        }

        public override bool Equals(object obj)
        {
            return obj is WebLoaderVO && url == ((WebLoaderVO)obj).url;
        }

        public override int GetHashCode()
        {
            return url.GetHashCode();
        }

        public bool DownLoadAfterCheckMd5()
        {
            if (!String.IsNullOrEmpty(md5) && md5.Length == 32)
            {
                string downMd5 = SystemConfig.GetMD5HashFromFile(tempFileName);
                return string.Equals(md5, downMd5);
            }
            return false;
        }

        public bool CheckMd5Local()
        {
            if (!String.IsNullOrEmpty(md5) && md5.Length == 32)
            {
                string localMd5 = SystemConfig.GetMD5HashCodeInLocal(file);
                return string.Equals(md5, localMd5);
            }
            return false;
        }

        public void Complete()
        {
            onComplete(this.downFileLgth == this.TotalSize, this);
        }
    }
}

