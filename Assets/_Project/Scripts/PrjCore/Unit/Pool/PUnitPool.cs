using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrjCore.Ext.System;
using PrjCore.Ext.Unity;
using PrjCore.System;
using PrjCore.System.Time;
using PrjCore.Util.Singleton;
using PrjCore.Util.System;
using PrjCore.Util.Unity;
using UnityEngine;

namespace PrjCore.Unit.Pool {
    public class PUnitPool : PMonoBehaviourSingleton<PUnitPool> {

        [SerializeField] private Config _unitPoolConfig = new Config();
        private State _unitPoolState = new State();

        private Config cnf => _unitPoolConfig;
        private State st => _unitPoolState;

        public static GameObject FetchCachedOrCreateUnit(GameObject prefabGO) {
            return Instance.InternalFetchCachedOrCreateUnit(prefabGO);
        }

        public static void PutCachedOrDestroyUnit(GameObject unitGO, out bool isCached) {
            Instance.InternalPutCachedOrDestroyUnit(unitGO, out isCached);
        }

        public static void DestroyUnit(GameObject unitGO) {
            Instance.InternalDestroyUnit(unitGO);
        }

        protected override void OnInit() {
            base.OnInit();
            
            st.PoolMainTransform = new GameObject("UnitPool").transform;
            st.PoolTransforms = new Dictionary<int, Transform>();
            st.PrefabInfos = new Dictionary<int, PrefabInfo>();
            st.PoolGO = new Dictionary<int, Queue<GameObject>>();
            st.UnitGOPrefabInfo = new Dictionary<GameObject, PrefabInfo>();
            
            foreach (var prefabGO in PResourcesExt.LoadAllPrefabs<GameObject>()) {
                AddPrefabInfo(prefabGO);
            }

            StartCoroutine(OnInitFillPool());
            StartCoroutine(CheckForLeaks());
        }

        private IEnumerator OnInitFillPool() {
            var timer = new PUtcTimer();
            
            foreach (var kvp in st.PrefabInfos) {
                var prefabInfo = kvp.Value;

                for (uint i = 0; i < prefabInfo.InitialInstances; ++i) {
                    if (timer.ElapsedMilliseconds > cnf.InitialFillPoolStepMS) {
                        yield return null;
                        timer.Start();
                    }

                    var unitGO = CreateUnitFromPrefab(prefabInfo.PrefabGO);
                    if (unitGO != null) {
                        PutUnitIntoPool(unitGO);
                    }
                }
            }
        }

        private IEnumerator CheckForLeaks() {
            var waitForSeconds = new WaitForSeconds(cnf.CheckForLeaksPeriodSec);
            var swapUnitGOPrefabInfo = new Dictionary<GameObject, PrefabInfo>();
            
            while (!ApplicationIsQuitting) {

                bool leak = st.UnitGOPrefabInfo.Keys.Any(go => go == null);
                if (leak) {
                    foreach (var kvp in st.UnitGOPrefabInfo) {
                        if (kvp.Key == null) {
                            Debug.LogErrorFormat("Leak for prefab {0}", kvp.Value.PrefabGO.name);
                            continue;
                        }
                        
                        swapUnitGOPrefabInfo.Add(kvp.Key, kvp.Value);
                    }
                    
                    PObjectUtil.Swap(ref st.UnitGOPrefabInfo, ref swapUnitGOPrefabInfo);
                    swapUnitGOPrefabInfo.Clear();
                }

                yield return waitForSeconds;
            }
        }

        private GameObject InternalFetchCachedOrCreateUnit(GameObject prefabGO) {
            if (prefabGO == null) {
                Debug.LogErrorFormat("Cannot process creation of null {0}", nameof(prefabGO));
                return null;
            }
            
            var obj = FetchUnitFromPool(prefabGO);
            if (obj == null) {
                obj = CreateUnitFromPrefab(prefabGO);
            }
            return obj;
        }

        private void InternalPutCachedOrDestroyUnit(GameObject unitGO, out bool isCached) {
            if (unitGO == null) {
                Debug.LogErrorFormat("Cannot process destruction of null {0}", nameof(unitGO));
                isCached = false;
                return;
            }

            isCached = PutUnitIntoPool(unitGO);
            if (!isCached) {
                InternalDestroyUnit(unitGO);
            }
        }
        
