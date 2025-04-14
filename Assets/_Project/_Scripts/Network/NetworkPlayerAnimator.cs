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
        
        readonly int moveXHash = Animator.StringToHash("MoveX");
        readonly int moveYHash = Animator.StringToHash("MoveY");

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

            if (Input.GetKeyDown(KeyCode.Space)) {
                Animator.Play("Attack03");
            }
        }
        
        protected override bool OnIsServerAuthoritative() {
            return false;
        }
    }
}
