using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Antoine {
    [CreateAssetMenu(menuName = "Antoine/Input Reader")]
    public class InputReader : ScriptableObject, PlayerInputActions.IPlayerActions {
        PlayerInputActions inputActions;
        
        public Vector2 Move => inputActions.Player.Move.ReadValue<Vector2>();

        public event Action OnSprintStart = delegate { };
        public event Action OnSprintEnd = delegate { };
        public event Action OnDiveEvent = delegate { };
        public event Action OnAttackEvent = delegate { };

        void OnEnable() {
            if (inputActions == null) {
                inputActions = new PlayerInputActions();
                inputActions.Player.SetCallbacks(this);
            }
        }

        public void EnablePlayerInputs() {
            inputActions.Enable();
        }

        public void DisablePlayerInputs() {
            inputActions.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        { }

        public void OnLook(InputAction.CallbackContext context)
        { }

        public void OnAttack(InputAction.CallbackContext context) {
            if (!context.performed) return;
            
            OnAttackEvent.Invoke();
        }

        public void OnInteract(InputAction.CallbackContext context) 
        { }

        public void OnCrouch(InputAction.CallbackContext context) 
        { }

        public void OnJump(InputAction.CallbackContext context) {
            if (!context.performed) return;
            
            OnDiveEvent.Invoke();
        }

        public void OnPrevious(InputAction.CallbackContext context)
        { }

        public void OnNext(InputAction.CallbackContext context)
        { }

        public void OnSprint(InputAction.CallbackContext context) {
            if (context.performed) {
                OnSprintStart.Invoke();
            } else if (context.canceled) {
                OnSprintEnd.Invoke();
            }
        }
    }
}
