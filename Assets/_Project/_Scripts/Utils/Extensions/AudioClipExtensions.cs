using UnityEngine;

namespace DSI.Utility {
    public static class AudioClipExtensions {
        /// <summary>
        /// Returns if an audio clip last more than the given length
        /// </summary>
        /// <param name="clip">clip to check</param>
        /// <param name="length">minimum length</param>
        public static bool LastMoreThan(this AudioClip clip, float length) {
            return clip.length >= length;
        }
    }
}