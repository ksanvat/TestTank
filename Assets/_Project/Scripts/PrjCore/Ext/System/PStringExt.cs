namespace PrjCore.Ext.System {
    public static class PStringExt {

        public static bool IsNullOrEmpty(this string self) {
            return string.IsNullOrEmpty(self);
        }

        public static bool IsNullOrWhiteSpace(this string self) {
            return string.IsNullOrWhiteSpace(self);
        }
        
    }
}