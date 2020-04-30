using System;
using Prj.Projectile;
using PrjCore.Character.Item;
using PrjCore.Ext.Unity;
using PrjCore.Util.System;
using UnityEngine;

namespace Prj.Character.Item {
    public class PTankWeapon : PBaseCharacterItem {

        [SerializeField] private Config _weaponConfig = new Config();
        private State _weaponState = new State();

        public float Damage {
            set => st.Damage = value;
        }

        public float ReloadTime {
            set => st.ReloadTime = value;
        }

        public float ProjectileVelocity {
            set => st.ProjectileVelocity = value;
        }

        private Config cnf => _weaponConfig;
        private State st => _weaponState;
        
        public void TryShot() {
            if (Time.time < st.ReloadTimeEnd) {
                return;
            }

            st.ReloadTimeEnd = Time.time + st.ReloadTime;

            var projectileUnit = cnf.ProjectilePrefab.CreateUnit();
            if (projectileUnit == null) {
                Debug.LogError("Cannot create projectile prefab");
                return;
            }

            projectileUnit.transform.position = st.ShotPoint.position;
            projectileUnit.transform.rotation = transform.rotation;
            projectileUnit.Rigidbody2D.velocity = PMathUtil.PolarToDecart(st.ProjectileVelocity, transform.Rotation2DRad());
            projectileUnit.Damage = st.Damage;
        }

        public override void OnInit() {
            base.OnInit();

            st.ReloadTimeEnd = 0;
        }

        protected void Awake() {
            SetFieldRef(out st.ShotPoint, "ShotPoint");
        }

        [Serializable]
        private class Config {
            public PProjectile ProjectilePrefab = null;
        }

        [Serializable]
        private class State {
            public Transform ShotPoint;
            
            public float Damage;
            public float ReloadTime;
            public float ProjectileVelocity;

            public float ReloadTimeEnd;
        }
        
    }
}