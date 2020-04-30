using System;
using PrjCore.Character;
using PrjCore.Ext.Unity;
using PrjCore.Util.System;
using UnityEngine;

namespace Prj.Character.Controller {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PEnemy))]
    public class PEnemyController : MonoBehaviour {

        private State _enemyControllerState = new State();

        private State st => _enemyControllerState;

        protected void Update() {
            var tank = st.Unit.OpponentGetter();
            
            ProcessForward(tank);
            ProcessRotation(tank);
        }
        
        protected void Awake() {
            st.Unit = GetComponent<PEnemy>();
        }

        private void ProcessForward(PBaseCharacter tank) {
            var movementDeltaPerSec = PMathUtil.PolarToDecart(st.Unit.MovementSpeed, st.Unit.Rotation2DRad()); 
            Vector3 movementDelta = movementDeltaPerSec * Time.fixedDeltaTime;

            var positionDelta = st.Unit.transform.position - tank.transform.position;

            float dist = positionDelta.sqrMagnitude;
            float distIfForward = (positionDelta + movementDelta).sqrMagnitude;
            float distIfBackward = (positionDelta - movementDelta).sqrMagnitude;

            if (dist < distIfForward && dist < distIfBackward) {
                st.Unit.Rigidbody2D.velocity = Vector2.zero;
            } else {
                st.Unit.Rigidbody2D.velocity = distIfForward < distIfBackward ? movementDeltaPerSec : -movementDeltaPerSec;
            }
        }

        private void ProcessRotation(PBaseCharacter tank) {
            var positionDelta = tank.transform.position - st.Unit.transform.position;

            int rotationDirection = PMathUtil.CalcRotationRadDirection(
                st.Unit.Rotation2DRad(),
                Mathf.Atan2(positionDelta.y, positionDelta.x),
                1f.AngleToRad()
            );

            st.Unit.Rigidbody2D.angularVelocity = rotationDirection * st.Unit.RotationSpeed;
        }

        [Serializable]
        private class State {
            public PEnemy Unit;
        }
    }
}