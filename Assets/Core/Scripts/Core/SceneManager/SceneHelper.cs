using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Framework
{
    public class SceneHelper
    {
        public static float WaitTimeout = 5f;
        protected static WaitUntil NetwrokConnected = new WaitUntil(() => !NetworkManager.Instance.NotReachable);
        protected static WaitUntil NetwrokPermissionConnected = new WaitUntil(() => NetworkManager.Instance.UsingWifi || (NetworkManager.Instance.UsingCarrierData&&NetworkManager.Instance.AllowCarrierData));
        public static IEnumerator RequireNetwork()
        {
            yield return NetwrokConnected;
        }
        public static IEnumerator RequireNetwrokAndAskForCarrierData()
        {
            var waitFun = NetwrokPermissionConnected;
            if (NetworkManager.Instance.UsingCarrierData && !NetworkManager.Instance.AllowCarrierData)
            {
                bool confirmed = false;
                GlobalUIManager.Instance.Show(Localization.Get(R.Lang.NoWifiWarning),
                    () =>
                    {
                        NetworkManager.Instance.AllowCarrierData = true;
                        confirmed = true;
                    },
                    () =>
                    {
                        waitFun = NetwrokConnected;
                        confirmed = true;
                    },
                    Localization.Get(R.Lang.IgnoreAndNotRemind),
                    Localization.Get(R.Lang.Ignore),GlobalUIIcon.NoNetwrok);
                while (!confirmed)
                {
                    yield return 1;
                }
            }
            yield return waitFun;
        }
    }
}
