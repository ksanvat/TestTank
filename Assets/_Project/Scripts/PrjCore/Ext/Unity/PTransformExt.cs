using PrjCore.Util.System;
using UnityEngine;

namespace PrjCore.Ext.Unity {
    public static class PTransformExt {
        public static float Rotation2DDeg(this Transform self) {
            if (self == null) {
                return 0;
            }
            
            return self.rotation.eulerAngles.z;
        }

        public static float Rotation2DRad(this Transform self) {
            return Rotation2DDeg(self).AngleToRad();
        }
        
    }
}