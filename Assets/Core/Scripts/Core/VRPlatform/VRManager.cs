using UnityEngine;

namespace Framework
{
    public enum VRPlatform
    {
        Cardboard = 1,
        Daydream = 2,
        GearVR = 3,
        Launcher = 4,
        Editor = 5,
    }

    public class VRManager : VRSingleton<VRManager>
    {
        public static IVRController Controller { get; internal set; }
        public static IVRViewer Viewer { get; internal set; }
        public static VRUserControl UserCtrl { get; internal set; }

        protected static string FirstScene = string.Empty;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnAppLoad()
        {
            FirstScene= UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }

        public override void OnInitialize()
        {

#if APP_LUANCHER || APP_LITE_LAUNCHER || APP_MANAGER
            if (Application.isEditor && FirstScene == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                VRApplication.VRPlatform = VRPlatform.Editor;
                Viewer = new EditorVRViewer();
                Controller = new EditorVRController();
            }
            else if(Viewer == null || Controller == null)
            {
                VRApplication.VRPlatform = VRPlatform.Launcher;
                Viewer = new LauncherViewer();
                Controller = new LauncherController();
                Debug.LogWarning("VRPlatform is Launcher!");
            }

#elif Cardboard
            VRApplication.VRPlatform = VRPlatform.Cardboard;
            Viewer = new CardboardViewer();
            Controller = new CardboardController();
            Debug.Log("VRPlatform is Cardboard");
#else
            VRApplication.VRPlatform = VRPlatform.Editor;
            Viewer = new EditorVRViewer();
            Controller = new EditorVRController();
#endif

            GameObject o = gameObject.AddChild(VRRes.Load<GameObject>(R.Prefab.VRUserControl));
            UserCtrl = o.GetComponent<VRUserControl>();
            UserCtrl.SetResetActive(false);
            UserCtrl.SetReturnActive(false);
            UserCtrl.returnAction = () =>
            {
                if (Controller != null) Controller.OnClickBack();
            };
            UserCtrl.resetAction = () =>
            {
                if (Viewer != null) Viewer.Recent();
            };
        }

        public override void OnUninitialize()
        {
            Viewer.Destroy();
            Viewer = null;
            Controller = null;
            UserCtrl = null;
        }

        //private void OnApplicationPause(bool pause)
        //{
        //    if (pause)
        //    {
        //        if (Viewer != null) Viewer.Recent();
        //    }
        //}



        public static void SendToNative<T>(string msg, T par)
        {
            if(IsAppQuitting || Viewer==null)
                return;

        }
    }
}

