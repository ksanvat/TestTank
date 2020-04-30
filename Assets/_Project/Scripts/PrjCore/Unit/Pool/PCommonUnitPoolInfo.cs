using System;
using UnityEngine;

namespace PrjCore.Unit.Pool {
    public class PCommonUnitPoolInfo : PBaseUnitPoolInfo {
        
        [SerializeField] private Config _unitPoolInfoConfig = new Config();
        
        public override int InitialInstances => cnf.InitialInstances;
        public override int MaxInstances => cnf.MaxInstances;

        private Config cnf => _unitPoolInfoConfig;

        [Serializable]
        private class Config {
            public int InitialInstances = 0;
            public int MaxInstances = 0;
        }
        
    }
}