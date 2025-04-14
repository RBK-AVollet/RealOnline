using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Vector3 = UnityEngine.Vector3;

namespace Antoine {
    public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
        [SerializeField] float hoverScaleFactor = 1.2f;
        [SerializeField] float clickScaleFactor = 0.8f;
        Tween scaleTween;
        
        public void OnPointerEnter(PointerEventData eventData) {
            StopCurrentTween();

            scaleTween = transform.DOScale(Vector3.one * hoverScaleFactor, 0.2f).SetEase(Ease.OutBack);
        }

        public void OnPointerExit(PointerEventData eventData) {
            StopCurrentTween();

            scaleTween = transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutSine);
        }

        public void OnPointerClick(PointerEventData eventData) {
            StopCurrentTween();

            transform.localScale = Vector3.one * hoverScaleFactor;
            scaleTween = transform.DOScale(Vector3.one * clickScaleFactor, 0.1f).SetLoops(2, LoopType.Yoyo);
        }

        void StopCurrentTween() {
            if (scaleTween != null) {
                scaleTween.Kill();
                scaleTween = null;   
            }
        }
    }
}
