using UnityEngine;
using App.TheValleyChase.Input.GestureInput;
using App.TheValleyChase.Input.AccelerometerInput;
using App.TheValleyChase.Input.GestureInput.Contracts;
using App.TheValleyChase.Input.AccelerometerInput.Contracts;
using App.TheValleyChase.Framework;


namespace App.TheValleyChase.Player {

    public class PlayerMovement : MonoBehaviour, IOnAccelerometerInput, IOnGestureInput {

        public float turnSpeed = 8f;
        public float jumpPower = 6f;
        public float slideTime = 1f;
        public float slidingHeight = 1f;
        public float groundCheckDistance = 0.3f;


        private AccelerometerInput accelerometerInput;
        private GestureInput gestureInput;
        private Rigidbody rigidBody;
        private Quaternion targetRotation;
        private Animator animator;
        private Vector3 originalCapsuleCenter;
        private CapsuleCollider colider;
        private Transform playerMesh;

        private bool onGround;
        private bool canMove;
        private bool isRotating;
        private bool isSliding;
        private float speed;
        private float originalCapsuleHeight;

        /// <summary>
        /// Unity's Awake function.
        /// </summary>
        void Awake() {
            accelerometerInput = GameObject.FindObjectOfType<AccelerometerInput>();
            gestureInput = GameObject.FindObjectOfType<GestureInput>();

            animator = GetComponent<Animator>();
            rigidBody = GetComponent<Rigidbody>();
            colider = GetComponent<CapsuleCollider>();
            playerMesh = transform.GetChild(1).transform;
        }

        /// <summary>
        /// Unity's Start function.
        /// </summary>
        void Start() {
            accelerometerInput.RegisterListener(this);
            gestureInput.RegisterListener(this);

            canMove = true;
            speed = 1f;
            originalCapsuleHeight = colider.height;
            originalCapsuleCenter = colider.center;
        }

        /// <summary>
        /// Unity's Update function.
        /// </summary>
        void Update() {
            Move();
        }

        /// <summary>
        /// Moves the player in the scene.
        /// </summary>
        private void Move() {
            CheckGroundStatus();

            if (canMove) {
                speed = 1f;

                UpdateRotation();
                UpdateSliding();
            } else {
                speed = 0f;
            }

            UpdateAnimator();
        }

