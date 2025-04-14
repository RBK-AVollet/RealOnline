using UnityEngine;

namespace DSI.Utility {
    public static class AudioExtensions {
        /// <summary>
        /// Given a fraction in the range of [0, 1], convert it to a logarithmic scale (also in range [0, 1])
        /// that mimics the way we hear volume (since human perception of sound volume is logarithmic).
        /// The math here performs the following steps:
        /// - Within the Log10 function, we're adding 9 times the original fraction to 1 before taking the logarithm. 
        ///   This makes sure that the fraction is smoothly scaled to our logarithmic curve, and it fits the range [0, 1].
        /// - Takes the base-10 logarithm of the interpolated fraction.
        /// - Divides the result by Log10(10) simply to normalize the result and ensure it fits within the [0, 1] range, 
        ///   since as we know the input to Log10 function can vary between 1 and 10 after the interpolation.
        ///
        /// This method is useful for improved fading effects between Audio Clips.
        /// | Credits: <see href="https://www.youtube.com/@git-amend">git-amend</see>
        /// </summary>
        public static float ToLogarithmicFraction(this float fraction) {
            return Mathf.Log10(1 + 9 * fraction) / Mathf.Log10(10);
        }
    }
}