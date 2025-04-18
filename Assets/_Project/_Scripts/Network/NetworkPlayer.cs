using Antoine.Systems.Timers;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Antoine {
    public class NetworkPlayer : NetworkBehaviour {
        [Header("Settings")]
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float sprintMultiplier = 2f;
        [SerializeField] float rotationSpeed = 10f;
        [SerializeField] float rotateThreshold = 0.8f;
        [SerializeField] float diveCooldown = 1f;
        
        [Header("Dependencies")]
        [SerializeField] InputReader input;
        [SerializeField] Camera cam;
        [SerializeField] CinemachineBrain cinemachineBrain;
        [SerializeField] CinemachineCamera cinemachineCamera;
        [SerializeField] AudioListener audioListener;
        NetworkPlayerAnimator playerAnimator;
        CountdownTimer diveTimer;
        bool inGround;
        bool isSeeker;
        Rigidbody rb;
        
        Vector2 moveInput;

        public Vector3 Movement { get; private set; }
        public Vector3 NonZeroMovement { get; private set; }
        public Vector3 NormalizedMovement { get; private set; }
        public Vector3 AnimationMovement { get; private set; }
        
        public bool IsSprinting { get; private set; }
        
        public override void OnNetworkSpawn() {
            cam.enabled = IsOwner;
            cinemachineCamera.enabled = IsOwner;
            cinemachineBrain.enabled = IsOwner;
            audioListener.enabled = IsOwner;
            
            var lobbyCam = Camera.main;
            if (lobbyCam != null) {
                lobbyCam.gameObject.SetActive(false);
            }
            
            if (!IsOwner) return;

            rb = GetComponent<Rigidbody>();
            playerAnimator = GetComponent<NetworkPlayerAnimator>();

            diveTimer = new CountdownTimer(diveCooldown);
            
            input.OnSprintStart += OnSprintStart;
            input.OnSprintEnd += OnSprintEnd;
            input.OnDiveEvent += OnDive;
        }

        public void InitializeCharacter(bool isSeeker) {
            if (!IsOwner) return;
            
            this.isSeeker = isSeeker;
            cinemachineCamera.Priority.Value = 100;
            input.EnablePlayerInputs();
        }

        public override void OnNetworkDespawn() {
            if (!IsOwner) return;
            input.OnSprintStart -= OnSprintStart;
            input.OnSprintEnd -= OnSprintEnd;
        }

        void FixedUpdate() {
            if (!IsOwner) return;
            if (inGround) return;
            
            moveInput = input.Move;
            Vector3 forward = cam.transform.forward;
            forward.y = 0;
            forward.Normalize();
            
            Vector3 right = cam.transform.right;
            right.y = 0;
            right.Normalize();
            
            Movement = (right * moveInput.x + forward * moveInput.y).normalized * (IsSprinting ? 1 : 0.5f);
            AnimationMovement = new Vector3(moveInput.x, 0f, moveInput.y) * (IsSprinting ? 1 : 0.5f);
            
            if (moveInput == Vector2.zero) return;

            NonZeroMovement = Movement;
            NormalizedMovement = NonZeroMovement.normalized;
            
            HandleMovement();
            if(moveInput.y > rotateThreshold) HandleRotation();
        }

        void HandleMovement() {
            float speed = moveSpeed * (IsSprinting ? sprintMultiplier : 1f);
            rb.MovePosition(transform.position + NormalizedMovement * (speed * Time.deltaTime));
        }

        void HandleRotation() {
            Quaternion targetRotation = Quaternion.LookRotation(NormalizedMovement);
            Quaternion rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            rb.MoveRotation(rotation);
        }
        
        void OnSprintStart() {
            IsSprinting = true;
        }
        
        void OnSprintEnd() {
            IsSprinting = false;
        }
        
        void OnDive() {
            if (!IsOwner) return;
            if (isSeeker) return;
            if (diveTimer.IsRunning) return;
            
            inGround ^= true;
            
            if (inGround) {
                playerAnimator.PlayDive();
            } else {
                playerAnimator.PlayDiveBack();
            }
            
            diveTimer.Start();
        }
    }
}
