using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

namespace Framework
{
    public class iOSPlugin : IPlugin
    {
        public void Init()
        {
			#if UNITY_IOS
			_Init ();
			#endif
        }

        public void BackToLauncher()
        {
			
        }

        public void Destroy()
        {
			#if UNITY_IOS
			_OnDestroy();
			#endif
        }

        public void StartPersistenConnection()
        {
			#if UNITY_IOS
			_OnStartPersistenConnection();
			#endif
        }

        public void StopPersistenConnection()
        {
			#if UNITY_IOS
			_OnStopPersistenConnection();
			#endif
        }

        public bool GetNetworkState(ref int state)
		{
			return true;
		}

        public void ApplicationQuit()
        {
        }

        public void SetScreenBrightness(int luminance)
        {
#if UNITY_IOS
			float val = luminance / 20f + 0.5f;
			_OnSetBrightness(val);
#endif
        }

        public bool GetBrightnessPercent(ref int lumain)
        {
#if UNITY_IOS
            float value = 0f;
            var success = _OnGetBrightness(ref value);
			lumain = Mathf.CeilToInt((value - 0.5f)  * 20f);
            return success;
#endif
            return false;
        }

        public void SetCurrentVolume(int percent)
        {
#if UNITY_IOS
			float val = percent / 10f;
			_OnSetVolume(val);
#endif
        }

        public bool GetCurrentVolumePercent(ref int percent)
        {
#if UNITY_IOS
            float value = 0f;
            var success = _OnGetVolume(ref value);
            percent = Mathf.CeilToInt(value * 10f);
            return success;
#endif
            return false;
        }

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void _Init ();

		[DllImport("__Internal")]
		private static extern void _OnDestroy ();

		[DllImport("__Internal")]
		private static extern void _OnStartPersistenConnection ();

		[DllImport("__Internal")]
		private static extern void _OnStopPersistenConnection ();

        [DllImport("__Internal")]
        private static extern bool _OnGetVolume(ref float Volume);

        [DllImport("__Internal")]
        private static extern bool _OnGetBrightness(ref float brightness);

        [DllImport("__Internal")]
        private static extern void _OnSetVolume(float Volume);

        [DllImport("__Internal")]
        private static extern void _OnSetBrightness(float brightness);
#endif
    }

}

