using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Components")]
    private CharacterController controller;

    [Header("Movement")]
    private Vector3 playerVelocity;
    private float gravity = -9.81f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    public bool IsMoving { get; private set; }
    public bool IsGrounded { get; private set; }
    public float CurrentSpeedNormalized { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void Move(Vector3 direction, float speed)
    {
        controller.Move(direction.normalized * Time.deltaTime * speed);

        IsMoving = direction.magnitude > 0.1f;
        
        if (IsMoving)
        {
            float currentSpeed = controller.velocity.magnitude;
            CurrentSpeedNormalized = Mathf.Clamp01(currentSpeed / speed);
        }
        else
        {
            CurrentSpeedNormalized = 0f;
        }
    }

    public void Rotate(Vector3 direction)
    {
        if (direction.magnitude > 0.1f)
        {
            transform.forward = direction;
        }
    }

    public void RotateToPosition(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Mantém rotação apenas no plano horizontal
        
        Rotate(direction);
    }


    public void ApplyGravity()
    {
        // Ground check
        IsGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (IsGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        // Aplica gravidade
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void Stop()
    {
        IsMoving = false;
        CurrentSpeedNormalized = 0f;
    }
}