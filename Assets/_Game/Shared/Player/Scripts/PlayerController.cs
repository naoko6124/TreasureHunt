using UnityEngine;

namespace _Game.Shared.Player.Scripts
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3.0f;
        [SerializeField] private float mouseSensitivity = 0.5f;
        [SerializeField] private float gravity = -1.0f;
        private Vector2 _moveInput;
        private Vector2 _cameraInput;
        private float cameraRot = 0f;

        [SerializeField] private Transform playerCamera;
        private CharacterController _characterController;
        private PlayerActions _playerActions;
        
        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _playerActions = new PlayerActions();
            _playerActions.Enable();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        private void Update()
        {
            _moveInput = _playerActions.Character.Movement.ReadValue<Vector2>();
            _cameraInput = _playerActions.Character.Camera.ReadValue<Vector2>();

            Vector3 moveAxis = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            _characterController.Move(moveAxis * (moveSpeed * Time.deltaTime));
            transform.Rotate(Vector3.up * (_cameraInput.x * mouseSensitivity * Time.deltaTime));

            Vector3 velocity = Vector3.zero;
            velocity.y += gravity;
            _characterController.Move(velocity * Time.deltaTime);

            cameraRot -= _cameraInput.y * mouseSensitivity * Time.deltaTime;
            cameraRot = Mathf.Clamp(cameraRot, -90f, 90f);
            playerCamera.localRotation = Quaternion.Euler(cameraRot, 0f, 0f);
        }
    }
}