        private void InternalDestroyUnit(GameObject unitGO) {
            if (unitGO == null) {
                Debug.LogErrorFormat("Cannot destroy null {0}", nameof(unitGO));
                return;
            }

            RemoveUnitPrefabInfo(unitGO);
            Destroy(unitGO);
        }
        
        private GameObject CreateUnitFromPrefab(GameObject prefabGO) {
            PAssert.IsNotNull(prefabGO, nameof(prefabGO));

            var prefabInfo = GetPrefabInfo(prefabGO);
            if (prefabInfo == null) {
                return null;
            }

            GameObject unitGO;
            try {
                unitGO = Instantiate(prefabGO);
            } catch (UnityException e) {
                Debug.LogErrorFormat("Cannot create {0} due to exception", nameof(unitGO));
                Debug.LogException(e);
                return null;
            }

            if (!AddUnitPrefabInfo(unitGO, prefabInfo)) {
                InternalDestroyUnit(unitGO);
                return null;
            }
            
            if (prefabInfo.HaveUnitPoolInfoComponent) {
                unitGO.DestroyComponent<PBaseUnitPoolInfo>();
            }

            unitGO.SetActive(true);
            
            return unitGO;
        }

        private GameObject FetchUnitFromPool(GameObject prefabGO) {
            PAssert.IsNotNull(prefabGO, nameof(prefabGO));

            var poolUnitsGO = GetPoolQueue(prefabGO);
            if (poolUnitsGO == null) {
                return null;
            }

            var unitGO = poolUnitsGO.Dequeue();
            if (unitGO == null) {
                Debug.LogErrorFormat("Unexpected null {0} in pool for prefab {1}", nameof(unitGO), prefabGO.name);
                return null;
            }
            
            unitGO.transform.SetParent(null);
            unitGO.SetActive(true);
            
            return unitGO;
        }

        private bool PutUnitIntoPool(GameObject unitGO) {
            PAssert.IsNotNull(unitGO, nameof(unitGO));

            var prefabInfo = GetUnitPrefabInfo(unitGO);
            if (prefabInfo == null) {
                return false;
            }
            
            var poolUnits = SetDefaultAndGetPoolQueue(prefabInfo.PrefabGO);
            if (poolUnits.Count >= prefabInfo.MaxInstances) {
                return false;
            }

            unitGO.SetActive(false);
            unitGO.transform.SetParent(GetPrefabTransform(prefabInfo.PrefabGO));
            poolUnits.Enqueue(unitGO);

            return true;
        }

        private Queue<GameObject> SetDefaultAndGetPoolQueue(GameObject prefabGO) {
            PAssert.IsNotNull(prefabGO, nameof(prefabGO));
            
            int prefabID = prefabGO.GetGOInstanceID();
            if (!st.PoolGO.TryGetValue(prefabID, out var poolUnitsGO)) {
                poolUnitsGO = new Queue<GameObject>();
                st.PoolGO.Add(prefabID, poolUnitsGO);
            }
            
            PAssert.IsNotNull(poolUnitsGO, nameof(poolUnitsGO));

            return poolUnitsGO;
        }
        
        private Queue<GameObject> GetPoolQueue(GameObject prefabGO) {
            PAssert.IsNotNull(prefabGO, nameof(prefabGO));
            
            if (!st.PoolGO.TryGetValue(prefabGO.GetGOInstanceID(), out var poolUnitsGO)) {
                return null;
            }
            
            PAssert.IsNotNull(poolUnitsGO, nameof(poolUnitsGO));

            if (poolUnitsGO.IsEmpty()) {
                return null;
            }

            return poolUnitsGO;
        }
        
        private bool AddUnitPrefabInfo(GameObject unitGO, PrefabInfo prefabInfo) {
            PAssert.IsNotNull(unitGO, nameof(unitGO));
            PAssert.IsNotNull(prefabInfo, nameof(prefabInfo));

            try {
                st.UnitGOPrefabInfo.Add(unitGO, prefabInfo);
            } catch (ArgumentException) {
                Debug.LogErrorFormat(
                    "Duplicate key {0} of {1} in {2}", 
                    unitGO.GetGOInstanceID(), 
                    unitGO.name, 
                    nameof(st.UnitGOPrefabInfo)
                );
                return false;
            }

            return true;
        }
        
