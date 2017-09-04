using UnityEngine;
using System.Collections;

namespace Framework
{
    public class PoolsManager : VRSingleton<PoolsManager>
    {

        public const string GlobalPoolName = "Global";
        public static readonly SpawnPoolsDict Pools = new SpawnPoolsDict();


        public override void OnInitialize()
        {
            globalPool = this.gameObject.AddComponent<SpawnPool>();
            globalPool.poolName = GlobalPoolName;
            Pools.Add(globalPool);
        }

        public override void OnUninitialize()
        {
            globalPool = null;
        }

        protected static SpawnPool globalPool;
        public static SpawnPool Global
        {
            get
            {
                return globalPool;
            }
        }
    }

}

