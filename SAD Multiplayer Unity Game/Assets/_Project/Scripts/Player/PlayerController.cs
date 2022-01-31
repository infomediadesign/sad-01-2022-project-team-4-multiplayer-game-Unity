using System;
using _Project.Scripts.Networking;
using UnityEngine;

namespace _Project.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Ground Check")] [SerializeField]
        private float groundedOffset = .14f;

        [SerializeField] private float groundedRadius = .28f;
        [SerializeField] private LayerMask groundLayer;
        private bool _grounded;

        [Header("Gravity")] [SerializeField] private float gravity = -15f;
        [SerializeField] private float jumpHeight = 5f;
        private float _verticalVelocity;
        private bool _jump;

        [Header("Movement")] [SerializeField] private bool rawInput;
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 10f;
        private float _playerSpeed = 10f;
        private Vector3 _inputVector;
        private Vector3 _movement;
        private bool _isSprinting;

        [Header("Camera Motion")] [SerializeField]
        private bool rawMouseLookInput;

        [SerializeField] private float mouseXSensitivity = 5f;
        [SerializeField] private float mouseYSensitivity = 5f;
        [SerializeField] private bool lockCursor;
        [SerializeField] private CursorLockMode currentCursorLockMode;
        [SerializeField] private Vector2 inputMouseLookVector;
        [SerializeField] private Vector2 mouseLookVector;

        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform cameraTransform;

        [SerializeField] private bool lockPlayerInput = false;

        public void SetLockInput(bool state)
        {
            lockPlayerInput = state;
            if (lockPlayerInput)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                
                //Zero Out Movement 
                _inputVector.x = 0;
                _inputVector.z = 0;

                //Zero out look rotations
                inputMouseLookVector.x = 0;
                inputMouseLookVector.y = 0;
            }

            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        public bool GetLockInputState()
        {
            return lockPlayerInput;
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            
            Cursor.visible = lockCursor;
            Cursor.lockState = currentCursorLockMode;
        }

        private void Update()
        {
            if (!GetComponent<PlayerSetup>().isLocalPlayer)
            {
               return;
            }

            if (!lockPlayerInput)
            {
                HandleInput();
            }
            GroundCheck();
            Jump();
            ApplyGravity();
            Move();
            CameraMove();
            SocketManager.GetInstance().SendPositionUpdate(transform.position);
        }

        private void HandleInput()
        {
            _inputVector.x = rawInput ? Input.GetAxisRaw("Horizontal") : Input.GetAxis("Horizontal");
            _inputVector.z = rawInput ? Input.GetAxisRaw("Vertical") : Input.GetAxis("Vertical");

            inputMouseLookVector.x = rawMouseLookInput ? Input.GetAxisRaw("Mouse X") : Input.GetAxis("Mouse X");
            inputMouseLookVector.y = rawMouseLookInput ? Input.GetAxisRaw("Mouse Y") : Input.GetAxis("Mouse Y");

            _isSprinting = Input.GetKey(KeyCode.LeftShift);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _jump = true;
            }
        }

        private void GroundCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x,
                transform.position.y - groundedOffset, transform.position.z);
            _grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayer,
                QueryTriggerInteraction.Ignore);
        }

        private void Jump()
        {
            if (_jump && _grounded)
            {
                _jump = false;
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        private void ApplyGravity()
        {
            _verticalVelocity += gravity * Time.deltaTime;
            if (_grounded && _verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }
        }

        private void Move()
        {
            _playerSpeed = _isSprinting ? sprintSpeed : walkSpeed;

            _movement = (cameraTransform.right * _inputVector.x * _playerSpeed) +
                        (cameraTransform.forward * _inputVector.z * _playerSpeed);
            _movement.y = _verticalVelocity;
            characterController.Move(_movement * Time.deltaTime);
        }

        private void CameraMove()
        {
            mouseLookVector = new Vector2(inputMouseLookVector.x * mouseXSensitivity,
                inputMouseLookVector.y * mouseYSensitivity);


            Vector3 newEulerAngles = cameraTransform.localRotation.eulerAngles +
                                     new Vector3(-mouseLookVector.y, mouseLookVector.x, 0);
            if (newEulerAngles.x > 180)
            {
                newEulerAngles.x = newEulerAngles.x - 360;
            }

            newEulerAngles.x = Mathf.Clamp(newEulerAngles.x, -90, 90);

            cameraTransform.localRotation = Quaternion.Euler(newEulerAngles);


        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (_grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(new Vector3(transform.position.x,
                transform.position.y - groundedOffset, transform.position.z), groundedRadius);
        }
    }
}
