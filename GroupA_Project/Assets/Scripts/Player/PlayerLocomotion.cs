using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    private PlayerController controller;
    private Rigidbody rb;

    [Header("Camera Settings")]
    private Transform cameraObj;

    [Header("Horizontal Movement Settings")]
    public float walkSpeed = 3;
    public float sprintSpeed = 6;
    private Vector3 moveDirection;
    private float actualSpeed;

    [Header("Vertical Movement Settings")]
    public float jumpForce = 8;
    public float fallingSpeed = -20;
    private float verticalSpeed;


    [Header("Rotation Settings")]
    public float rotationSpeed = 25;
    public float aimRotationSpeed = 25;



    [Header("Collision Settings")]
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.25f;
    public float stepCheckRayDistance = 1.5f;
    public float maxStepHeight = 0.3f;
    public float stepUpSpeed = 18;
    public float wallRayDistance = 0.6f;
    public float wallRayAngle = 0.2f;

    public void GetPlayerLocomotionComponents()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<PlayerController>();
    }

    public void SetPlayerLocomotion()
    {
        cameraObj = Camera.main.transform;
    }

    //Movement-------------------------------------
    public void HandleMovement(float delta) 
    {
        //SetInputs
        float xAxis = controller.hAxisInput;
        float zAxis = controller.vAxisInput;

        //GetDirection
        Vector3 direction = new Vector3(xAxis, 0, zAxis);
        direction.Normalize();
        direction.y = 0;

        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraObj.eulerAngles.y;
        moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward * controller.axisInputAmount;

        //SetSpeed
        if (Input.GetKey(KeyCode.LeftShift))
        {
            actualSpeed = sprintSpeed;
        }
        else
        {
            actualSpeed = walkSpeed;
        }

        //WallCollision
        if (controller.wallCollision) actualSpeed = 0;

        Vector3 verticalVelocity = Vector3.up * verticalSpeed;
        Vector3 horizontalVelocity = moveDirection * actualSpeed * controller.axisInputAmount;

        Vector3 movementVelocity = horizontalVelocity + verticalVelocity;
        
        rb.position += movementVelocity * delta;
    }
    //-------------------------------------

    //Rotation-------------------------------------
    public void HandleRotation(float delta)
    {
        Vector3 targetDirection = moveDirection;
        if (moveDirection == Vector3.zero) targetDirection = transform.forward;

        Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, lookRotation, delta * rotationSpeed);

        rb.rotation = targetRotation;
    }
    public void HandleAimRotation(float delta)
    {
        Vector3 targetDirection = transform.forward - (transform.right * controller.aimAxisInput.x);

        targetDirection.Normalize();
        targetDirection.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, lookRotation, delta * aimRotationSpeed);

        rb.rotation = targetRotation;


    }
    //-------------------------------------

    public void HandleJump()
    {
        if (controller.jumpInput && controller.isGrounded)
        {
            verticalSpeed = jumpForce;
        }
    }

    public void HandleGravity(float delta)
    {
        if (controller.isGrounded && verticalSpeed < 0.1f)
        {
            verticalSpeed = 0;
        }
        else if (!controller.isGrounded)
        {
            verticalSpeed += fallingSpeed * delta; 
        }

        verticalSpeed = Mathf.Clamp(verticalSpeed, fallingSpeed, 20);
    }
    public void HandleGroundCollision(float delta)
    {
        Vector3 groundPos = transform.position;
        Vector3 centerPos = transform.position + Vector3.up; 

        bool groundDetection = Physics.CheckSphere(groundPos, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
        controller.isGrounded = groundDetection;

        if (groundDetection)
        {
            RaycastHit hit;
            bool hitGround;

            if (controller.axisInputAmount > 0.1f) 
            {
                hitGround = Physics.Raycast(centerPos + transform.forward * 0.3f, -Vector3.up, out hit, stepCheckRayDistance, groundLayer);
                Debug.DrawRay(centerPos + transform.forward * 0.3f, -Vector3.up * stepCheckRayDistance, Color.blue);

            }
            else 
            {
                hitGround = Physics.Raycast(centerPos, -Vector3.up, out hit, stepCheckRayDistance, groundLayer);
                Debug.DrawRay(centerPos, -Vector3.up * stepCheckRayDistance, Color.blue);
            }

            Vector3 targetPosition = transform.position;
            targetPosition.y = hit.point.y; 

            if (hitGround && verticalSpeed < 0.1f)
            {
                float stepPositionDifference = targetPosition.y - transform.position.y;
                if(Mathf.Abs(stepPositionDifference) < maxStepHeight)
                {
                    rb.position = Vector3.Slerp(transform.position, targetPosition, delta * stepUpSpeed);
                }
            }
        }
    }

    public void HandleWallCollision()
    {
        bool wallDetection = false;


        Vector3 groundPos = transform.position;
        if (controller.isGrounded)
        {
            groundPos = transform.position + Vector3.up * 0.5f;
        }
        Vector3 centerPos = transform.position + Vector3.up; 

        Vector3 forward = transform.forward * 1;
        Vector3 forwardLeft = transform.forward * 1 - transform.right * 1;
        Vector3 forwardRight = transform.forward * 1 + transform.right * 1;

        Vector3[] raycastDirection = { forward, forwardLeft, forwardRight };

        for (int i = 0; i < raycastDirection.Length; i++)
        {
            bool center = Physics.Raycast(centerPos, raycastDirection[i], wallRayDistance, groundLayer);
            bool ground = Physics.Raycast(groundPos, raycastDirection[i], wallRayDistance, groundLayer);

            Debug.DrawRay(centerPos, raycastDirection[i] * wallRayDistance, Color.green);
            Debug.DrawRay(groundPos, raycastDirection[i] * wallRayDistance, Color.green);

            if(center || ground)
            {
                wallDetection = true;

                Debug.DrawRay(centerPos, raycastDirection[i] * wallRayDistance, Color.red);
                Debug.DrawRay(groundPos, raycastDirection[i] * wallRayDistance, Color.red);
            }
        }

        controller.wallCollision = wallDetection;
    }

    private void OnDrawGizmosSelected()
    {
        Color green = new Color(0, 1, 0, 0.35f);
        Color red = new Color(1, 0, 0, 0.35f);

        if(controller != null)
        {
            if (controller.isGrounded)
            {
                Gizmos.color = green;
            }
            else
            {
                Gizmos.color = red;
            }

            Gizmos.DrawSphere(transform.position, groundCheckRadius);
        }
    }
}
