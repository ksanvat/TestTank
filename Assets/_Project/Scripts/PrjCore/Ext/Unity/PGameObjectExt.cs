using PrjCore.Unit;
using UnityEngine;

namespace PrjCore.Ext.Unity {
    public static class PGameObjectExt {

        public static int GetGOInstanceID(this GameObject self) {
            // according to the source code, GetInstanceID makes EnsureRunningOnMainThread call every time
            // on the other hand, GetHashCode returns the same value without thread checks
            // so for performance reasons GetHashCode is used in production
            // source code:
            // https://github.com/Unity-Technologies/UnityCsReference/blob/b376c3ae190fd054f4dfd01899fb9ecdf4f160e8/Runtime/Export/Scripting/UnityEngineObject.bindings.cs#L69
            
#if (DEVELOPMENT_BUILD || UNITY_EDITOR) && !PRJ_FORCED_PRODUCTION_BUILD
            return self.GetInstanceID();
#else
            return self.GetHashCode();
#endif
        }

        public static void DestroyComponent<T>(this GameObject self) where T : MonoBehaviour {
            var component = self.GetComponent<T>();
            if (component != null) {
                Object.Destroy(component);
            }
        }

        public static GameObject CreateUnit(this GameObject self) {
            return PUnitManager.CreateUnit(self);
        }

        public static void DestroyUnit(this GameObject self) {
            PUnitManager.DestroyUnit(self);
        }
        
    }
}