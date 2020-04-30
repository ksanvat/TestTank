using System;
using System.Collections;
using PrjCore.Ext.Unity;
using PrjCore.Projectile;
using PrjCore.Unit;
using UnityEngine;

namespace Prj.Character {
    public class PProjectile : PBaseProjectile {

        private State _bulletState = new State();

        public float Damage {
            set => st.Damage = value;
        }

        public Rigidbody2D Rigidbody2D => st.Rigidbody2D;
        
        private State st => _bulletState;

        public override void OnInit() {
            base.OnInit();

            st.AutoDestroyer = StartCoroutine(AutoDestroyer());
        }

        public override void OnDeinit() {
            base.OnDeinit();

            if (st.AutoDestroyer != null) {
                StopCoroutine(st.AutoDestroyer);
                st.AutoDestroyer = null;
            }
        }

        protected void Awake() {
            SetFieldRef(out st.Rigidbody2D);
        }

        protected void OnTriggerEnter2D(Collider2D other) {
            var enemyUnit = other.GetComponent<PEnemy>();
            if (enemyUnit == null) {
                return;
            }
            
            enemyUnit.ReceiveDamage(st.Damage);
            this.DestroyUnit();
        }

        private IEnumerator AutoDestroyer() {
            yield return new WaitForSeconds(5f);
            this.DestroyUnit();
        }
        
        [Serializable]
        private class State {
            public Rigidbody2D Rigidbody2D;

            public Coroutine AutoDestroyer;
            
            public float Damage;
        }
    }
}