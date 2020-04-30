using System;
using PrjCore.Unit;
using UnityEngine;

namespace Prj.Battle {
    [RequireComponent(typeof(Camera))]
    public class PBattleCamera : PUnit {

        private State _cameraState = new State();

        public Camera Camera => st.Camera;
        
        private State st => _cameraState;
        
        public void GetMinMaxWorldPositions(out Vector2 min, out Vector2 max) {
            min = st.Camera.ViewportToWorldPoint(Vector3.zero);
            max = st.Camera.ViewportToWorldPoint(Vector3.one);
        }

        protected void Awake() {
            SetFieldRef(out st.Camera);
        }

        [Serializable]
        private class State {
            public Camera Camera;
        }
        
    }
}