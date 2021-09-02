using Assets.Scripts.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Core
{
    public class ALogType
    {
        public const int Info = 1;
        public const int Warning = 2;
        public const int Error = 4;
        public const int LuaInfo = 8;
        public const int LuaWarning = 16;
        public const int LuaError = 32;
        public const int Socket = 64;
        public const int FMD = 128;
        public const int CSQ = 256;
        public const int CYG = 512;
        public const int LHJ = 1024;
        public const int HJK = 2048;
        public const int QL = 4096;

        private static Dictionary<int, string> nameDict = new Dictionary<int, string>() {
            {Info , "Info"},{Warning , "Warning"},{Error , "Error"},
            {LuaInfo , "LuaInfo"},{LuaWarning , "LuaWarning"},{LuaError , "LuaError"},{Socket , "Socket"},
            {FMD , "FMD"},{CSQ , "CSQ"},{CYG , "CYG"},{LHJ , "LHJ"},{HJK , "HJK"},{QL , "QL"},
        };

        public static string Type2Name(int logType)
        {
            return nameDict[logType];
        }

        public static void AddLogType(int logType, string name)
        {
            nameDict[logType] = name;
        }
    }

    public class ALog
    {


        private static ALogFile aLogFile = new ALogFile();
        public static int curLogType;

        private static Func<string> getLuaStackInfo;
        private static bool isCanSendBugly = false;
        public static Action<string, bool, int> dispatchToGMPanelCB;
        public static void Init()
        {
            aLogFile.Init();
            SetCurLogType(ALogType.Info | ALogType.Error | ALogType.Warning | ALogType.LuaError);
            InitLogMsgReceived();
        }

        public static void SetCurLogType(int logType)
        {
            curLogType = logType;
        }

        public static void AddCurLogType(int logType)
        {
            curLogType = curLogType | logType;
        }

        public static void SetLuaStackInfoCB(Func<string> luaStackInfoCB)
        {
            getLuaStackInfo = luaStackInfoCB;
        }

        public static void CanSendBugly()
        {
            isCanSendBugly = true;
        }

        public static string GetLogRootPath()
        {
            return aLogFile.logRootPath;
        }

        public static void OnQuit()
        {
            aLogFile.ClearOutOfDateLog();
            aLogFile.Dispose();
        }

        public static void Info(string msg, bool isWriteStack = false)
        {
            if (ALogType.Info == (curLogType & ALogType.Info))
            {
                LogSingle(msg, isWriteStack, ALogType.Info);
            }
        }

        public static void Warning(string msg, bool isWriteStack = false)
        {
            if (ALogType.Warning == (curLogType & ALogType.Warning))
            {
                LogSingle(msg, isWriteStack, ALogType.Warning);
            }
        }

        public static void Error(string msg, bool isWriteStack = false)
        {
            if (ALogType.Error == (curLogType & ALogType.Error))
            {
                LogSingle(msg, isWriteStack, ALogType.Error);
            }
        }

        public static void LuaInfo(string msg, bool isWriteStack = false)
        {
            if (ALogType.LuaInfo == (curLogType & ALogType.LuaInfo))
            {
                LogSingle(msg, isWriteStack, ALogType.LuaInfo);
            }
        }

        public static void LuaWarning(string msg, bool isWriteStack = false)
        {
            if (ALogType.LuaWarning == (curLogType & ALogType.LuaWarning))
            {
                LogSingle(msg, isWriteStack, ALogType.LuaWarning);
            }
        }

        public static void LuaError(string msg, bool isWriteStack = false)
        {
            if (ALogType.LuaError == (curLogType & ALogType.LuaError))
            {
                LogSingle(msg, isWriteStack, ALogType.LuaError);
            }
        }

        public static void Other(int logType, string msg, bool isWriteStack = false)
        {
            if (logType == (curLogType & logType))
            {
                LogSingle(msg, isWriteStack, logType);
            }
        }

        private static void LogSingle(string msg, bool isWriteStack, int logType)
        {
            var srcMsg = string.Format("[{0}] {1}£º{2}", ALogType.Type2Name(logType), DateUtils.GetNowYMDFormatALog(), msg);
            var fullMsg = srcMsg;

            if (isWriteStack)
            {
                if (getLuaStackInfo != null && (logType == ALogType.LuaInfo || logType == ALogType.LuaWarning ||
                    logType == ALogType.LuaError))
                {
                    fullMsg += "\n\n" + getLuaStackInfo.Invoke();
                }
                else
                {
                    fullMsg += "\n\n" + GetStackInfo();
                }
            }

            if (dispatchToGMPanelCB != null && (logType == ALogType.Error || logType == ALogType.LuaError))
            {
                dispatchToGMPanelCB.Invoke(fullMsg, isWriteStack, logType);
            }

            if (Application.isEditor)
            {
                PrintLogToConsole(logType, fullMsg);
            }
            aLogFile.WriteLog(fullMsg);
        }

        private static void PrintLogToConsole(int logType, string msg)
        {
            switch (logType)
            {
                case ALogType.LuaInfo:
                case ALogType.Info:
                    Debug.Log(msg);
                    break;
                case ALogType.LuaWarning:
                case ALogType.Warning:
                    Debug.LogWarning(msg);
                    break;
                case ALogType.LuaError:
                case ALogType.Error:
                    Debug.LogError(msg);
                    break;
                default:
                    Debug.Log(msg);
                    break;
            }
        }

        private static string GetStackInfo()
        {
            try
            {
                StackTrace stackTrace = new StackTrace(true);
                StringBuilder sb = new StringBuilder();
                for (int i = 3; i < stackTrace.FrameCount; i++)
                {
                    StackFrame sf = stackTrace.GetFrame(i);
                    MethodBase methodaaa = sf.GetMethod();
                    sb.AppendLine(string.Format(" {0}.{1}()    (at {2} : {3})",
                        methodaaa.ReflectedType != null ? methodaaa.ReflectedType.Name : "???",
                        methodaaa.Name,
                        sf.GetFileName(),
                        sf.GetFileLineNumber()));
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                return "stack error: \n" + e.Message;
            }
        }

        private static void InitLogMsgReceived()
        {
            Application.logMessageReceived += LogMsgReceivedCB;
        }

        private static void LogMsgReceivedCB(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                if (
                    condition.IndexOf("[Error]") == -1 &&
                    condition.IndexOf("[LuaError]") == -1 &&
                    condition.IndexOf("[Warning]") == -1 &&
                    condition.IndexOf("[LuaWarning]") == -1)
                {
                    Error(condition + "\n\n" + stackTrace, true);
                }
            }
        }

        public static void ReStart()
        {
            getLuaStackInfo = null;
            dispatchToGMPanelCB = null;
        }

        private class ALogFile
        {
            public string logRootPath = Application.persistentDataPath + "/log/";
            private string logPath = "";
            private int keepLogCount = 7;
            private FileStream fileStream;
            private StreamWriter streamWriter;
            private object lockObject = new object();

            public void Init()
            {
                logPath = Path.Combine(logRootPath, "l_" + DateUtils.GetNowYMDFormatALogFileName() + ".txt");
                if (Directory.Exists(logRootPath) == false)
                {
                    Directory.CreateDirectory(logRootPath);
                }
                fileStream = new FileStream(logPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                fileStream.Seek(0, SeekOrigin.End);
                streamWriter = new StreamWriter(fileStream);
            }

            public void WriteLog(string msg)
            {
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    SaveWriteLog(msg);
                }
                else
                {
                    ((Action<string>)SaveWriteLog).BeginInvoke(msg, null, null);
                }
            }

            private void SaveWriteLog(string msg)
            {
                lock (lockObject)
                {
                    streamWriter.WriteLine(msg);
                    streamWriter.Flush();
                }
            }

            public void ClearOutOfDateLog()
            {
                if (Directory.Exists(logPath))
                {
                    var files = Directory.GetFiles(logPath);
                    for (int i = 0; i < Math.Min(files.Length, keepLogCount); i++)
                    {
                        File.Delete(files[i]);
                    }
                }
            }

            public void Dispose()
            {
                fileStream.Flush();
                streamWriter.Flush();

                streamWriter.Close();
                fileStream.Close();
                fileStream.Dispose();
                fileStream = null;
                streamWriter = null;
            }
        }
    }
}