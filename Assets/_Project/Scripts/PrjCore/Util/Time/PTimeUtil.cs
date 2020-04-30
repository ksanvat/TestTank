using System;

namespace PrjCore.Util.Time {
    public static class PTimeUtil {
        public static long GetUtcMilliseconds() {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}