        private PrefabInfo GetUnitPrefabInfo(GameObject unitGO) {
            PAssert.IsNotNull(unitGO, nameof(unitGO));
            
            if (!st.UnitGOPrefabInfo.TryGetValue(unitGO, out var prefabInfo)) {
                Debug.LogErrorFormat("Unit {0} have no {1} in {2}", unitGO.name, nameof(prefabInfo), nameof(st.UnitGOPrefabInfo));
                return null;
            }
            
            PAssert.IsNotNull(prefabInfo, nameof(prefabInfo));
            
            return prefabInfo;
        }

        private void RemoveUnitPrefabInfo(GameObject unitGO) {
            PAssert.IsNotNull(unitGO, nameof(unitGO));
            
            st.UnitGOPrefabInfo.Remove(unitGO);
        }
        
        private void AddPrefabInfo(GameObject prefabGO) {
            PAssert.IsNotNull(prefabGO, nameof(prefabGO));
            
            var poolInfoComponent = prefabGO.GetComponent<PBaseUnitPoolInfo>();
            PrefabInfo prefabInfo;
            if (poolInfoComponent != null) {
                prefabInfo = new PrefabInfo {
                    PrefabGO = prefabGO,
                    InitialInstances = poolInfoComponent.InitialInstances,
                    MaxInstances = poolInfoComponent.MaxInstances,
                    HaveUnitPoolInfoComponent = true
                };
            } else {
                prefabInfo = new PrefabInfo {
                    PrefabGO = prefabGO,
                    InitialInstances = cnf.DefaultInitialInstances,
                    MaxInstances = cnf.DefaultMaxInstances,
                    HaveUnitPoolInfoComponent = false
                };
            }

            try {
                st.PrefabInfos.Add(prefabGO.GetGOInstanceID(), prefabInfo);
            } catch (ArgumentException) {
                Debug.LogErrorFormat("Duplicate of {0} prefab in {1}", prefabGO.name, nameof(st.PrefabInfos));
            }
        }

        private PrefabInfo GetPrefabInfo(GameObject prefabGO) {
            PAssert.IsNotNull(prefabGO, nameof(prefabGO));
            
            if (!st.PrefabInfos.TryGetValue(prefabGO.GetGOInstanceID(), out var prefabInfo)) {
                Debug.LogErrorFormat("Prefab {0} have no {1} in {2}", prefabGO.name, nameof(prefabInfo), nameof(st.PrefabInfos));
                return null;
            }
            
            PAssert.IsNotNull(prefabInfo, nameof(prefabInfo));

            return prefabInfo;
        }
        
        private Transform GetPrefabTransform(GameObject prefabGO) {
            PAssert.IsNotNull(prefabGO, nameof(prefabGO));
            
            int prefabID = prefabGO.GetGOInstanceID();
            if (!st.PoolTransforms.TryGetValue(prefabID, out var prefabTransform)) {
                prefabTransform = new GameObject($"{prefabGO.name}_{prefabID}").transform;
                prefabTransform.SetParent(st.PoolMainTransform);
                st.PoolTransforms.Add(prefabID, prefabTransform);
            }

            return prefabTransform;
        }

        [Serializable]
        private class PrefabInfo {
            public GameObject PrefabGO;
            public int InitialInstances;
            public int MaxInstances;
            public bool HaveUnitPoolInfoComponent;
        }

        [Serializable]
        private class Config {
            public int DefaultInitialInstances = 0;
            public int DefaultMaxInstances = 0;

            public long InitialFillPoolStepMS = 15;

            public float CheckForLeaksPeriodSec = 60f;
        }

        [Serializable]
        private class State {
            public Transform PoolMainTransform;
            public Dictionary<int, Transform> PoolTransforms;
        
            public Dictionary<int, PrefabInfo> PrefabInfos;
            public Dictionary<int, Queue<GameObject>> PoolGO;

            public Dictionary<GameObject, PrefabInfo> UnitGOPrefabInfo;
        }
        
    }
}