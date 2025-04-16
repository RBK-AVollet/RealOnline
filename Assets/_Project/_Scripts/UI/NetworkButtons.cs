using DG.Tweening;
using TMPro;
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
        [SerializeField] Sprite notReadySprite;
        [SerializeField] Sprite readySprite;
        RectTransform networkRect, createRect, joinRect, leaveRect;

        SessionManager sessionManager;
        UsernameConfig usernameConfig;
        
        Vector3 createStartPos;
        Vector3 joinStartPos;
        Vector3 leaveStartPos;
        bool networkButtonsEnabled;
        bool joinCodeEnabled;
        bool isReady;
        
        Sequence networkButtonsSequence;
        Sequence joinCodeSequence;
        
        void Awake() {
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

            sessionManager.OnSessionJoined += OnSessionJoined;
            sessionManager.OnSessionLeft += OnSessionLeft;
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
            isReadyBtn.gameObject.SetActive(true); 
            isReadyBtn.onClick.AddListener(ToggleReady);
            Debug.Log("Registered");
        }

        void OnSessionLeft() {
            EnableCreateButton(true);
            EnableJoinButton(true);
            EnableLeaveButton(false);
            
            joinCodeText.gameObject.SetActive(false);
            
            isReadyBtn.onClick.RemoveListener(ToggleReady);
            isReadyBtn.gameObject.SetActive(false);
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
        }
        
        void ToggleReady() {
            Debug.Log("CLicked");
            isReady ^= true;
            isReadyBtn.image.sprite = isReady ? readySprite : notReadySprite;
            sessionManager.SetPlayerReady(isReady).Forget();
        }
    }
}
