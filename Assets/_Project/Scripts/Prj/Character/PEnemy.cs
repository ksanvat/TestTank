using System;
using PrjCore.Character;
using UnityEngine;

namespace Prj.Character {
    public class PEnemy : PBaseCharacter {
        
        private State _enemyState = new State();

        private State st => _enemyState;

        public float Damage {
            get => st.Damage;
            set => st.Damage = value;
        }

        public Func<PBaseCharacter> OpponentGetter {
            get => st.OpponentGetter;
            set => st.OpponentGetter = value;
        }

        public override void OnDeinit() {
            base.OnDeinit();

            st.OpponentGetter = null;
        }

        private void OnCollisionEnter2D(Collision2D other) {
            var tankUnit = other.transform.GetComponentInParent<PTank>();
            if (tankUnit == null) {
                return;
            }
            
            tankUnit.ReceiveDamage(st.Damage);
            Kill();
        }

        [Serializable]
        private class State {
            public float Damage;
            public Func<PBaseCharacter> OpponentGetter;
        }
    }
}