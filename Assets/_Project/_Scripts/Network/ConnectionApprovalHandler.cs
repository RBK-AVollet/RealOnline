using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Antoine {
    [RequireComponent(typeof(NetworkManager))]
    public class ConnectionApprovalHandler : MonoBehaviour {
        [SerializeField] List<NetworkObject> playerAlternatePrefabs;
        [SerializeField] List<Transform> spawnPoints;
        NetworkManager networkManager;

        void Awake() {
            networkManager = GetComponent<NetworkManager>();
        }

        void OnEnable() {
            networkManager.ConnectionApprovalCallback += ConnectionApprovalCheck;
            networkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
        }

        void OnDisable() {
            networkManager.ConnectionApprovalCallback -= ConnectionApprovalCheck;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }
        
        void ConnectionApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
            int clientCount = networkManager.ConnectedClientsList.Count;
            
            if (clientCount >= playerAlternatePrefabs.Count) {
                response.Approved = false;
                response.Reason = "Server is full";
            } else {
                response.Approved = true;
                response.CreatePlayerObject = true;
                response.PlayerPrefabHash = playerAlternatePrefabs[clientCount].PrefabIdHash;
                response.Position = spawnPoints[clientCount].position;
                response.Rotation = spawnPoints[clientCount].rotation;
            }
            
            response.Pending = false;
        }
        
        void OnClientDisconnectCallback(ulong clientId) {
            if(networkManager.IsServer || networkManager.DisconnectReason == string.Empty) return;
            
            Debug.Log($"Client {clientId} disconnected: {networkManager.DisconnectReason}");
        }
    }
}
