using DG.Tweening;
using Antoine.Systems.Timers;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Antoine
{
    public class NetworkButtons : MonoBehaviour {
        [Header("Settings")] 
        [SerializeField] float btnSpawnDuration = 0.4f;
        [SerializeField] float btnSpawnDelay = 0.15f;
        [SerializeField] float joinCodeDuration = 0.2f;
        
        [Header("References")]
        [SerializeField] Button networkBtn;
        [SerializeField] Button createBtn;
        [SerializeField] Button joinBtn;
        [SerializeField] Button leaveBtn;
        [SerializeField] RectTransform joinCodeRect;
        [SerializeField] TMP_InputField joinCodeIF;
        [SerializeField] Button joinCodeButton;
        [Space]
        [SerializeField] TMP_Text joinCodeText;
        [SerializeField] Button isReadyBtn;
        [SerializeField] Button startGameBtn;
        [SerializeField] Sprite notReadySprite;
        [SerializeField] Sprite readySprite;
        [SerializeField] TextMeshProUGUI[] usernameTextes;
        [SerializeField] TextMeshProUGUI screenDisplayText;
        [SerializeField] CanvasGroup screenDisplayCanvasGroup;
        [SerializeField] Image coverPlayerView;
        [SerializeField] TextMeshProUGUI countdownText;
        RectTransform networkRect, createRect, joinRect, leaveRect;
        CanvasGroup canvasGroup;
        CountdownTimer readyTimerCooldown;
        
        SessionManager sessionManager;
        NetworkGameManager networkGameManager;
        UsernameConfig usernameConfig;
        
        Vector3 createStartPos;
        Vector3 joinStartPos;
        Vector3 leaveStartPos;
        bool networkButtonsEnabled;
        bool joinCodeEnabled;
        bool isReady;
        bool displayCountdown;
        
        Sequence networkButtonsSequence;
        Sequence joinCodeSequence;
        Sequence screenDisplaySequence;
        Tween lobbyFadeOutTween, coverPlayerViewTween;
        
        void Awake() {
            canvasGroup = GetComponent<CanvasGroup>();
            networkRect = networkBtn.GetComponent<RectTransform>();
            createRect = createBtn.GetComponent<RectTransform>();
            joinRect = joinBtn.GetComponent<RectTransform>();
            leaveRect = leaveBtn.GetComponent<RectTransform>();
            
            createStartPos = createRect.anchoredPosition;
            joinStartPos = joinRect.anchoredPosition;
            leaveStartPos = leaveRect.anchoredPosition;
            
            createRect.localScale = Vector3.zero;
            joinRect.localScale = Vector3.zero;
            leaveRect.localScale = Vector3.zero;
            joinCodeRect.localScale = Vector3.zero;
            
            createRect.anchoredPosition = networkRect.anchoredPosition;
            joinRect.anchoredPosition = networkRect.anchoredPosition;
            leaveRect.anchoredPosition = networkRect.anchoredPosition;

            networkButtonsEnabled = false;
            joinCodeEnabled = false;
            isReady = false;
            displayCountdown = false;
            
            readyTimerCooldown = new CountdownTimer(1f);
            
            EnableLeaveButton(false);
        }

        void OnEnable() {
            networkBtn.onClick.AddListener(ToggleNetworkButtons);
        }

        void OnDisable() {
            networkBtn.onClick.RemoveListener(ToggleNetworkButtons);
        }

        void Start() {
            sessionManager = SessionManager.Instance;
            usernameConfig = UsernameConfig.Instance;
            networkGameManager = NetworkGameManager.Instance;

            sessionManager.OnSessionJoined += OnSessionJoined;
            sessionManager.OnSessionLeft += OnSessionLeft;
        }

        void Update() {
            if (!displayCountdown) return;

            int gameCountdown = Mathf.FloorToInt(networkGameManager.gameCountdown.Value);
            countdownText.SetText(gameCountdown.ToString());
        }

        void OnSessionJoined() {
            EnableCreateButton(false);
            EnableJoinButton(false);
            EnableLeaveButton(true);
            
            joinCodeIF.text = "";
            joinCodeText.SetText($"Lobby Code: {sessionManager.ActiveSession.Code}");
            joinCodeText.gameObject.SetActive(true);
            
            isReady = false;
            isReadyBtn.image.sprite = notReadySprite;
            isReadyBtn.interactable = true;
            isReadyBtn.gameObject.SetActive(true);
            isReadyBtn.onClick.AddListener(ToggleReady);
            
            // Show all players already in the lobby
            UpdatePlayerNames();
            sessionManager.ActiveSession.PlayerJoined += OnPlayerJoined;
            sessionManager.ActiveSession.PlayerLeft += OnPlayerLeft;
            sessionManager.ActiveSession.PlayerPropertiesChanged += UpdatePlayerNames;
        }

        void OnPlayerJoined(string clientId) {
            UpdatePlayerNames();
        }

        void OnPlayerLeft(string clientId) {
            UpdatePlayerNames();
        }
        
        void UpdatePlayerNames() {
            const string k_usernamePropertyKey = "username";
            const string k_isReadyPropertyKey = "isReady";
            
            if(sessionManager.ActiveSession == null) return;

            bool allReady = true;
            
            for (int i = 0; i < sessionManager.ActiveSession.Players.Count; i++) {
                var plr = sessionManager.ActiveSession.Players[i];
                if (plr == null) continue;
                
                plr.Properties.TryGetValue(k_usernamePropertyKey, out var usernameProperty);
                plr.Properties.TryGetValue(k_isReadyPropertyKey, out var isReadyProperty);
                
                if (usernameProperty == null || isReadyProperty == null) continue;
                
                string username = usernameProperty.Value;
                bool ready = bool.Parse(isReadyProperty.Value);

                if (!ready) allReady = false;
                
                usernameTextes[i].SetText(username);
                usernameTextes[i].color = ready ? Color.green : Color.white;
            }

            // All players are ready, show the button only for the host to start the game
            if (sessionManager.ActiveSession.PlayerCount >= 2
                && NetworkManager.Singleton.IsHost
                && allReady) {
                startGameBtn.gameObject.SetActive(true);
                startGameBtn.onClick.AddListener(StartGame);
            } else {
                startGameBtn.gameObject.SetActive(false);
                startGameBtn.onClick.RemoveListener(StartGame);
            }
        }

        void StartGame() {
            if(!NetworkManager.Singleton.IsHost) return;

            startGameBtn.gameObject.SetActive(false);
            startGameBtn.onClick.RemoveListener(StartGame);
            HideLobbyUI();
            
            NetworkGameManager.Instance.StartGame();
        }

        public void HideLobbyUI() {
            joinCodeText.gameObject.SetActive(false);
            isReadyBtn.gameObject.SetActive(false);
            isReadyBtn.interactable = false;

            lobbyFadeOutTween = canvasGroup.DOFade(0f, 0.4f).SetEase(Ease.Linear).OnComplete(EraseUsernames);

            sessionManager.SetPlayerReady(false).Forget();
        }

        void EraseUsernames() {
            foreach (var username in usernameTextes) {
                username.SetText("");
            }
        }

        void OnSessionLeft() {
            EnableCreateButton(true);
            EnableJoinButton(true);
            EnableLeaveButton(false);
            
            joinCodeText.gameObject.SetActive(false);
            isReadyBtn.gameObject.SetActive(false);
            isReadyBtn.interactable = false;
        }

        void StartHost() {
            string username = usernameConfig.Username;

            sessionManager.StartSessionAsHost(username).Forget();
        }

        void JoinLobby() {
            string code = joinCodeIF.text;  
            string username = usernameConfig.Username;
            
            sessionManager.JoinSessionByCode(code, username).Forget();

            HideJoinCode();
        }

        Tween ButtonScaleTween(RectTransform btnRect, Vector3 targetScale, bool enabled) {
            return btnRect.DOScale(targetScale, btnSpawnDuration)
                .SetEase(enabled ? Ease.OutBack : Ease.InBack)
                .SetUpdate(true);
        }

        Tween ButtonMoveTween(RectTransform btnRect, Vector3 targetPos, bool enabled) {
            return btnRect.DOAnchorPos(targetPos, btnSpawnDuration)
                .SetEase(enabled ? Ease.OutQuint : Ease.InQuint)
                .SetUpdate(true);
        }
        
        void ToggleNetworkButtons() {
            networkButtonsEnabled ^= true;
            
            if (networkButtonsSequence != null) {
                networkButtonsSequence.Kill();
                networkButtonsSequence = null;
            }

            HideJoinCode();

            networkButtonsSequence = DOTween.Sequence();

            if (networkButtonsEnabled) {
                networkButtonsSequence
                    .Insert(0f, ButtonMoveTween(createRect, networkButtonsEnabled ? createStartPos : networkRect.anchoredPosition, true))
                    .Join(ButtonScaleTween(createRect, networkButtonsEnabled ? Vector3.one : Vector3.zero, true))
                    .Insert(btnSpawnDelay, ButtonMoveTween(joinRect, networkButtonsEnabled ? joinStartPos : networkRect.anchoredPosition, true))
                    .Join(ButtonScaleTween(joinRect, networkButtonsEnabled ? Vector3.one : Vector3.zero, true))
                    .Insert(btnSpawnDelay * 2, ButtonMoveTween(leaveRect, networkButtonsEnabled ? leaveStartPos : networkRect.anchoredPosition, true))
                    .Join(ButtonScaleTween(leaveRect, networkButtonsEnabled ? Vector3.one : Vector3.zero, true));
            }
            else {
                networkButtonsSequence
                    .Insert(0f, ButtonMoveTween(leaveRect, networkButtonsEnabled ? leaveStartPos : networkRect.anchoredPosition, false))
                    .Join(ButtonScaleTween(leaveRect, networkButtonsEnabled ? Vector3.one : Vector3.zero, false))
                    .Insert(btnSpawnDelay, ButtonMoveTween(joinRect, networkButtonsEnabled ? joinStartPos : networkRect.anchoredPosition, false))
                    .Join(ButtonScaleTween(joinRect, networkButtonsEnabled ? Vector3.one : Vector3.zero, false))
                    .Insert(btnSpawnDelay * 2, ButtonMoveTween(createRect, networkButtonsEnabled ? createStartPos : networkRect.anchoredPosition, false))
                    .Join(ButtonScaleTween(createRect, networkButtonsEnabled ? Vector3.one : Vector3.zero, false));
            }

            if (networkButtonsEnabled) {
                createBtn.onClick.AddListener(StartHost);
                joinBtn.onClick.AddListener(ToggleJoinCode);
            } else {
                createBtn.onClick.RemoveListener(StartHost);
                joinBtn.onClick.RemoveListener(ToggleJoinCode);  
            }
        }

        void ToggleJoinCode() {
            joinCodeEnabled ^= true;

            if (joinCodeEnabled) {
                ShowJoinCode();
            }
            else {
                HideJoinCode();
            }
        }
        
        void ShowJoinCode() {
            if (joinCodeSequence != null) {
                joinCodeSequence.Kill();
                joinCodeSequence = null;
            }
            
            joinCodeEnabled = true;

            joinCodeSequence = DOTween.Sequence()
                .Append(joinCodeRect.DOScaleX(1f, joinCodeDuration).SetEase(Ease.InQuad))
                .Join(joinCodeRect.DOScaleY(1f, joinCodeDuration).SetEase(Ease. OutQuad));
            
            joinCodeButton.onClick.AddListener(JoinLobby);
        }
        
        void HideJoinCode() {
            if (joinCodeSequence != null) {
                joinCodeSequence.Kill();
                joinCodeSequence = null;
            }
            
            joinCodeEnabled = false;

            joinCodeSequence = DOTween.Sequence()
                .Append(joinCodeRect.DOScaleX(0f, joinCodeDuration).SetEase(Ease.OutQuad))
                .Join(joinCodeRect.DOScaleY(0f, joinCodeDuration).SetEase(Ease.InQuad));
            
            joinCodeButton.onClick.RemoveListener(JoinLobby);
        }

        void EnableCreateButton(bool enabled) {
            createBtn.image.color = enabled ? Color.white : Color.gray;
            createBtn.interactable = enabled;
        }
        
        void EnableJoinButton(bool enabled) {
            joinBtn.image.color = enabled ? Color.white : Color.gray;
            joinBtn.interactable = enabled;
        }
        
        void EnableLeaveButton(bool enabled) {
            leaveBtn.image.color = enabled ? Color.white : Color.gray;
            leaveBtn.interactable = enabled;
            
            if(enabled)
                leaveBtn.onClick.AddListener(Leave);
            else
                leaveBtn.onClick.RemoveListener(Leave);
        }

        void Leave() {
            sessionManager.LeaveSession().Forget();
        }
        
        void ToggleReady() {
            if(readyTimerCooldown.IsRunning) return;
            
            isReady ^= true;
            isReadyBtn.image.sprite = isReady ? readySprite : notReadySprite;
            sessionManager.SetPlayerReady(isReady).Forget();
            readyTimerCooldown.Start();
        }

        public void DisplaySeekerOnScreen(string username) => DisplayOnScreen($"SEEKER:\n{username}", Color.red);

        public void DisplayOnScreen(string text, Color color) {
            if (screenDisplaySequence != null) {
                screenDisplaySequence.Kill();
                screenDisplaySequence = null;
            }
            
            screenDisplayCanvasGroup.alpha = 0f;
            
            screenDisplayText.SetText(text);
            screenDisplayText.color = color;

            screenDisplaySequence = DOTween.Sequence()
                .Append(screenDisplayCanvasGroup.DOFade(1f, 1f).SetEase(Ease.InQuad))
                .AppendInterval(5f)
                .Append(screenDisplayCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.OutQuad));
        }

        public void HidePlayerView(bool enabled) {
            if (coverPlayerViewTween != null) {
                coverPlayerViewTween.Kill();
                coverPlayerViewTween = null;
            }
            
            coverPlayerViewTween = coverPlayerView.DOFade(enabled ? 1f : 0f, 1f)
                .SetEase(enabled ? Ease.OutQuad : Ease.InQuad);
        }

        public void EnableCountdownDisplayRpc(bool enabled) {
            displayCountdown = enabled;
            countdownText.gameObject.SetActive(enabled);
        }
    }
}
