using PrjCore.Unit;
using PrjCore.Util.System;
using UnityEngine;

namespace PrjCore.Ext.Unity {
    public static class PMonoBehaviourExt {

        public static int GetGOInstanceID(this MonoBehaviour self) {
            return self.gameObject.GetGOInstanceID();
        }

        public static T CreateUnit<T>(this T self) where T : PUnit {
            return PUnitManager.CreateUnit(self);
        }

        public static void DestroyUnit<T>(this T self) where T : PUnit {
            PUnitManager.DestroyUnit(self);
        }

        public static void SetGOActive<T>(this T self, bool value) where T : MonoBehaviour {
            if (self == null) {
                return;
            }
            
            self.gameObject.SetActive(value);
        }

        public static float Rotation2DDeg<T>(this T self) where T : PUnit {
            if (self == null) {
                return 0;
            }

            return self.transform.Rotation2DDeg();
        }

        public static float Rotation2DRad<T>(this T self) where T : PUnit {
            return Rotation2DDeg(self).AngleToRad();
        }
        
    }
}