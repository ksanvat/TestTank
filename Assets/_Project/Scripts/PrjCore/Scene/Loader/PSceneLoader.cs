using System;
using PrjCore.Scene.Controller;
using PrjCore.Unit;
using PrjCore.Unit.Pool;
using PrjCore.Util.Data;
using UnityEngine;

namespace PrjCore.Scene.Loader {
    public class PSceneLoader : MonoBehaviour {

        [SerializeField] private Config _sceneLoaderConfig = new Config();

        private Config cnf => _sceneLoaderConfig;
        
        protected void Awake() {
            PUnitPool.ForceInit();
            PUnitManager.ForceInit();
        }

        protected void Start() {
            PUnitManager.CreateUnit(cnf.SceneController);
            
            Destroy(gameObject);
        }

        [Serializable]
        private class Config {
            public PBaseSceneController SceneController = null;
        }
        
    }
}