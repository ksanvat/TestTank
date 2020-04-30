using System.Collections;

namespace PrjCore.Ext.System {
    public static class PCollectionExt {

        public static bool IsEmpty(this ICollection self) {
            return self.Count <= 0;
        }
        
        public static bool IsNotEmpty(this ICollection self) {
            return IsEmpty(self);
        }
        
        public static bool IsNullOrEmpty(this ICollection self) {
            return self == null || self.Count <= 0;
        }

        public static bool IsNotNullOrEmpty(this ICollection self) {
            return !IsNullOrEmpty(self);
        }

    }
}