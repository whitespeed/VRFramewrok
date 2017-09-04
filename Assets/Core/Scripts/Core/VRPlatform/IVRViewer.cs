using UnityEngine;
using System;

namespace Framework
{
    public interface IVRViewer
    {

        Transform Head { get; }
        void BackToLauncher();
        void ForceQuit();
        void Recent();
        void Destroy();
        int EyeTextureWidth { get; }
        int EyeTextureHeight { get; }

        Camera Right { get; }
        Camera Left { get; }
        VRReticle Reticle { get; }


    }

}



