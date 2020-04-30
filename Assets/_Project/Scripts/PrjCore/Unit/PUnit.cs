using UnityEngine;

namespace PrjCore.Unit {
    [DisallowMultipleComponent]
    public class PUnit : MonoBehaviour {

        public virtual void OnInit() {}
        
        public virtual void OnDeinit() {}
        
        #region SetField
	    
        protected void SetFieldRef<T>(out T field) {
            SetFieldRef(out field, transform);
        }

        protected void SetFieldRef<T>(out T field, string path) {
            SetFieldRef(out field, transform, path);
        }

        protected void SetFieldRef(out Transform field, string path) {
            SetFieldRef(out field, transform, path);
        }

        protected void SetFieldRef(out GameObject field, string path) {
            SetFieldRef(out field, transform, path);
        }

        protected static void SetFieldRef<T>(out T field, Transform transform) {
            field = transform.GetComponent<T>();
        }

        protected static void SetFieldRef<T>(out T field, Transform transform, string path) {
            var t = transform.Find(path);
            field = t != null ? t.GetComponent<T>() : default(T);  
        }

        protected static void SetFieldRef(out Transform field, Transform transform, string path) {
            field = transform.Find(path);
        }

        protected static void SetFieldRef(out GameObject field, Transform transform, string path) {
            var t = transform.Find(path);
            field = t != null ? t.gameObject : null;
        }
	    
        #endregion
        
    }
}