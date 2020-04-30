using System.Diagnostics;
using UnityEngine.Assertions;

namespace PrjCore.Util.Unity {
    public static class PAssert {

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsNotNull<T>(T obj, string objName) where T : class {
            Assert.IsNotNull(obj, $"{objName} is null");
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsNull<T>(T obj, string objName) where T : class {
            Assert.IsNull(obj, $"{objName} is not null");
        }
        
    }
}