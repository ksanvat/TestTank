using System;
using UnityEngine;

namespace PrjCore.Util.System {
    public static class PMathUtil {

        public const float PI = Mathf.PI;
        public const float TwoPI = Mathf.PI * 2f;
        public const float Epsilon = 0.0001f;
        
        public static Vector2 PolarToDecart(float radius, float angle) {
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        public static int CalcRotationRadDirection(float source, float target, float epsilon=Epsilon) {
            source %= TwoPI;
            if (source < 0) {
                source += TwoPI;
            }
            
            target %= TwoPI;
            if (target < 0) {
                target += TwoPI;
            }
            
            if (Math.Abs(source - target) <= epsilon) {
                return 0;
            }
            
            if (source < target) {
                float diff = target - source;
                return diff < PI ? 1 : -1;
            } else {
                float diff = source - target;
                return diff > PI ? 1 : -1;
            }
        }

        public static void AssureMinMax(ref Vector2 min, ref Vector2 max) {
            if (min.x > max.x) {
                PObjectUtil.Swap(ref min.x, ref max.x);
            }
            if (min.y > max.y) {
                PObjectUtil.Swap(ref min.y, ref max.y);
            }
        }
        
    }
}