using UnityEngine;

namespace PrjCore.Unit.Pool {
    public abstract class PBaseUnitPoolInfo : MonoBehaviour {

        public abstract int InitialInstances { get; }
        public abstract int MaxInstances { get; }
        
    }
}