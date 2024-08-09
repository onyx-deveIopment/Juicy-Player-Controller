using UnityEngine;
using UnityEngine.InputSystem;

namespace ONYX 
{
    public class KUMQUAT_Character : MonoBehaviour
    {
        private enum PlayerState
        {
            Crouching,
            Sliding,
            Walking
        }

        [HideInInspector] public static KUMQUAT_Character instance;

        [Header ("Mouse Lock")]
        [SerializeField] private bool auto_lock_cursor = true;

        [Header ("Camera Look")]
        [SerializeField] private float mouse_sensitivity = 250f;

        [Header ("Jump")]
        [SerializeField] private float jump_height = 1.5f;
        [SerializeField] private float gravity = -9.81f;

        [Header ("Walk")]
        [SerializeField] private float walk_speed = 4f;
        [SerializeField] private float _walkHeight = 1f;

        [Header ("Crouch")]
        [SerializeField] private float crouch_speed = 2f;
        [SerializeField] private float crouch_height = 0.5f;

        [Header ("Debug (read-only)")]
        [ReadOnly, SerializeField] private PlayerState state;
        [ReadOnly, SerializeField] private Vector3 velocity;
        [ReadOnly, SerializeField] private float speed;
        [ReadOnly, SerializeField] private float _currentHeight;
        [ReadOnly, SerializeField] private bool is_grounded;
        [ReadOnly, SerializeField] private Transform _cameraTarget;
        [ReadOnly, SerializeField] private CharacterController controller;
        [ReadOnly, SerializeField] private float cam_x_rotation;
        [ReadOnly, SerializeField] private bool all_checks_passed;

        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private bool _slideInput;
        private bool _jumpInput;
        private bool crouch_input;

        private void Awake(){
            instance = this;
            
            _cameraTarget = GetComponentInChildren<KUMQUAT_Camera_Target>().transform;
            controller = gameObject.GetComponent<CharacterController>();

            RunChecks();
        }

        private void Start(){
            if(auto_lock_cursor){ Cursor.lockState = CursorLockMode.Locked; }
        }

        private void Update(){
            if(!all_checks_passed){ return; }

            DoCameraLook();

            is_grounded = Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, _currentHeight + 0.2f);

            // if(is_grounded && velocity.y < 0){
            //     velocity.y = -2f;
            // }

            Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y; 

            UpdatePlayerState();

            DoWalking(state == PlayerState.Walking);
            DoCrouching(state == PlayerState.Crouching);

            controller.Move(move * speed * Time.deltaTime);

            if(_jumpInput && is_grounded){
                velocity.y = Mathf.Sqrt(jump_height * -2 * gravity);
                _jumpInput = false; // Reset jump input after jump is processed
            }

            velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);
        }

        private void RunChecks(){
            all_checks_passed = true;

            if(controller == null){ all_checks_passed = false;
                Debug.LogError("Juicy Controller > No \"Character Controller\" found. Did you add one to this same GameObject?");
            }
        }

        private void UpdatePlayerState(){
            state = PlayerState.Walking;
            if(crouch_input){ state = PlayerState.Crouching; }
        }

        private void DoWalking(bool walking){
            if(walking){
                speed = walk_speed;
            }
        }

        private void DoCrouching(bool crouching){
            if(crouching){
                speed = crouch_speed;
                _currentHeight = crouch_height;
            }else{
                _currentHeight = _walkHeight;
            }

            Vector3 new_scale = transform.localScale;
            new_scale.y = _currentHeight;
            transform.localScale = new_scale;
        }

        private void DoCameraLook(){
            float mouse_x = _lookInput.x * mouse_sensitivity * Time.deltaTime;
            float mouse_y = _lookInput.y * mouse_sensitivity * Time.deltaTime;

            cam_x_rotation -= mouse_y;
            cam_x_rotation = Mathf.Clamp(cam_x_rotation, -90f, 90f);

            _cameraTarget.localRotation = Quaternion.Euler(cam_x_rotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouse_x);
        }

        public void LocomotionInput(InputAction.CallbackContext ctx){
            _moveInput = ctx.ReadValue<Vector2>();
        }

        public void LookInput(InputAction.CallbackContext ctx){
            _lookInput = ctx.ReadValue<Vector2>();
        }

        public void JumpInput(InputAction.CallbackContext ctx){
            if(ctx.performed){
                _jumpInput = true;
            }
        }

        public void SlideInput(InputAction.CallbackContext ctx){
            _slideInput = ctx.ReadValueAsButton();
        }

        public void CrouchInput(InputAction.CallbackContext ctx){
            crouch_input = ctx.ReadValueAsButton();
        }

        #region api

        #region set
        public void SetMouseSensitivity(float new_sensitivity){ mouse_sensitivity = new_sensitivity; }
        public void SetJumpHeight(float new_jump_height){ jump_height = new_jump_height; }
        public void SetGravity(float new_gravity){ gravity = new_gravity; }
        public void SetWalkSpeed(float new_walk_speed){ walk_speed = new_walk_speed; }
        public void SetCrouchSpeed(float new_crouch_speed){ crouch_speed = new_crouch_speed; }
        public void SetDefaultHeight(float new__walkHeight){ _walkHeight = new__walkHeight; }
        public void SetCrouchHeight(float new_crouch_height){ crouch_height = new_crouch_height; }
        #endregion

        #region get
        public float GetMouseSensitivity(){ return mouse_sensitivity; }
        public float GetJumpHeight(){ return jump_height; }
        public float GetGravity(){ return gravity; }
        public float GetWalkSpeed(){ return walk_speed; }
        public float GetCrouchSpeed(){ return crouch_speed; }
        public float GetDefaultHeight(){ return _walkHeight; }
        public float GetCrouchHeight(){ return crouch_height; }
        #endregion

        #endregion
    }
}