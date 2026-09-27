using UnityEngine;
using UnityEngine.InputSystem;
using UnobservedRooms.Core;
using UnobservedRooms.UI;

namespace UnobservedRooms.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 3.2f;
        [SerializeField] private float sprintSpeed = 5f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float mouseSensitivity = .12f;
        [SerializeField] private float gravity = -22f;
        private CharacterController controller;
        private Transform view;
        private Vector3 planarVelocity;
        private float verticalVelocity;
        private float pitch;
        private GameStateMachine stateMachine;
        private bool failed;

        public void Configure(Transform cameraTransform, GameStateMachine gameState)
        {
            controller = GetComponent<CharacterController>();
            view = cameraTransform;
            stateMachine = gameState;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Awake() => controller = GetComponent<CharacterController>();

        private void Update()
        {
            if (!failed && transform.position.y < -8f)
            {
                failed = true;
                stateMachine?.TryEnter(GameFlowState.Collapse);
                GameHud.ShowResult("COHERENCE COLLAPSE\nYou fell beyond the observed facility.\n\nPress R to retry.");
                GameAudio.Play(AudioCue.Collapse,transform.position);
                Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            }
            if (failed) return;
            if (view == null || Keyboard.current == null || Mouse.current == null) return;
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                var locked = Cursor.lockState == CursorLockMode.Locked;
                Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = locked;
            }
            if (Cursor.lockState != CursorLockMode.Locked) return;

            var look = Mouse.current.delta.ReadValue() * mouseSensitivity;
            transform.Rotate(Vector3.up, look.x, Space.Self);
            pitch = Mathf.Clamp(pitch - look.y, -85f, 85f);
            view.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            var input = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed) input.y -= 1;
            if (Keyboard.current.dKey.isPressed) input.x += 1;
            if (Keyboard.current.aKey.isPressed) input.x -= 1;
            input = Vector2.ClampMagnitude(input, 1f);
            var targetSpeed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;
            var desired = (transform.forward * input.y + transform.right * input.x) * targetSpeed;
            planarVelocity = Vector3.MoveTowards(planarVelocity, desired, acceleration * Time.deltaTime);
            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity += gravity * Time.deltaTime;
            controller.Move((planarVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
        }
    }
}
