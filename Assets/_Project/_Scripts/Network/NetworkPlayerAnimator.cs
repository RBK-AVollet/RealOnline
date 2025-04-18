using Unity.Netcode.Components;
using UnityEngine;

namespace Antoine {
    public class NetworkPlayerAnimator : NetworkAnimator {
        [SerializeField] float smoothTime = 0.1f;
        NetworkPlayer player;
        float moveX;
        float moveY;
        float moveXVel;
        float moveYVel;
        
        readonly int moveXHash = UnityEngine.Animator.StringToHash("MoveX");
        readonly int moveYHash = UnityEngine.Animator.StringToHash("MoveY");
        readonly int diveHash = UnityEngine.Animator.StringToHash("Dive");
        readonly int diveBackHash = UnityEngine.Animator.StringToHash("DiveBack");

        void Start() {
            if (!IsOwner) return;
            
            player = GetComponent<NetworkPlayer>();
        }

        void Update() {
            if (!IsOwner) return;
            
            Vector3 movement = player.AnimationMovement;
            moveX = Mathf.SmoothDamp(moveX, movement.x, ref moveXVel, smoothTime);
            moveY = Mathf.SmoothDamp(moveY, movement.z, ref moveYVel, smoothTime);
            
            Animator.SetFloat(moveXHash, moveX);
            Animator.SetFloat(moveYHash, moveY);
        }

        public void PlayDive() {
            if (!IsOwner) return;
            if (Animator == null) return;
            
            Animator.SetTrigger(diveHash);
        }

        public void PlayDiveBack() {
            if (!IsOwner) return;
            if (Animator == null) return;
            
            Animator.SetTrigger(diveBackHash);
        }
        
        protected override bool OnIsServerAuthoritative() {
            return false;
        }
    }
}
