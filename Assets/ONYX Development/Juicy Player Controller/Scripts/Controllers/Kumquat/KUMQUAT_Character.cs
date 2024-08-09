using UnityEngine;
using UnityEngine.InputSystem;

// TODO:
// - Add tooltips to all variable to explain what they do.
// - Add coyote time.
// - Add jump sustain.

namespace ONYX
{
    public class KUMQUAT_Character : MonoBehaviour
    {
        private enum PlayerState {
            Crouching,
            Sliding,
            Walking
        }

        private enum GroundState {
            Grounded,
            InAir
        }

        [Header ("Settings/Look")]
        [SerializeField] private bool _lockCursorOnAwake = true;
        [SerializeField] private bool _isYAxisInverted = false;

        [SerializeField] private Vector2 _lookSensitivity = new Vector2(20, 20);

        [Range (-90, 90)] [SerializeField] private float _minLookAngle = -90f;
        [Range (-90, 90)] [SerializeField] private float _maxLookAngle = 90f;

        [Header ("Settings/Crouch")]
        [SerializeField] private float _crouchSpeed = 10f;
        [SerializeField] private float _crouchHeight = 0.5f;

        [Header ("Settings/Slide")]
        [SerializeField] private float _slideStartSpeed = 20f;
        [SerializeField] private float _slideEndSpeed = 10f;
        [SerializeField] private float _slideFriction = 0.9f;

        [Header ("Settings/Walk")]
        [SerializeField] private float _walkSpeed = 15f;
        [SerializeField] private float _walkHeight = 1f;

        [Header ("Settings/Jump")]
        [SerializeField] private float _jumpSpeed = 7.5f;
        [SerializeField] private float _gravity = -19.62f;
        [Range (0, 1)]
        [SerializeField] private float _jumpSustainGravity = 0.5f;

        [Header ("Debug/References")]
        [ReadOnly, SerializeField] private KUMQUAT_Camera _camera;
        [ReadOnly, SerializeField] private Transform _cameraTarget;

        [Header ("Debug/States")]
        [ReadOnly, SerializeField] private PlayerState _playerState;
        [ReadOnly, SerializeField] private GroundState _groundState;
        [ReadOnly, SerializeField] private GroundState _lastGroundState;

        [Header ("Debug/Other")]
        [ReadOnly, SerializeField] private Vector3 _currentVelocity;
        [ReadOnly, SerializeField] private float _playerHeight = 1; // REMOVE: remove the "= 1" after the states are implamentd.
        [ReadOnly, SerializeField] private bool _leftGroundBecauseOfJump;
        [ReadOnly, SerializeField] private float _effectiveGravity;

        [Header ("Debug/Inputs")]
        [ReadOnly, SerializeField] private Vector2 _moveInput;
        [ReadOnly, SerializeField] private Vector2 _lookInput;
        [ReadOnly, SerializeField] private bool _requestedJump;
        [ReadOnly, SerializeField] private bool _requestSustainJump;
        [ReadOnly, SerializeField] private bool _slideInput;
        [ReadOnly, SerializeField] private bool _crouchInput;

        private void Awake()
        {
            _camera = transform.parent.GetComponentInChildren<KUMQUAT_Camera>();
            _cameraTarget = GetComponentInChildren<KUMQUAT_Camera_Target>().transform;

            if(_lockCursorOnAwake)
                Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            DoLook();

            UpdateGroundState();

            DoGravityAndJump();
            
            transform.position += _currentVelocity * Time.deltaTime;

            _lastGroundState = _groundState;
        }

        private void DoLook()
        {
            Vector2 lookWithSensitivity = _lookInput * _lookSensitivity * Time.deltaTime;

            if (!_isYAxisInverted)
                lookWithSensitivity.y *= -1;

            Vector3 currentLook = _cameraTarget.localRotation.eulerAngles;

            float currentPitch = currentLook.x;
            if (currentPitch > 180f) currentPitch -= 360f;

            float newXRotation = currentPitch + lookWithSensitivity.y;

            newXRotation = Mathf.Clamp(newXRotation, _minLookAngle, _maxLookAngle);

            float newYRotation = transform.rotation.eulerAngles.y + lookWithSensitivity.x;

            _cameraTarget.localRotation = Quaternion.Euler(newXRotation, 0, 0);
            transform.rotation = Quaternion.Euler(0, newYRotation, 0);
        }

        private void UpdateGroundState()
        {
            float rayDistance = _playerHeight;

            if (Physics.Raycast(transform.position, Vector3.down, rayDistance))
                _groundState = GroundState.Grounded;
            else
                _groundState = GroundState.InAir;
        }

        private void DoGravityAndJump(){
            _effectiveGravity = _gravity;
            if(_requestSustainJump && Vector3.Dot(_currentVelocity, transform.up) > 0)
                _effectiveGravity *= _jumpSustainGravity;

            _currentVelocity.y += _effectiveGravity * Time.deltaTime;

            if(_groundState == GroundState.Grounded && _lastGroundState == GroundState.InAir) _leftGroundBecauseOfJump = false;

            if(_requestedJump && _groundState == GroundState.Grounded)
            {
                var currentVerticalSpeed = Vector3.Dot(_currentVelocity, transform.up);
                var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, _jumpSpeed);
                _currentVelocity += transform.up * (targetVerticalSpeed - currentVerticalSpeed);

                _requestedJump = false;
                _leftGroundBecauseOfJump = true;
            }

            if(_groundState == GroundState.Grounded && !_leftGroundBecauseOfJump)
            {
                _currentVelocity.y = 0;
            }
        }

        #region INPUTS
        public void LocomotionInput(InputAction.CallbackContext ctx)
        {
            _moveInput = ctx.ReadValue<Vector2>();
        }

        public void LookInput(InputAction.CallbackContext ctx)
        {
            _lookInput = ctx.ReadValue<Vector2>();
        }

        public void JumpInput(InputAction.CallbackContext ctx)
        {
            _requestSustainJump = ctx.ReadValueAsButton();
            if(ctx.performed)
                _requestedJump = true;
        }

        public void SlideInput(InputAction.CallbackContext ctx)
        {
            _slideInput = ctx.ReadValueAsButton();
        }

        public void CrouchInput(InputAction.CallbackContext ctx)
        {
            _crouchInput = ctx.ReadValueAsButton();
        }
        #endregion
    }
}