        /// <summary>
        /// Checks if the player is on ground.
        /// </summary>
        private void CheckGroundStatus() {
            RaycastHit hit;
#if UNITY_EDITOR
            Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance));
#endif
            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hit, groundCheckDistance)) {
                if (hit.collider.tag == TagsContainer.Ground) {
                    onGround = true;
                    animator.applyRootMotion = true;
                }
            } else {
                onGround = false;
                animator.applyRootMotion = false;
            }
        }

        /// <summary>
        /// Updates Rotation of the player.
        /// </summary>
        private void UpdateRotation() {
            if (isRotating) {
                Vector3 angle = transform.rotation.eulerAngles - targetRotation.eulerAngles;
                if (transform.rotation == targetRotation || Mathf.Abs(angle.magnitude) <= 3f) {
                    transform.rotation = targetRotation;
                    isRotating = false;
                } else {
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                }
            } else {
                if (transform.rotation.eulerAngles != targetRotation.eulerAngles) {
                    transform.rotation = targetRotation;
                }
            }
        }

        /// <summary>
        /// Updates isSliding variable.
        /// </summary>
        private void UpdateSliding() {
            if (isSliding) {
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Sliding") && animator.GetNextAnimatorStateInfo(0).IsName("Grounded")) {
                    isSliding = false;
                }
            }
            ResizeColiderHeight();
        }

        /// <summary>
        /// Resizes Colider height based upon the time of animation playback for sliding animation.
        /// </summary>
        private void ResizeColiderHeight() {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("Sliding")) {
                if (info.normalizedTime >= 0.22) {
                    float timeScale = 8f;
                    if (info.normalizedTime >= 0.5) {
                        colider.direction = 1;
                        colider.center = Vector3.Lerp(colider.center, originalCapsuleCenter, Time.deltaTime * timeScale);
                        playerMesh.localPosition = Vector3.Lerp(playerMesh.localPosition, Vector3.zero, Time.deltaTime * timeScale);
                        playerMesh.localRotation = Quaternion.Lerp(playerMesh.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * timeScale);
                    } else {
                        colider.direction = 2;
                        colider.center = Vector3.Lerp(colider.center, new Vector3(0, 0.25f, 0), Time.deltaTime * timeScale);
                        playerMesh.localPosition = Vector3.Lerp(playerMesh.localPosition, new Vector3(0, -0.4f, 0), Time.deltaTime * timeScale);
                        playerMesh.localRotation = Quaternion.Lerp(playerMesh.localRotation, Quaternion.Euler(-25, 0, 0), Time.deltaTime * timeScale);
                    }
                }
            }
        }

        /// <summary>
        /// Updates animator for various parameters.
        /// </summary>
        private void UpdateAnimator() {
            animator.SetFloat("Forward", speed);

            if (!onGround) {
                animator.SetFloat("Jump", rigidBody.velocity.y);
            } else {
                animator.SetFloat("Jump", 0);
            }

            float runCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.2f, 1);

            float jumpLeg = (runCycle < 0.5f ? 1 : -1);
            if (onGround) {
                animator.SetFloat("JumpLeg", jumpLeg);
            }

            animator.SetBool("Sliding", isSliding);

            animator.SetBool("OnGround", onGround);
        }

        /// <summary>
        /// Called when an accelerometer input is detected.
        /// </summary>
        /// <param name="fixedAcceleration"></param>
        public void OnAccelerometerDetected(Vector3 fixedAcceleration) {
            MoveHorizontally(fixedAcceleration.x);
        }

        /// <summary>
        /// Moves the player horizontally based upon the accelerometer input.
        /// </summary>
        /// <param name="x"></param>
        private void MoveHorizontally(float x) {
            transform.position = transform.position + Vector3.Cross(transform.up, transform.forward) * x * accelerometerInput.GetSensitivity();
        }

        /// <summary>
        /// Called when a gesture input is detected.
        /// </summary>
        /// <param name="gesture"></param>
        public void OnSwipe(Gesture gesture) {
            switch (gesture.GetGestureType()) {
                case GestureType.SWIPE_UP:
                    Jump();
                    break;
                case GestureType.SWIPE_DOWN:
                    Slide();
                    break;
                case GestureType.SWIPE_LEFT:
                    TurnLeft();
                    break;
                case GestureType.SWIPE_RIGHT:
                    TurnRight();
                    break;
            }
        }

        /// <summary>
        /// Makes the player jump.
        /// </summary>
        private void Jump() {
            if (onGround && animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded")) {
                onGround = false;
                animator.applyRootMotion = false;
                if (rigidBody.velocity.x == 0f || rigidBody.velocity.z == 0f) {
                    rigidBody.velocity = new Vector3(rigidBody.velocity.x, jumpPower, rigidBody.velocity.z) + transform.forward * speed;
                } else {
                    rigidBody.velocity = new Vector3(rigidBody.velocity.x, jumpPower, rigidBody.velocity.z);
                }
            }
        }

        /// <summary>
        /// Makes the player slide.
        /// </summary>
        private void Slide() {
            if (onGround && !isSliding) {
                isSliding = true;
            }
        }

        /// <summary>
        /// Turns the player to right.
        /// </summary>
        private void TurnRight() {
            ApplyRotation(90);
        }

        /// <summary>
        /// Turns the player to left.
        /// </summary>
        private void TurnLeft() {
            ApplyRotation(-90);
        }

        /// <summary>
        /// Applies rotation to the player.
        /// </summary>
        /// <param name="degrees"></param>
        void ApplyRotation(float degrees) {
            if (!isRotating && !isSliding && onGround) {
                Quaternion newRotation = Quaternion.identity;
                newRotation = Quaternion.AngleAxis(degrees, Vector3.up);
                targetRotation = transform.rotation * newRotation;
                isRotating = true;
            }
        }

        /// <summary>
        /// Stops the movement of the player.
        /// </summary>
        public void StopMovement() {
            canMove = false;
        }
    }

}
