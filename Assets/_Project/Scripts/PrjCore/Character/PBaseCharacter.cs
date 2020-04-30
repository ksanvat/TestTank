using System;
using PrjCore.Unit;
using UnityEngine;

namespace PrjCore.Character {
    [RequireComponent(typeof(Rigidbody2D))]
    public class PBaseCharacter : PUnit {

        private State _baseCharacterState = new State();

        public float HP {
            get => st.HP;
            set => st.HP = value;
        }

        public float Vulnerability {
            get => st.Vulnerability;
            set => st.Vulnerability = value;
        }

        public float MovementSpeed {
            get => st.MovementSpeed;
            set => st.MovementSpeed = value;
        }

        public float RotationSpeed {
            get => st.RotationSpeed;
            set => st.RotationSpeed = value;
        }

        public Rigidbody2D Rigidbody2D => st.Rigidbody2D;
        
        private State st => _baseCharacterState;
        
        public void SetOnDieCallback(Action callback) {
            st.dieCallback = callback;
        }

        public void Kill() {
            CallDieCallback();
        }

        public void ReceiveDamage(float damage) {
            st.HP -= damage * st.Vulnerability;
            if (st.HP <= 0) {
                CallDieCallback();
            }
        }

        protected void CallDieCallback() {
            st.dieCallback?.Invoke();
        }
        
        protected virtual void Awake() {
            SetFieldRef(out st.Rigidbody2D);
        }

        [Serializable]
        private class State {
            public Rigidbody2D Rigidbody2D;
            
            public float HP;
            public float Vulnerability;
            public float MovementSpeed;
            public float RotationSpeed;

            public Action dieCallback;
        }
        
    }
}