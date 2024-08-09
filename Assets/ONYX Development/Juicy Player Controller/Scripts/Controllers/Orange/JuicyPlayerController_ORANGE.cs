using UnityEngine;
using UnityEngine.InputSystem;

namespace ONYX {
    public class JuicyPlayerController_ORANGE : MonoBehaviour {
        [HideInInspector] public static JuicyPlayerController_ORANGE instance;

        [Header ("Mouse Lock")]
        [SerializeField] private bool auto_lock_cursor = true;

        [Header ("Camera Look")]
        [SerializeField] private string camera_container_tag = "camera_container";
        [SerializeField] private float mouse_sensitivity = 250f;

        [Header ("Jump")]
        [SerializeField] private float jump_height = 1.5f;
        [SerializeField] private float gravity = -9.81f;

        [Header ("Walk")]
        [SerializeField] private float walk_speed = 4f;

        [Header ("Run")]
        [SerializeField] private float run_speed = 6f;
        [SerializeField] private float stamina_drain = -10f;
        [SerializeField] private float stamina_regen = 5f;
        [SerializeField] private float max_stamina = 100f;

        [Header ("Crouch")]
        [SerializeField] private float crouch_speed = 2f;
        [SerializeField] private float default_height = 1f;
        [SerializeField] private float crouch_height = 0.5f;

        [Header ("Ground Check")]
        [SerializeField] private string ground_check_tag = "ground_check";
        [SerializeField] private string ground_layer_name = "Ground";
        [SerializeField] private float ground_distance = 0.4f;

        [Header ("Debug (read-only)")]
        [ReadOnly, SerializeField] private PlayerState state;
        [ReadOnly, SerializeField] private Vector3 velocity;
        [ReadOnly, SerializeField] private float speed;
        [ReadOnly, SerializeField] private float height;
        [ReadOnly, SerializeField] private float stamina;
        [ReadOnly, SerializeField] private bool is_grounded;
        [ReadOnly, SerializeField] private Transform camera_container;
        [ReadOnly, SerializeField] private Transform ground_check;
        [ReadOnly, SerializeField] private LayerMask ground_mask;
        [ReadOnly, SerializeField] private CharacterController controller;
        [ReadOnly, SerializeField] private float cam_x_rotation;
        [ReadOnly, SerializeField] private bool all_checks_passed;

        private Vector2 move_input;
        private Vector2 look_input;
        private bool jump_input;
        private bool run_input;
        private bool crouch_input;

        private enum PlayerState {
            Walking,
            Running,
            Crouching
        }

        private void Awake(){
            instance = this;
            
            stamina = max_stamina;

            camera_container = GameObject.FindGameObjectsWithTag(camera_container_tag)[0].transform;
            ground_check = GameObject.FindGameObjectsWithTag(ground_check_tag)[0].transform;
            controller = gameObject.GetComponent<CharacterController>();
            ground_mask = LayerMask.GetMask(ground_layer_name);

            RunChecks();
        }

        private void Start(){
            if(auto_lock_cursor){ Cursor.lockState = CursorLockMode.Locked; }
        }

        private void Update(){
            if(!all_checks_passed){ return; }

            DoCameraLook();

            is_grounded = Physics.CheckSphere(ground_check.position, ground_distance, ground_mask);

            if(is_grounded && velocity.y < 0){
                velocity.y = -2f;
            }

            Vector3 move = transform.right * move_input.x + transform.forward * move_input.y; 

            UpdatePlayerState();

            DoWalking(state == PlayerState.Walking);
            DoRunning(state == PlayerState.Running);
            DoCrouching(state == PlayerState.Crouching);

            controller.Move(move * speed * Time.deltaTime);

            if(jump_input && is_grounded){
                velocity.y = Mathf.Sqrt(jump_height * -2 * gravity);
                jump_input = false; // Reset jump input after jump is processed
            }

            velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);
        }

