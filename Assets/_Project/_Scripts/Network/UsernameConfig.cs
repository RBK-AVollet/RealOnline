using Antoine.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Antoine {
    public class UsernameConfig : PersistentSingleton<UsernameConfig> {
        public string Username { get; private set; }

        protected override void Awake() {
            base.Awake();
            Username = "Anonymous";
        }

        public async void SetUsername(string username) {
            Username = username;
            Debug.Log($"Username set to: {Username}");

            await SceneManager.LoadSceneAsync("S_MainMenu");
        }
    }
}
