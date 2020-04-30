using System;
using System.Collections.Generic;
using Prj.Character.Item;
using PrjCore.Character;
using PrjCore.Ext.System;
using PrjCore.Ext.Unity;
using UnityEngine;

namespace Prj.Character {
    public class PTank : PBaseCharacter {
        
        [SerializeField] private Config _tankConfig = new Config();
        private State _tankState = new State();

        private Config cnf => _tankConfig;
        private State st => _tankState;
        
        public void AddWeapon(PTankWeapon weapon) {
            weapon.transform.SetParent(st.WeaponsOffset);
            weapon.SetGOActive(st.Weapons.IsEmpty());
            st.Weapons.Add(weapon);
        }
        
        public void TryShot() {
            if (st.Weapons.IsEmpty()) {
                Debug.LogError("Cannot shot without weapon");
                return;
            }
            
            st.CurrentWeapon.TryShot();
        }

        public void SwitchWeaponForward() {
            if (st.Weapons.IsEmpty()) {
                return;
            }
            
            st.CurrentWeapon.SetGOActive(false);
            st.CurrentWeaponIndex = (st.CurrentWeaponIndex + 1) % st.Weapons.Count;
            st.CurrentWeapon.SetGOActive(true);
        }

        public void SwitchWeaponBackward() {
            if (st.Weapons.IsEmpty()) {
                return;
            }

            st.CurrentWeapon.SetGOActive(false);
            
            st.CurrentWeaponIndex -= 1;
            if (st.CurrentWeaponIndex < 0) {
                st.CurrentWeaponIndex = st.Weapons.Count - 1;
            }
            
            st.CurrentWeapon.SetGOActive(true);
        }

        public override void OnInit() {
            base.OnInit();

            st.CurrentWeaponIndex = 0;
        }

        public override void OnDeinit() {
            base.OnDeinit();

            foreach (var w in st.Weapons) {
                w.DestroyUnit();
            }
            st.Weapons.Clear();
        }

        protected override void Awake() {
            base.Awake();
            
            SetFieldRef(out st.WeaponsOffset, "Weapons");
            st.Weapons = new List<PTankWeapon>();
        }

        [Serializable]
        private class Config {
        }
        
        [Serializable]
        private class State {
            public Transform WeaponsOffset;
            
            public List<PTankWeapon> Weapons;
            public int CurrentWeaponIndex;

            public PTankWeapon CurrentWeapon => Weapons[CurrentWeaponIndex];
        }
        
    }
}