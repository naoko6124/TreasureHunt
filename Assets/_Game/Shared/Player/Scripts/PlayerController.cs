using UnityEngine;
using UnityEngine.InputSystem;

namespace _Game.Shared.Player.Scripts
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4.0f;
        [SerializeField] private float mouseSensitivity = 0.5f;
        [SerializeField] private float gravity = -1.0f;
        private Vector2 _moveInput;
        private Vector2 _cameraInput;
        private float cameraRot = 0f;

        [SerializeField] private Transform playerCamera;
        private CharacterController _characterController;
        private PlayerActions _playerActions;
        private Camera _camera;

        
        [Header("Chest")]
        [SerializeField] private ChestCollector chestCollector;

        private void OnEnable()
        {
            _characterController = GetComponent<CharacterController>();
            _playerActions = new PlayerActions();
            _playerActions.Enable();
            _playerActions.Chest.Collect.performed += Collect;
            _camera = Camera.main;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable()
        {
            _playerActions.Disable();
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

        private void Collect(InputAction.CallbackContext context)
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100.0f))
            {
                if (hit.transform.CompareTag("Chest"))
                {
                    chestCollector.IncreaseChest();
                    Destroy(hit.transform.gameObject);
                }
            }
        }
    }
}