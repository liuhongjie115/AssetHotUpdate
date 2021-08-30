using System;
using System.Collections.Generic;

namespace Asset.Scripts.Util
{
    [Serializable]
    public class MD5Message
    {
        public string file;
        public string md5;
        public string fileLength;
    }

    [Serializable]
    public class FileMd5
    {
        public string length = "0";  //³¤¶È
        public List<MD5Message> files = new List<MD5Message>();
    }
}

