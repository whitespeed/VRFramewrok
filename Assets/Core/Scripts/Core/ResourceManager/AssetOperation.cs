using System.Collections;
using UnityEngine;

namespace Framework
{
    public abstract class AssetBundleOperation : IEnumerator
    {
        private bool hasCalledOnFinished;

        protected AssetBundleOperation(string assetBundleName)
        {
            this.assetBundleName = assetBundleName;
        }

        public string assetBundleName { get; private set; }

        public abstract bool IsDone { get; }

        public abstract AssetBundle assetBundle { get; }

        public abstract string error { get; }

        public abstract string url { get; }

        public object Current { get { return null; } }

        public abstract bool MoveNext();

        public void Reset()
        {
        }

        //continue Update() when return true. 
        //ensure only calling OnFinised() once.
        public virtual bool Update()
        {
            if(IsDone && !hasCalledOnFinished)
            {
                AssetBundleManager.ProcessFinishedOperation(this);
                hasCalledOnFinished = true;
                if(!string.IsNullOrEmpty(error))
                {
                    Debug.LogErrorFormat("[AssetBundleOperation] Failed loading AssetBundle:{0} from {1}: {2}",assetBundleName,url,error);
                }
            }
            return !IsDone;
        }
    }

    public class AssetBundleWWWOperation : AssetBundleOperation
    {
        protected WWW www;

        public AssetBundleWWWOperation(string assetBundleName) : base(assetBundleName)
        {
            var url = AssetBundleManager.GetAssetBundleURL(assetBundleName);
            var hash = AssetBundleManager.GetAssetBundleHash(assetBundleName);
            www = WWW.LoadFromCacheOrDownload(url, hash, 0);
        }

        public override bool IsDone { get { return www.isDone; } }

        public override AssetBundle assetBundle
        {
            get
            {
                if(www.isDone && string.IsNullOrEmpty(www.error))
                {
                    return www.assetBundle;
                }
                return null;
            }
        }

        public override string error { get { return www.error; } }
        public override string url { get { return www.url; } }

        public override bool MoveNext()
        {
            return AssetBundleManager.GetLoadedAssetBundle(assetBundleName) == null;
        }

    }

    public class AssetBundleManifestOperation : AssetBundleOperation
    {
        protected WWW www;
        public AssetBundleManifestOperation(string assetBundleName) : base(assetBundleName)
        {
            var bundleUrl = AssetBundleManager.GetAssetBundleURL(assetBundleName);
            www = new WWW(bundleUrl);
        }

        public override bool IsDone { get { return www.isDone; } }
        public override AssetBundle assetBundle { get { return www.assetBundle; } }
        public override string error { get { return www.error; } }
        public override string url { get { return www.url; } }

        public override bool MoveNext()
        {
            return !AssetBundleManager.HasAssetBundleManifest;
        }
    }
}