
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Framework
{
    public static class InstanceHandler
    {
        public delegate GameObject InstantiateDelegate(GameObject prefab, Vector3 pos, Quaternion rot);
        public delegate void DestroyDelegate(GameObject instance);

        /// <summary>
        /// Creates a new instance. 
        /// 
        /// If at least one delegate is added to InstanceHandler.InstantiateDelegates it will be used instead of 
        /// Unity's Instantiate.
        /// </summary>
        public static InstantiateDelegate InstantiateDelegates;

        /// <summary>
        /// Destroys an instance. 
        /// 
        /// If at least one delegate is added to InstanceHandler.DestroyDelegates it will be used instead of 
        /// Unity's Instantiate.
        /// </summary>
        public static DestroyDelegate DestroyDelegates;

        /// <summary>
        /// See the DestroyDelegates docs
        /// </summary>
        /// <param name="prefab">The prefab to spawn an instance from</param>
        /// <param name="pos">The position to spawn the instance</param>
        /// <param name="rot">The rotation of the new instance</param>
        /// <returns>Transform</returns>
        internal static GameObject InstantiatePrefab(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            if (InstanceHandler.InstantiateDelegates != null)
            {
                return InstanceHandler.InstantiateDelegates(prefab, pos, rot);
            }
            else
            {
                return Object.Instantiate(prefab, pos, rot) as GameObject;
            }
        }


        /// <summary>
        /// See the InstantiateDelegates docs
        /// </summary>
        /// <param name="prefab">The prefab to spawn an instance from</param>
        /// <returns>void</returns>
        internal static void DestroyInstance(GameObject instance)
        {
            if (InstanceHandler.DestroyDelegates != null)
            {
                InstanceHandler.DestroyDelegates(instance);
            }
            else
            {
                Object.Destroy(instance);
            }
        }
    }


    public class SpawnPoolsDict
    {
        #region Event Handling
        public delegate void OnCreatedDelegate(SpawnPool pool);

        internal Dictionary<string, OnCreatedDelegate> onCreatedDelegates =
             new Dictionary<string, OnCreatedDelegate>();

        public void AddOnCreatedDelegate(string poolName, OnCreatedDelegate createdDelegate)
        {
            // Assign first delegate "just in time"
            if (!this.onCreatedDelegates.ContainsKey(poolName))
            {
                this.onCreatedDelegates.Add(poolName, createdDelegate);
                return;
            }

            this.onCreatedDelegates[poolName] += createdDelegate;
        }

        public void RemoveOnCreatedDelegate(string poolName, OnCreatedDelegate createdDelegate)
        {
            if (!this.onCreatedDelegates.ContainsKey(poolName))
                throw new KeyNotFoundException
                (
                    "No OnCreatedDelegates found for pool name '" + poolName + "'."
                );

            this.onCreatedDelegates[poolName] -= createdDelegate;
        }

        #endregion Event Handling

        #region Public Custom Memebers
        /// <summary>
        /// Creates a new GameObject with a SpawnPool Component which registers itself
        /// with the PoolManager.Pools dictionary. The SpawnPool can then be accessed 
        /// directly via the return value of this function or by via the PoolManager.Pools 
        /// dictionary using a 'key' (string : the name of the pool, SpawnPool.poolName).
        /// </summary>
        /// <param name="poolName">
        /// The name for the new SpawnPool. The GameObject will have the word "Pool"
        /// Added at the end.
        /// </param>
        /// <returns>A reference to the new SpawnPool component</returns>
        public SpawnPool Create(string poolName)
        {
            // Add "Pool" to the end of the poolName to make a more user-friendly
            //   GameObject name. This gets stripped back out in SpawnPool Awake()
            var owner = new GameObject(poolName);
            return owner.AddComponent<SpawnPool>();
        }


        /// <summary>
        ///Creates a SpawnPool Component on an 'owner' GameObject which registers 
        /// itself with the PoolManager.Pools dictionary. The SpawnPool can then be 
        /// accessed directly via the return value of this function or via the
        /// PoolManager.Pools dictionary.
        /// </summary>
        /// <param name="poolName">
        /// The name for the new SpawnPool. The GameObject will have the word "Pool"
        /// Added at the end.
        /// </param>
        /// <param name="owner">A GameObject to add the SpawnPool Component</param>
        /// <returns>A reference to the new SpawnPool component</returns>
        public SpawnPool Create(string poolName, GameObject owner)
        {
            if (!this.assertValidPoolName(poolName))
                return null;

            // When the SpawnPool is created below, there is no way to set the poolName
            //   before awake runs. The SpawnPool will use the gameObject name by default
            //   so a try statement is used to temporarily change the parent's name in a
            //   safe way. The finally block will always run, even if there is an error.
            string ownerName = owner.gameObject.name;

            try
            {
                owner.gameObject.name = poolName;

                // Note: This will use SpawnPool.Awake() to finish init and self-add the pool
                return owner.AddComponent<SpawnPool>();
            }
            finally
            {
                // Runs no matter what
                owner.gameObject.name = ownerName;
            }
        }


        /// <summary>
        /// Used to ensure a name is valid before creating anything.
        /// </summary>
        /// <param name="poolName">The name to test</param>
        /// <returns>True if sucessful, false if failed.</returns>
        private bool assertValidPoolName(string poolName)
        {
            // Cannot request a name with the word "Pool" in it. This would be a 
            //   rundundant naming convention and is a reserved word for GameObject
            //   defaul naming
            string tmpPoolName;
            tmpPoolName = poolName.Replace("Pool", "");
            if (tmpPoolName != poolName)  // Warn if "Pool" was used in poolName
            {
                // Log a warning and continue on with the fixed name
                string msg = string.Format("'{0}' has the word 'Pool' in it. " +
                       "This word is reserved for GameObject defaul naming. " +
                       "The pool name has been changed to '{1}'",
                       poolName, tmpPoolName);

                Debug.LogWarning(msg);
                poolName = tmpPoolName;
            }

            if (this.ContainsKey(poolName))
            {
                Debug.Log(string.Format("A pool with the name '{0}' already exists",
                                        poolName));
                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns a formatted string showing all the pool names
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // Get a string[] array of the keys for formatting with join()
            var keysArray = new string[this._pools.Count];
            this._pools.Keys.CopyTo(keysArray, 0);

            // Return a comma-sperated list inside square brackets (Pythonesque)
            return string.Format("[{0}]", System.String.Join(", ", keysArray));
        }



        /// <summary>
        /// Destroy an entire SpawnPool, including its GameObject and all children.
        /// You can also just destroy the GameObject directly to achieve the same result.
        /// This is really only here to make it easier when a reference isn't at hand.
        /// </summary>
        /// <param name="spawnPool"></param>
        public bool Destroy(string poolName)
        {
            // Use TryGetValue to avoid KeyNotFoundException.
            //   This is faster than Contains() and then accessing the dictionary
            SpawnPool spawnPool;
            if (!this._pools.TryGetValue(poolName, out spawnPool))
            {
                Debug.LogError(
                    string.Format("PoolManager: Unable to destroy '{0}'. Not in PoolManager",
                                  poolName));
                return false;
            }

            // The rest of the logic will be handled by OnDestroy() in SpawnPool
            UnityEngine.Object.Destroy(spawnPool.gameObject);

            // Remove it from the dict in case the user re-creates a SpawnPool of the 
            //  same name later
            //this._pools.Remove(spawnPool.poolName);

            return true;
        }

        /// <summary>
        /// Destroy ALL SpawnPools, including their GameObjects and all children.
        /// You can also just destroy the GameObjects directly to achieve the same result.
        /// This is really only here to make it easier when a reference isn't at hand.
        /// </summary>
        /// <param name="spawnPool"></param>
        public void DestroyAll()
        {
            foreach (KeyValuePair<string, SpawnPool> pair in this._pools)
                UnityEngine.Object.Destroy(pair.Value);

            // Clear the dict in case the user re-creates a SpawnPool of the same name later
            this._pools.Clear();
        }
        #endregion Public Custom Memebers

        #region Dict Functionality
        // Internal (wrapped) dictionary
        private Dictionary<string, SpawnPool> _pools = new Dictionary<string, SpawnPool>();

        /// <summary>
        /// Used internally by SpawnPools to add themseleves on Awake().
        /// Use PoolManager.CreatePool() to create an entirely new SpawnPool GameObject
        /// </summary>
        /// <param name="spawnPool"></param>
        internal void Add(SpawnPool spawnPool)
        {
            // Don't let two pools with the same name be added. See error below for details
            if (this.ContainsKey(spawnPool.poolName))
            {
                Debug.LogError(string.Format("A pool with the name '{0}' already exists. " +
                                                "This should only happen if a SpawnPool with " +
                                                "this name is added to a scene twice.",
                                             spawnPool.poolName));
                return;
            }

            this._pools.Add(spawnPool.poolName, spawnPool);

            if (this.onCreatedDelegates.ContainsKey(spawnPool.poolName))
                this.onCreatedDelegates[spawnPool.poolName](spawnPool);
        }


        /// <summary>
        /// Used internally by SpawnPools to remove themseleves on Destroy().
        /// Use PoolManager.Destroy() to destroy an entire SpawnPool GameObject.
        /// </summary>
        /// <param name="spawnPool"></param>
        internal bool Remove(SpawnPool spawnPool)
        {
            if (!this.ContainsKey(spawnPool.poolName) & Application.isPlaying)
            {
                Debug.LogError(string.Format("PoolManager: Unable to remove '{0}'. " +
                                                "Pool not in PoolManager",
                                            spawnPool.poolName));
                return false;
            }

            this._pools.Remove(spawnPool.poolName);
            return true;
        }

        /// <summary>
        /// Get the number of SpawnPools in PoolManager
        /// </summary>
        public int Count { get { return this._pools.Count; } }

        /// <summary>
        /// Returns true if a pool exists with the passed pool name.
        /// </summary>
        /// <param name="poolName">The name to look for</param>
        /// <returns>True if the pool exists, otherwise, false.</returns>
        public bool ContainsKey(string poolName)
        {
            return this._pools.ContainsKey(poolName);
        }

        /// <summary>
        /// Used to get a SpawnPool when the user is not sure if the pool name is used.
        /// This is faster than checking IsPool(poolName) and then accessing Pools][poolName.]
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string poolName, out SpawnPool spawnPool)
        {
            return this._pools.TryGetValue(poolName, out spawnPool);
        }

        #region IEnumerable<KeyValuePair<string,SpawnPool>> Members
        public IEnumerator<KeyValuePair<string, SpawnPool>> GetEnumerator()
        {
            return this._pools.GetEnumerator();
        }
        #endregion


        #endregion Dict Functionality

    }


}


