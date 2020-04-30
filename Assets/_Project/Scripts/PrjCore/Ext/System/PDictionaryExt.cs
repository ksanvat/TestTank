using System.Collections.Generic;

namespace PrjCore.Ext.System {
    public static class PDictionaryExt {

        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, TValue defaultValue = default) {
            return self.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static bool TryGetAndRemoveValue<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, out TValue value) {
            if (!self.TryGetValue(key, out value)) {
                return false;
            }

            self.Remove(key);
            return true;
        }

    }
}