using UnityEngine;

namespace DSI.Utility {
    public static class Utils {
        public static float Remap(float iMin, float iMax, float oMin, float oMax, float n) {
            float t = Mathf.InverseLerp(iMin, iMax, n);
            return Mathf.Lerp(oMin, oMax, t);
        }
    }
}