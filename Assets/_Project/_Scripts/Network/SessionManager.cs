using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace Antoine {
    public class SessionManager : Singleton<SessionManager> {
        ISession activeSession;

        public ISession ActiveSession {
            get => activeSession;
            set {
                activeSession = value;
                Debug.Log($"Active session: {activeSession}");
            }
        }

        const string k_authNamePropertyKey = "authName";
        const string k_usernamePropertyKey = "username";
        const string k_isReadyPropertyKey = "isReady";

        public event Action OnSessionJoined = delegate { };
        public event Action OnSessionLeft = delegate { };

        async void Start() {
            try {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
        }

        async UniTask<Dictionary<string, PlayerProperty>> GetPlayerProperties(string username) {
            var authName = await AuthenticationService.Instance.GetPlayerNameAsync();
            
            var authNameProperty = new PlayerProperty(authName, VisibilityPropertyOptions.Member);
            var usernameProperty = new PlayerProperty(username, VisibilityPropertyOptions.Member);
            var isReadyProperty = new PlayerProperty("false", VisibilityPropertyOptions.Member);
            
            return new Dictionary<string, PlayerProperty> {
                { k_authNamePropertyKey, authNameProperty },
                { k_usernamePropertyKey, usernameProperty },
                { k_isReadyPropertyKey, isReadyProperty }
            };
        }

        public async UniTaskVoid StartSessionAsHost(string username) {
            var playerProperties = await GetPlayerProperties(username);

            var options = new SessionOptions() {
                MaxPlayers = 4,
                IsLocked = false,
                IsPrivate = false,
                PlayerProperties = playerProperties
            }.WithRelayNetwork();

            ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(options);
            Debug.Log($"Session {ActiveSession.Id} created! Join code: {ActiveSession.Code}");

            OnSessionJoined.Invoke();
        }

        public async UniTaskVoid JoinSessionByCode(string sessionCode, string username) {
            var playerProperties = await GetPlayerProperties(username);

            var options = new JoinSessionOptions() {
                PlayerProperties = playerProperties
            };
            
            ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode, options);
            Debug.Log($"Session {ActiveSession.Id} joined!");
            
            OnSessionJoined.Invoke();
        }

        public async UniTaskVoid KickPlayer(string playerId) {
            if (!ActiveSession.IsHost) return;
            await ActiveSession.AsHost().RemovePlayerAsync(playerId);
        }

        public async UniTask<IList<ISessionInfo>> QuerySessions() {
            var sessionQueryOptions = new QuerySessionsOptions();
            var results = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions);
            return results.Sessions;
        }

        public async UniTaskVoid LeaveSession() {
            if (ActiveSession != null) {
                try {
                    await ActiveSession.LeaveAsync();
                }
                catch {
                    // Ignored as we are exiting the game
                }
                finally {
                    ActiveSession = null;
                }
            }
            
            OnSessionLeft.Invoke();
        }

        public async UniTaskVoid SetPlayerReady(bool isReady) {
            if (ActiveSession == null) return;

            var isReadyProperty = new PlayerProperty(isReady ? "true" : "false", VisibilityPropertyOptions.Member);
            ActiveSession.CurrentPlayer.SetProperty(k_isReadyPropertyKey, isReadyProperty);
            
            await ActiveSession.SaveCurrentPlayerDataAsync();
        }
    }
}
