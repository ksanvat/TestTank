using PrjCore.Util.Time;

namespace PrjCore.System.Time {
    public class PUtcTimer {

        private long _msStart;
        private long _msFinish;
        private bool _isRunning;
        
        public PUtcTimer() {
            Start();
        }

        public long ElapsedMilliseconds => _isRunning ? (PTimeUtil.GetUtcMilliseconds() - _msStart) : (_msFinish - _msStart);

        public void Start() {
            _msStart = PTimeUtil.GetUtcMilliseconds();
            _isRunning = true;
        }

        public void Stop() {
            _msFinish = PTimeUtil.GetUtcMilliseconds();
            _isRunning = false;
        }
        
    }
}