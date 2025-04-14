using UnityEngine;
using UnityEngine.SceneManagement;

namespace Antoine {
    public class NetworkMapLoader : MonoBehaviour {
        const string k_map = "S_HalloweenMap";
        
        void Start() {
            SceneManager.LoadSceneAsync(k_map, LoadSceneMode.Additive);
        }
    }
}
