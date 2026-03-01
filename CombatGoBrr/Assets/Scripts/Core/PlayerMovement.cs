using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public AnimationCurve dashSpeedCurve;
    private CameraController cameraController;
    public float dashSpeed = 30f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.6f;


    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector3 dashDirection;


    public float moveSpeed = 6f;
    public float rotationSpeed = 15f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Camera mainCamera;
    private InputSystem_Actions input;
    private Vector2 moveInput;
    private float verticalVelocity;

    private void Awake()
    {
        cameraController = Object.FindFirstObjectByType<CameraController>();
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;

        input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

    private void Update()
{
    moveInput = input.Player.Move.ReadValue<Vector2>();

    if (dashCooldownTimer > 0f)
        dashCooldownTimer -= Time.deltaTime;

    // Trigger dash
    if (!isDashing && dashCooldownTimer <= 0f && input.Player.Dash.WasPressedThisFrame())
    {
        StartDash();
    }

    // If currently dashing, override normal movement
    if (isDashing)
    {
        dashTimer -= Time.deltaTime;

        float dashProgress = 1f - (dashTimer / dashDuration);
        float speedMultiplier = dashSpeedCurve.Evaluate(dashProgress);

        Vector3 dashVelocity = dashDirection * dashSpeed * speedMultiplier;

        controller.Move(dashVelocity * Time.deltaTime);

        if (dashTimer <= 0f)
        {
            isDashing = false;
            if (cameraController != null)
                cameraController.SetDashFollowMode(false);
        }

        return;
    }

    HandleMovement();
}

    private void HandleMovement()
    {
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (inputDirection.magnitude > 1f)
            inputDirection.Normalize();

        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = cameraForward * inputDirection.z + cameraRight * inputDirection.x;

        if (controller.isGrounded)
            verticalVelocity = -2f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = moveDirection * moveSpeed;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    private void StartDash()
    {
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (inputDirection.sqrMagnitude > 0.01f)
        {
            inputDirection.Normalize();

            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            dashDirection = cameraForward * inputDirection.z + cameraRight * inputDirection.x;
        }
        else
        {
            dashDirection = transform.forward;
        }

        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        if (cameraController != null)
        {
            cameraController.SetDashFollowMode(true);
        }
    }
}