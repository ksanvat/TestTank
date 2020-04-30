using System;
using System.Collections;
using System.Collections.Generic;
using PrjCore.Ext.System;
using PrjCore.Util.Singleton;
using PrjCore.Util.System;
using UnityEngine;

namespace PrjCore.System.AsyncTask {
    public class PInternalAsyncTaskManager : PMonoBehaviourSingleton<PInternalAsyncTaskManager> {

        [SerializeField] private Config _managerConfig = new Config();
        private State _managerState = new State();

        private Config cnf => _managerConfig;
        private State st => _managerState;

        public static PInternalAsyncTaskManager AsOwner() {
            return Instance;
        }

        public bool Add(PBaseAsyncTask task) {
            if (task == null) {
                return false;
            }

            st.RunningTasks.Add(task.ID, task);
            return true;
        }

        public void Remove(ulong taskID) {
            if (!st.RunningTasks.TryGetAndRemoveValue(taskID, out var task)) {
                return;
            }

            if (TryDispose(task)) {
                return;
            }

            st.DisposingTasks.Add(task);
        }
        
        public ulong GenerateID() {
            return InternalGenerateID();
        }

        protected override void OnInit() {
            base.OnInit();

            st.IDGenerator = 0;
            st.RunningTasks = new Dictionary<ulong, PBaseAsyncTask>();
            st.DisposingTasks = new HashSet<PBaseAsyncTask>();

            StartCoroutine(DisposeCATs());
        }

        private ulong InternalGenerateID() {
            return st.IDGenerator++;
        }

        private IEnumerator DisposeCATs() {
            var waitForSeconds = new WaitForSeconds(cnf.DisposeCATsPeriodSec);
            var swapDisposingCATs = new HashSet<PBaseAsyncTask>();
            
            while (!ApplicationIsQuitting) {

                foreach (var cat in st.DisposingTasks) {
                    if (!TryDispose(cat)) {
                        swapDisposingCATs.Add(cat);
                    }
                }
                
                PObjectUtil.Swap(ref st.DisposingTasks, ref swapDisposingCATs);
                swapDisposingCATs.Clear();
                
                yield return waitForSeconds;
            }
        }

        private bool TryDispose(PBaseAsyncTask task) {
            return PBaseAsyncTask.Internal.TryDispose(task);
        }

        [Serializable]
        private class Config {
            public float DisposeCATsPeriodSec = 10f;
        }

        [Serializable]
        private class State {
            public ulong IDGenerator;

            public Dictionary<ulong, PBaseAsyncTask> RunningTasks;
            public HashSet<PBaseAsyncTask> DisposingTasks;
        }
        
    }
}