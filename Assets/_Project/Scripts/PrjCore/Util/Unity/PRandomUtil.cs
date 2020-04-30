using System;
using System.Collections.Generic;
using System.Linq;

namespace Prj {
    public static class PRandomUtil {
        private static Random random = new Random();

        public static void SetSeed(int seed) {
            random = new Random(seed);
        }
        
        // [min, max]
        public static int IntIn(int min, int max) {
            return random.Next(min, max + 1);
        }

        // [min, max)
        public static int IntEx(int min, int max) {
            return random.Next(min, max);
        }

        public static uint UintEx(uint min, uint max) {
            return min + (uint) (random.NextDouble() * (max - min));
        }

        public static float Float(float min, float max) {
            return (float) (min + random.NextDouble() * (max - min));
        }

        public static bool Bool() {
            return IntIn(0, 1) == 0;
        }
        
        public static float Random01() {
            return Float(0f, 1f);
        }

        public static U SelectByWeight<T, U>(List<T> objects, Func<T, float> weightSelector, Func<T, U> objectSelector, U objectDefault=default(U)) {
            float weight = Float(0, objects.Sum(weightSelector));
            foreach (var obj in objects) {
                weight -= weightSelector(obj);
                if (weight <= 0) {
                    return objectSelector(obj);
                }
            }

            return objectDefault;
        }

        public static T Select<T>(List<T> elems, T elemDefault=default) {
            return elems.Count > 0 ? elems[IntEx(0, elems.Count)] : elemDefault;
        }

        public static T SelectAndRemove<T>(List<T> elems, T elemDefault = default) {
            if (elems.Count <= 0) {
                return elemDefault;
            }
            
            int index = IntEx(0, elems.Count);
            
            var e = elems[index];
            elems.RemoveAt(index);
            
            return e;
        }

        public static T Select<T>(params T[] elems) {
            return elems[IntEx(0, elems.Length)];
        }

        public static int SelectIndex<T>(List<T> elems) {
            return IntEx(0, elems.Count);
        }
        
    }
}