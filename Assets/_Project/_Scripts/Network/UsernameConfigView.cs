using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Antoine {
    public class UsernameConfigView : MonoBehaviour {
        [SerializeField] UsernameConfig config;
        [Space]
        [SerializeField] TMP_InputField usernameIF;
        [SerializeField] Button confirmButton;
        [SerializeField] Image borderImg;

        const int k_minUsernameLength = 3;
        const int k_maxUsernameLength = 24;

        Tween flashTween;
        
        void OnEnable() => RegisterEvents();
        void OnDisable() => DeregisterEvents();

        void RegisterEvents() {
            confirmButton.onClick.AddListener(OnConfirm);
        }

        void DeregisterEvents() {
            confirmButton.onClick.RemoveListener(OnConfirm);
        }

        void OnConfirm() {
            string username = usernameIF.text;
            
            if(username.Length < k_minUsernameLength || username.Length > k_maxUsernameLength) {
                FlashBorderRed();
                return; 
            }

            config.SetUsername(username);
        }

        void FlashBorderRed() {
            if (flashTween != null) {
                flashTween.Kill();
                flashTween = null;
            }
            
            borderImg.color = Color.red;
            flashTween = borderImg.DOColor(Color.white, 2f);
        }
    }
}