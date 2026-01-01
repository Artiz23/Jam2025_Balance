using System;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 1f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.2f;

    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    

    [Header("References")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private Transform cameraMimic;
    [SerializeField] private Transform cameraTransform;

    public static PlayerMove localPlayerMove;



    private CharacterController characterController;
    private Vector3 velocity;
    private float verticalRotation = 0f;


    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        localPlayerMove = this;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        

        if (playerCamera == null)
        {
            enabled = false;
            return;
        }
    }
    

    private void Update()
    { 
        HandleMovement();
        HandleRotation(); 
    }


    private void HandleMovement()
    {
        if(!characterController)
            return;

        bool isGrounded = IsGrounded();
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

        if(characterController.enabled == true)
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        if(characterController.enabled == true)
            characterController.Move(velocity * Time.deltaTime);
    }

  

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        cameraMimic.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    
        transform.Rotate(Vector3.up * mouseX);

    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.03f, Vector3.down, groundCheckDistance);
    }



#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.03f, Vector3.down * groundCheckDistance); 
    }
#endif
}