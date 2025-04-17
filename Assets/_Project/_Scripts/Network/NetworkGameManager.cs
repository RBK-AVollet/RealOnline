using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Antoine {
    public class NetworkGameManager : NetworkSingleton<NetworkGameManager> {
        [SerializeField] int gameDuration = 300;
        [SerializeField] NetworkButtons networkButtons;
        SessionManager sessionManager;

        public NetworkVariable<float> gameCountdown = new NetworkVariable<float>(
            0f, 
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        ulong seekerId;
        bool gameStarted = false;

        void Start() {
            sessionManager = SessionManager.Instance;
        }
        
        public void StartGame() {
            if (!IsServer) return;
            if (gameStarted) return;

            StartCoroutine(StartGameRoutine());
        }

        IEnumerator StartGameRoutine() {
            gameStarted = true;
            gameCountdown.Value = gameDuration;

            HideLobbyHUDRpc();

            seekerId = PickRandomSeeker(out string seekerName);

            ShowSeekerOnScreenRpc(seekerName);

            yield return new WaitForSeconds(7f);
            
            // Release non seeker players
            foreach (var player in NetworkManager.Singleton.ConnectedClientsList) {
                if (player.ClientId == seekerId) {
                    HideSeekerViewRpc(true, RpcTarget.Single(seekerId, RpcTargetUse.Temp));
                    continue;
                }

                EnableCharacterRpc(RpcTarget.Single(player.ClientId, RpcTargetUse.Temp));
            }
            
            gameStarted = true;
            EnableCountdownDisplayRpc(true);

            yield return new WaitForSeconds(30f);
            
            HideSeekerViewRpc(false, RpcTarget.Single(seekerId, RpcTargetUse.Temp));
            
            // Release seeker player
            EnableCharacterRpc(RpcTarget.Single(seekerId, RpcTargetUse.Temp));
            
        }

        void Update() {
            if (!IsServer) return;
            if (!gameStarted) return;

            gameCountdown.Value -= gameDuration;

            if (gameCountdown.Value <= 0) {
                HiderVictory();
            }
        }

        void HiderVictory() {
            gameStarted = false;
            EnableCountdownDisplayRpc(false);
        }

        void SeekerVictory() {
            gameStarted = false;
            EnableCountdownDisplayRpc(false);
        }

        ulong PickRandomSeeker(out string seekerName) {
            seekerName = "";
            
            if (sessionManager.ActiveSession == null) {
                Debug.LogError("No session but game is starting !");
                return 0;
            }
            
            if (sessionManager.ActiveSession.PlayerCount == 0) {
                Debug.LogError("No players in the session but game is starting !");
                return 0;
            }

            var players = NetworkManager.Singleton.ConnectedClientsIds;
            var id = players[Random.Range(0, players.Count)];
            
            const string k_usernamePropertyKey = "username";
            var seekerSession = sessionManager.ActiveSession.Players[(int)id];
            if (!seekerSession.Properties.TryGetValue(k_usernamePropertyKey, out var seekerNameProperty)) {
                Debug.LogError("Couldn't fetch seeker's username !");
                return 0;
            }
            
            seekerName = seekerNameProperty.Value;
            
            Debug.Log($"Seeker is {id}_{seekerName}");

            return id;
        }

        [Rpc(SendTo.NotServer)]
        void HideLobbyHUDRpc() {
            if (networkButtons == null) return;
            
            networkButtons.HideLobbyUI();
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        void ShowSeekerOnScreenRpc(string seekerName) {
            networkButtons.DisplaySeekerOnScreen(seekerName);
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        void EnableCharacterRpc(RpcParams rpcParams) {
            var playerCharacter = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (playerCharacter == null) return;

            var playerBehaviour = playerCharacter.GetComponent<NetworkPlayer>();
            if (playerBehaviour == null) return;

            playerBehaviour.InitializeCharacter();
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        void HideSeekerViewRpc(bool enabled, RpcParams rpcParams) {
            networkButtons.HidePlayerView(enabled);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void EnableCountdownDisplayRpc(bool enabled) {
            networkButtons.EnableCountdownDisplayRpc(enabled);
        }
    }
}
