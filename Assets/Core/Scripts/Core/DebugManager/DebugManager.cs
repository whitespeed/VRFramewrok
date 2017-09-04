using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Object = UnityEngine.Object;

namespace Framework
{

    public class DebugEx
    {
        public static void Log(object tag, object msg)
        {
            Debug.LogFormat("[{0}] {1}",tag.GetType(),msg);
        }
        public static void LogWarning(object tag, object msg)
        {
            Debug.LogWarningFormat("[{0}] {1}", tag.GetType(), msg);
        }
        public static void LogError(object tag, object msg)
        {
            Debug.LogErrorFormat("[{0}] {1}", tag.GetType(), msg);
        }
    }
    public class DebugManager : Singleton<DebugManager>
    {
        public static readonly string[] DisableTags = new string[]
        {
            //"[Singleton]",
            //"[HTTPRequest]",
            //"[HTTPResponse]",
            //"[TimeCodec]",
            //"[TextureManager]",
            //"[LoadingScene]",
            //"[JsonDataContainer]",
        };
        public static bool HasDebugFile()
        {
            var streamAssetPath = Path.Combine(AppPath.StreamingAssetPath, "vr.debug");
            var persistenPath = Path.Combine(AppPath.PersistDataPath, "vr.debug");
            return File.Exists(streamAssetPath) || File.Exists(persistenPath);
        }
        public override void OnInitialize()
        {
            if (HasDebugFile())
            {
                LogWarning("Find 'vr.debug' file and enable debug Mode");
                VRApplication.IsDebug = true;
            }
        }

        public override void OnUninitialize()
        {
        }

        public class VRLogHandler :ILogHandler
        {
            protected static ILogHandler defaultLogHandler;
            protected static VRLogHandler instance = null;

            protected VRLogHandler()
            {
                Debug.logger.logHandler = this;
            }
            public static void CreateHandler()
            {
                defaultLogHandler = Debug.logger.logHandler;
                instance = new VRLogHandler();
            }

            public static void DestroyHandler()
            {
                instance = null;
                Debug.logger.logHandler = defaultLogHandler;
            }

            public void LogFormat(LogType logType, Object context, string format, params object[] args)
            {
                var prefix = string.Format(format, args);
                for (int i = 0; i < DebugManager.DisableTags.Length; i++)
                {
                    if (prefix.StartsWith(DebugManager.DisableTags[i]))
                        return;
                }
                defaultLogHandler.LogFormat(logType,context,format,args);
            }

            public void LogException(Exception exception, Object context)
            {
                defaultLogHandler.LogException(exception, context);
            }
        }
    }
}

