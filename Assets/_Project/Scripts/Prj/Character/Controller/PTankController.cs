using System;
using PrjCore.Ext.Unity;
using PrjCore.Util.System;
using UnityEngine;

namespace Prj.Character.Controller {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PTank))]
    public class PTankController : MonoBehaviour {
        
        private State _tankControllerState = new State();

        private State st => _tankControllerState;
        
        protected void Awake() {
            st.Tank = GetComponent<PTank>();
        }

        protected void Update() {
            ProcessMovement();
            ProcessRotation();
            ProcessWeaponSwitch();
            ProcessShot();
        }

        private void ProcessMovement() {
            bool forward = Input.GetKey(KeyCode.W);
            bool backward = Input.GetKey(KeyCode.S);

            if (forward == backward) {
                st.Tank.Rigidbody2D.velocity = Vector2.zero;
            } else if (forward) {
                st.Tank.Rigidbody2D.velocity = PMathUtil.PolarToDecart(st.Tank.MovementSpeed, st.Tank.Rotation2DRad());
            } else {
                st.Tank.Rigidbody2D.velocity = -PMathUtil.PolarToDecart(st.Tank.MovementSpeed, st.Tank.Rotation2DRad());
            }
        }

        private void ProcessRotation() {
            bool left = Input.GetKey(KeyCode.A);
            bool right = Input.GetKey(KeyCode.D);

            if (left == right) {
                st.Tank.Rigidbody2D.angularVelocity = 0;
            } else if (left) {
                st.Tank.Rigidbody2D.angularVelocity = st.Tank.RotationSpeed;
            } else {
                st.Tank.Rigidbody2D.angularVelocity = -st.Tank.RotationSpeed;
            }
        }

        private void ProcessWeaponSwitch() {
            bool forward = Input.GetKeyDown(KeyCode.E);
            bool backward = Input.GetKeyDown(KeyCode.Q);
            if (forward == backward) {
                return;
            }

            if (forward) {
                st.Tank.SwitchWeaponForward();
            } else {
                st.Tank.SwitchWeaponBackward();
            }
        }

        private void ProcessShot() {
            bool shot = Input.GetKey(KeyCode.X);
            if (!shot) {
                return;
            }
            
            st.Tank.TryShot();
        }

        [Serializable]
        private class State {
            public PTank Tank;
        }
    }
}