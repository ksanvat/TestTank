using System.Linq;
using PrjCore.Ext.System;
using PrjCore.Ext.Unity;
using UnityEngine;

namespace PrjCore.Util.Singleton {
    public abstract class PMonoBehaviourSingleton<T> : PBaseMonoBehaviourSingleton where T : PMonoBehaviourSingleton<T> {
        
        private static T _instance = null;

        // ReSharper disable once StaticMemberInGenericType
        private static bool _applicationIsQuitting;
        
        // ReSharper disable once StaticMemberInGenericType
        private static object _lock = new object();

        protected static bool ApplicationIsQuitting => _applicationIsQuitting;

        protected static T Instance {
            get {
                if (_applicationIsQuitting) {
                    return null;
                }

                if (_instance == null) {
                    lock (_lock) {
                        if (_instance == null) {
                            _instance = CreateInstance();
                        }
                    }
                }

                return _instance;
            }
        }

        public static void ForceInit() {
            Instance.Noop();
        }
        
        protected virtual void OnInit() {}

        protected virtual void OnDestroy() {
            _applicationIsQuitting = true;
        }

        private static T CreateInstance() {
            var sceneInstances = FindObjectsOfType<T>();
            if (sceneInstances.IsNotNullOrEmpty()) {
                Debug.LogErrorFormat(
                    "Unexpected instances of {0} singleton: {1}", 
                    typeof(T), 
                    string.Join(", ", sceneInstances.Select(x => x.name))
                );
                return sceneInstances[0];
            }

            GameObject instGO;
            T instT;
            var prefabComponents = PResourcesExt.LoadAllPrefabs<T>();
            if (prefabComponents.IsNullOrEmpty()) {
                instGO = new GameObject($"[Singleton] {typeof(T)}");
                instT = instGO.AddComponent<T>();
            } else {
                if (prefabComponents.Length > 1) {
                    Debug.LogErrorFormat(
                        "Unexpected instances of {0} singleton: {1}", 
                        typeof(T), 
                        string.Join(", ", prefabComponents.Select(x => x.name))
                    );
                }
                
                instGO = Instantiate(prefabComponents[0].gameObject);
                instT = instGO.GetComponent<T>();
            }

            DontDestroyOnLoad(instGO);
            instT.OnInit();

            return instT;
        }
	    
        private void Noop() {}
        
    }
}