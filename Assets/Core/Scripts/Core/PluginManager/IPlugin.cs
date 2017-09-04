using UnityEngine;
using System.Collections;

namespace Framework
{
    public interface IPlugin
    {
        void Init();
        void BackToLauncher();
        bool GetNetworkState(ref int state);
        void Destroy();
        void ApplicationQuit();
        void SetScreenBrightness(int luminance);
        bool GetBrightnessPercent(ref int percent);
        //audio volume: 0 is mute, 10 is full
        void SetCurrentVolume(int level);
        bool GetCurrentVolumePercent(ref int level);

        void StartPersistenConnection();
        void StopPersistenConnection();
    }

}

