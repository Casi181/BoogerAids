using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform target;

    public float mouseSensitivity = 200f;
    public float verticalClamp = 70f;

    private InputSystem_Actions input;
    private float xRotation = 0f;
    private float yRotation = 0f;

    public float defaultDistance = 7f;

    private float currentDistance;

    // Position lag
    private Vector3 smoothedTargetPos;
    public float normalFollowSpeed = 10f;   // units per second — consistent catch-up
    public float dashFollowSpeed = 3f;       // lerp factor — intentional eased lag during dash
    private bool isDashingFollow = false;


    private void Awake()
    {
        input = new InputSystem_Actions();
        currentDistance = defaultDistance;
        smoothedTargetPos = target != null ? target.position : Vector3.zero;
    }

    private void OnEnable()
    {
        if (input == null) input = new InputSystem_Actions();
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input?.Player.Disable();
    }

    private void LateUpdate()
    {
        Vector2 lookInput = input.Player.Look.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);

        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0f);

        Vector3 pivotPosition = target.position + Vector3.up * 1.5f;

        // During dash: Lerp for a smooth lag. After dash: MoveTowards for consistent catch-up speed.
        if (isDashingFollow)
            smoothedTargetPos = Vector3.Lerp(smoothedTargetPos, pivotPosition, dashFollowSpeed * Time.deltaTime);
        else
            smoothedTargetPos = Vector3.MoveTowards(smoothedTargetPos, pivotPosition, normalFollowSpeed * Time.deltaTime);

        Vector3 basePosition = smoothedTargetPos - rotation * Vector3.forward * currentDistance;
        Vector3 desiredPosition = basePosition;

        RaycastHit hit;

        if (Physics.Linecast(smoothedTargetPos, desiredPosition, out hit))
        {
            transform.position = hit.point + hit.normal * 0.2f;
        }
        else
        {
            transform.position = desiredPosition;
        }

        transform.rotation = rotation;
    }

    public void SetDashFollowMode(bool dashing)
    {
        isDashingFollow = dashing;
    }
}
