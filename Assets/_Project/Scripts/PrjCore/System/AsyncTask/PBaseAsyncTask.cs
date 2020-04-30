using PrjCore.Util.Unity;

namespace PrjCore.System.AsyncTask {
    public abstract class PBaseAsyncTask {

        public abstract ulong ID { get; }
        
        protected abstract bool TryDispose();
        
        public static class Internal {

            public static bool TryDispose(PBaseAsyncTask self) {
                PAssert.IsNotNull(self, nameof(self));
                return self.TryDispose();
            }
            
        }
    }
}