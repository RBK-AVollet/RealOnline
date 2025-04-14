using System.Collections.Generic;

namespace DSI.Utility {
    public static class QueueExtensions {
        public static void Shuffle<T>(this Queue<T> queue) {
            List<T> list = new List<T>(queue);
            list.Shuffle();
            queue.Clear();
            list.ForEach(x => queue.Enqueue(x));
        }

        public static bool IsEmpty<T>(this Queue<T> queue) {
            return queue.Count <= 0;
        }
    }
}