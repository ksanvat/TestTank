namespace PrjCore.Util.System {
    public static class PObjectUtil {

        public static void Swap<T>(ref T lhs, ref T rhs) {
            var tmp = lhs;
            lhs = rhs;
            rhs = tmp;
        }
        
    }
}