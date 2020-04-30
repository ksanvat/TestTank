using UnityEngine;

namespace PrjCore.Ext.Unity {
    public static class PResourcesExt {

        public static T[] LoadAllPrefabs<T>() where T : Object {
            return Resources.LoadAll<T>("Prefabs");
        }
        
    }
}