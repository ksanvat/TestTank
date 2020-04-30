using System;
using System.Collections.Generic;
using PrjCore.Ext.Unity;
using PrjCore.Unit.Pool;
using PrjCore.Util.Singleton;
using PrjCore.Util.Unity;
using UnityEngine;

namespace PrjCore.Unit {
    public class PUnitManager : PMonoBehaviourSingleton<PUnitManager> {

        private State st = new State();
        
        public static GameObject CreateUnit(GameObject prefabGO) {
            try {
                return Instance.InternalCreateUnit(prefabGO);
            } catch (Exception e) {
                Debug.LogErrorFormat("Unexpected exception on unit creation from {0}", nameof(prefabGO));
                Debug.LogException(e);
                return null;
            }
        }

        public static T CreateUnit<T>(T prefabComponent) where T : PUnit {
            try {
                return Instance.InternalCreateUnit(prefabComponent);
            } catch (Exception e) {
                Debug.LogErrorFormat("Unexpected exception on unit creation from {0}", nameof(prefabComponent));
                Debug.LogException(e);
                return null;
            }
        }
        
        public static void DestroyUnit(GameObject unitGO) {
            try {
                Instance.InternalDestroyUnit(unitGO);
            } catch (Exception e) {
                Debug.LogErrorFormat("Unexpected exception on {0} destruction", nameof(unitGO));
                Debug.LogException(e);
            }
        }

        public static void DestroyUnit<T>(T unitComponent) where T : PUnit {
            try {
                Instance.InternalDestroyUnit(unitComponent);
            } catch (Exception e) {
                Debug.LogErrorFormat("Unexpected exception on {0} destruction", nameof(unitComponent));
                Debug.LogException(e);
            }
        }

        protected override void OnInit() {
            base.OnInit();
            
            st.UnitComponentCache = new Dictionary<int, PUnit>();
        }
        
        private GameObject InternalCreateUnit(GameObject prefabGO) {
            if (prefabGO == null) {
                Debug.LogErrorFormat("Cannot create unit of null {0}", nameof(prefabGO));
                return null;
            }

            var unitGO = PUnitPool.FetchCachedOrCreateUnit(prefabGO);
            if (unitGO == null) {
                return null;
            }
            
            var unit = GetCacheableUnitComponent<PUnit>(unitGO);
            if (unit != null) {
                unit.OnInit();
            }

            return unitGO;
        }

        private T InternalCreateUnit<T>(T prefabComponent) where T : PUnit {
            if (prefabComponent == null) {
                Debug.LogErrorFormat("Cannot create unit of null {0}", nameof(prefabComponent));
                return null;
            }
            
            var unitGO = PUnitPool.FetchCachedOrCreateUnit(prefabComponent.gameObject);
            if (unitGO == null) {
                Debug.LogErrorFormat("Unexpected error while creating {0}", nameof(unitGO));
                return null;
            }

            var unitComponent = GetCacheableUnitComponent<T>(unitGO);
            if (unitComponent == null) {
                Debug.LogErrorFormat("Unexpected error while getting {0}", nameof(unitComponent));
                PUnitPool.DestroyUnit(unitGO);
                return null;
            }
            
            unitComponent.OnInit();

            return unitComponent;
        }

        private void InternalDestroyUnit(GameObject unitGO) {
            if (unitGO == null) {
                return;
            }

            int unitID = unitGO.GetGOInstanceID();
            var unit = GetCacheableUnitComponent<PUnit>(unitGO);
            if (unit != null) {
                unit.OnDeinit();
            }

            PUnitPool.PutCachedOrDestroyUnit(unitGO, out bool isCached);
            if (!isCached) {
                RemoveCacheableUnitComponent(unitID);
            }
        }

        private void InternalDestroyUnit<T>(T unitComponent) where T : PUnit {
            if (unitComponent == null) {
                return;
            }

            int unitID = unitComponent.GetGOInstanceID();
            unitComponent.OnDeinit();

            PUnitPool.PutCachedOrDestroyUnit(unitComponent.gameObject, out bool isCached);
            if (!isCached) {
                RemoveCacheableUnitComponent(unitID);
            }
        }

        private T GetCacheableUnitComponent<T>(GameObject unitGO) where T : PUnit {
            PAssert.IsNotNull(unitGO, nameof(unitGO));
            
            int unitID = unitGO.GetGOInstanceID();
            if (!st.UnitComponentCache.TryGetValue(unitID, out var unit)) {
                unit = unitGO.GetComponent<T>();
                st.UnitComponentCache.Add(unitID, unit);
            }

            try {
                return (T) unit;
            } catch (InvalidCastException) {
                return null;
            }
        }

        private void RemoveCacheableUnitComponent(int unitID) {
            st.UnitComponentCache.Remove(unitID);
        }

        [Serializable]
        private class State {
            public Dictionary<int, PUnit> UnitComponentCache;
        }
        
    }
}