using System;
using UnityEngine;
using App.TheValleyChase.Input.GestureInput;
using App.TheValleyChase.Input.AccelerometerInput;
using App.TheValleyChase.Input.GestureInput.Contracts;
using App.TheValleyChase.Input.AccelerometerInput.Contracts;
using App.TheValleyChase.Framework;

public class PlayerMovement : MonoBehaviour, IOnAccelerometerInput,IOnGestureInput {

    public float turnSpeed = 8f;
    public float jumpPower = 6f;
    public float slideTime = 1f;
    public float groundCheckDistance = 0.3f;

    private AccelerometerInput accelerometerInput;
    private GestureInput gestureInput;
    private Rigidbody rigidBody;
    private Quaternion targetRotation;
    private Animator animator;

    private bool onGround;
    private bool canMove;
    private bool isRotating;
    private bool isSliding;
    private float speed = 1f;   

    void Awake() {
        accelerometerInput = GameObject.FindObjectOfType<AccelerometerInput>();
        gestureInput = GameObject.FindObjectOfType<GestureInput>();

        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
    }
    
	void Start () {
        accelerometerInput.RegisterListener(this);
        gestureInput.RegisterListener(this);

        canMove = true;
	}
	
	void Update () {
        Move();
	}

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

    private void UpdateSliding() {
        if (isSliding) {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Sliding") && animator.GetNextAnimatorStateInfo(0).IsName("Grounded")) {
                isSliding = false;
            }
        }
    }

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

    public void OnAccelerometerDetected(Vector3 fixedAcceleration) {
        MoveHorizontally(fixedAcceleration.x);
    }

    private void MoveHorizontally(float x) {
        transform.position = transform.position + Vector3.Cross(transform.up, transform.forward) * x * accelerometerInput.GetSensitivity();
    }

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

    private void Slide() {
        if (onGround && !isSliding) {
            isSliding = true;
        }
    }

    private void TurnRight() {
        ApplyRotation(90);
    }

    private void TurnLeft() {
        ApplyRotation(-90);
    }

    void ApplyRotation(float degrees) {
        if (!isRotating && !isSliding && onGround) {
            Quaternion newRotation = Quaternion.identity;
            newRotation = Quaternion.AngleAxis(degrees, Vector3.up);
            targetRotation = transform.rotation * newRotation;
            isRotating = true;
        }
    }

    private void Jump() {
        if (onGround && animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded")) {
            onGround = false;
            animator.applyRootMotion = false;
            if(rigidBody.velocity.x == 0f || rigidBody.velocity.z == 0f) {
                rigidBody.velocity = new Vector3(rigidBody.velocity.x, jumpPower, rigidBody.velocity.z) + transform.forward * speed;
            } else {
                rigidBody.velocity = new Vector3(rigidBody.velocity.x, jumpPower, rigidBody.velocity.z);
            }
        }
    }
}
