using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Framework
{

    public class PluginManager : VRSingleton<PluginManager>
    {
        public class Msg
        {
            public const string NativeMessage = "_Native Massage";
        }

        public static readonly MsgDispather<string> Dispather = new MsgDispather<string>("PluginManager");

        protected static IPlugin pluginHandler = null;
        public static IPlugin Plugin
        {
            get { return pluginHandler; }
        }
        protected IPlugin CreateHandler()
        {
#if UNITY_EDITOR
            return new EditorPlugin();
#elif UNITY_ANDROID
        return new AndroidPlugin();
#elif UNITY_IPHONE
        return new iOSPlugin();
#else
        return new EditorPlugin();
#endif
        }

        public override void OnInitialize()
        {
            pluginHandler = CreateHandler();
            pluginHandler.Init();
        }

        public override void OnUninitialize()
        {
            if (pluginHandler != null)
            {
                pluginHandler.Destroy();
                pluginHandler = null;
            }
        }

        protected StringSplitter splitter = new StringSplitter(10);
        readonly char[] spliter = new char[] { '&' };
        public void ReceiveNativeMessage(string str)
        {
            //Debug.Log("Receive from plugin " + str);
            string[] msg = str.Split(spliter);
            //Debug.Log(msg[0]+"   "+ msg[1]);
            Dispather.DispathMsg(msg[0], msg[1]);
        }
    }

}

