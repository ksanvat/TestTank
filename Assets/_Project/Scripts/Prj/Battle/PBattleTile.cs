using System;
using PrjCore.Unit;
using UnityEngine;

namespace Prj.Battle {
    [RequireComponent(typeof(SpriteRenderer))]
    public class PBattleTile : PUnit {

        private State _battleTileState = new State();

        public Vector2 Size {
            set => st.SpriteRenderer.size = value;
        }

        private State st => _battleTileState;

        protected void Awake() {
            SetFieldRef(out st.SpriteRenderer);
        }

        [Serializable]
        private class State {
            public SpriteRenderer SpriteRenderer;
        }
        
    }
}