        private void RunChecks(){
            all_checks_passed = true;

            if(camera_container == null){ all_checks_passed = false;
                Debug.LogError("Juicy Controller > No camera container GameObject with tag: \"" + camera_container_tag + "\" found. Did you create a tag?");
            }
            if(ground_check == null){ all_checks_passed = false;
                Debug.LogError("Juicy Controller > No ground check GameObject with tag: \"" + ground_check_tag + "\" found. Did you create a tag?");
            }
            if(controller == null){ all_checks_passed = false;
                Debug.LogError("Juicy Controller > No \"Character Controller\" found. Did you add one to this same GameObject?");
            }
            if(ground_mask == 0) { all_checks_passed = false;
                Debug.LogError("Juicy Controller > No ground layer named \"" + ground_layer_name + "\" found. Did you have a ground layer?");
            }
        }

        private void UpdatePlayerState(){
            state = PlayerState.Walking;
            if(run_input){ state = PlayerState.Running; }
            if(crouch_input){ state = PlayerState.Crouching; }
        }

        private void DoWalking(bool walking){
            if(walking){
                speed = walk_speed;
            }
        }

        private void DoRunning(bool running){
            if(running && stamina > 0 && move_input.magnitude > 0){
                stamina += stamina_drain * Time.deltaTime;
            }else{
                stamina += stamina_regen * Time.deltaTime;
            }

            if(running && stamina >= 1f){
                speed = run_speed;
            }else{
                speed = walk_speed;
            }

            stamina = Mathf.Clamp(stamina, 0, max_stamina);
        }

        private void DoCrouching(bool crouching){
            if(crouching){
                speed = crouch_speed;
                height = crouch_height;
            }else{
                height = default_height;
            }

            Vector3 new_scale = transform.localScale;
            new_scale.y = height;
            transform.localScale = new_scale;
        }

        private void DoCameraLook(){
            float mouse_x = look_input.x * mouse_sensitivity * Time.deltaTime;
            float mouse_y = look_input.y * mouse_sensitivity * Time.deltaTime;

            cam_x_rotation -= mouse_y;
            cam_x_rotation = Mathf.Clamp(cam_x_rotation, -90f, 90f);

            camera_container.localRotation = Quaternion.Euler(cam_x_rotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouse_x);
        }

        public void LocomotionInput(InputAction.CallbackContext ctx){
            move_input = ctx.ReadValue<Vector2>();
        }

        public void LookInput(InputAction.CallbackContext ctx){
            look_input = ctx.ReadValue<Vector2>();
        }

        public void JumpInput(InputAction.CallbackContext ctx){
            if(ctx.performed){
                jump_input = true;
            }
        }

        public void RunInput(InputAction.CallbackContext ctx){
            run_input = ctx.ReadValueAsButton();
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
        public void SetRunSpeed(float new_run_speed){ run_speed = new_run_speed; }
        public void SetStaminaDrain(float new_stamina_drain){ stamina_drain = new_stamina_drain; }
        public void SetStaminaRegen(float new_stamina_regen){ stamina_regen = new_stamina_regen; }
        public void SetMaxStamina(float new_max_stamina){ max_stamina = new_max_stamina; }
        public void SetCrouchSpeed(float new_crouch_speed){ crouch_speed = new_crouch_speed; }
        public void SetDefaultHeight(float new_default_height){ default_height = new_default_height; }
        public void SetCrouchHeight(float new_crouch_height){ crouch_height = new_crouch_height; }
        public void SetGroundDistance(float new_ground_distance){ ground_distance = new_ground_distance; }
        #endregion

        #region get
        public float GetMouseSensitivity(){ return mouse_sensitivity; }
        public float GetJumpHeight(){ return jump_height; }
        public float GetGravity(){ return gravity; }
        public float GetWalkSpeed(){ return walk_speed; }
        public float GetRunSpeed(){ return run_speed; }
        public float GetStaminaDrain(){ return stamina_drain; }
        public float GetStaminaRegen(){ return stamina_regen; }
        public float GetMaxStamina(){ return max_stamina; }
        public float GetCrouchSpeed(){ return crouch_speed; }
        public float GetDefaultHeight(){ return default_height; }
        public float GetCrouchHeight(){ return crouch_height; }
        public float GetGroundDistance(){ return ground_distance; }
        #endregion

        #endregion
    }
}