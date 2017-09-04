using UnityEngine;

namespace Framework
{
    public class NetworkManager : VRSingleton<NetworkManager>
    {
        public const string EVENT_STATUS_CHANGED = "on_status_changed";
        public readonly MsgDispather<NetworkReachability> Events = new MsgDispather<NetworkReachability>("NetworkManager");
        public bool AllowCarrierData = false;
        protected NetworkReachability lastState;
        //public NetworkReachability cur = NetworkReachability.ReachableViaLocalAreaNetwork;
        protected NetworkReachability Reachability
        {
            get { return Application.internetReachability; }
        }

        public bool UsingWifi
        {
            get { return Reachability == NetworkReachability.ReachableViaLocalAreaNetwork || VRApplication.IsOffline; }
        }

        public bool NotReachable
        {
            get { return Reachability == NetworkReachability.NotReachable && !VRApplication.IsOffline; }
        }

        public bool UsingCarrierData
        {
            get { return Reachability == NetworkReachability.ReachableViaCarrierDataNetwork; }
        }

        //public NetworkReachability curState = NetworkReachability.ReachableViaCarrierDataNetwork;

        public override void OnInitialize()
        {
            //NetworkTransport.Init();
            lastState = Reachability;
        }

        public override void OnUninitialize()

        {
            //NetworkTransport.Shutdown();
        }


        private void FixedUpdate()
        {
            if (VRApplication.IsOffline)
                return;
            if (Reachability != lastState)
            {
                Events.DispathMsg(EVENT_STATUS_CHANGED, Reachability);
                lastState = Reachability;
            }
            if (Reachability == NetworkReachability.NotReachable && !GlobalUIManager.Instance.IsShown())
            {
                GlobalUIManager.Instance.Show(Localization.Get(R.Lang.NoNetWorkError), null, null, string.Empty,
                    string.Empty, GlobalUIIcon.NoNetwrok);
            }
        }
    }
}