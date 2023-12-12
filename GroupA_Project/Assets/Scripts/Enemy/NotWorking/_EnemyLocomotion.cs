using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class _EnemyLocomotion : MonoBehaviour
{
    EnemyController controller;
    Rigidbody rb;

    public List<Vector3> waypoint = new List<Vector3>();
    private Vector3 currentTarget;
    private int currentWaypoint;

    [Header("Movement")]
    [SerializeField] float actualSpeed = 10;
    [SerializeField] Vector3 moveDirection;
    [SerializeField] float rotationSpeed = 20;


    [Header("Vertical Movement Settings")]
    public float fallingSpeed = -30;
    private float verticalSpeed;

    [Header("Ground Collision Settings")]
    public LayerMask groundLayer;
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] float groundRayHeight = 1.5f;

    [Header("Step Settings")]
    [SerializeField] float stepCheckDistance = 0.4f;
    [SerializeField] float groundStepHeight = 0.3f;
    [SerializeField] float groundStepSpeed = 45;

    [Header("Wall Collision Settings")]
    [SerializeField] float wallCheckRayDistance = 0.6f;
    [SerializeField] float wallAngle = 0.2f;

    public void GetLocomotionComponents()
    {
        controller = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody>();
    }
    public void SetLocomotionComponents()
    {
    }
    public void HandleEnemyMovement(float delta)
    {
        if (waypoint.Count < 1)
            return;

        if (Vector3.Distance(transform.position, waypoint[currentWaypoint]) < 0.3f)
        {
            if (currentWaypoint < waypoint.Count-1)
            {
                currentWaypoint++;
            }
        }

        moveDirection = waypoint[currentWaypoint] - transform.position;

        moveDirection.Normalize();
        moveDirection.y = 0;

        //Y velocity
        Vector3 verticalVelocity = Vector3.up * verticalSpeed;
        //X and Z velocity
        Vector3 horizontalVelocity = moveDirection * actualSpeed;
        Debug.Log(horizontalVelocity);

        //All velocity
        Vector3 movementVelocity = horizontalVelocity + verticalVelocity;


        rb.position += movementVelocity * delta;


    }

    public void HandleEnemyRotation(float delta)
    {
        Vector3 targetDir = moveDirection;
        if (targetDir == Vector3.zero) targetDir = transform.forward;

        Quaternion lookRotation = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * delta);

        rb.rotation = targetRotation;
    }
    public void HandleEnemyGravity(float delta)
    {

        if (controller.isGrounded && verticalSpeed < 0.1f)
        {
            verticalSpeed = 0;
        }
        else if (!controller.isGrounded)
        {
            verticalSpeed += fallingSpeed * delta;
        }

        verticalSpeed = Mathf.Clamp(verticalSpeed, fallingSpeed, Mathf.Abs(fallingSpeed));
    }

    public void SetMovementDestination(Vector3 newAgentDestination, float newTargetSpeed)
    {
        waypoint.Clear();

        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, newAgentDestination, NavMesh.AllAreas, path);

        // Move along the path
        for (int i = 1; i < path.corners.Length; i++)
        {
            waypoint.Add(path.corners[i]);
        }

        currentTarget = newAgentDestination;
        actualSpeed = newTargetSpeed;
        currentWaypoint = 0;
    }


    public bool EnemyArrivesToTarget()

        //Problema, el punto puede generarse dentro de la pared. Debe detectar colisiones
    {
        Vector3 enemyPosition = transform.position;
        enemyPosition.y = 0;
        Vector3 agentPosition = currentTarget;
        agentPosition.y = 0;

        float distance = Vector3.Distance(enemyPosition, agentPosition);
        
        if(Mathf.Abs(distance) < 1f)
        {
            return true;
        }

        return false;
    }

    //Detección de colisiones
    public void HandleGroundCollision(float delta)
    {
        Vector3 groundPos = transform.position;
        Vector3 centerPos = transform.position + Vector3.up;

        bool detectGround = Physics.CheckSphere(groundPos, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);


        controller.isGrounded = detectGround;

        if (detectGround)
        {
            RaycastHit hit;
            bool hitGround;

            Vector3 rayOrigin;
            if (moveDirection.magnitude > 0.1f)
            {
                rayOrigin = centerPos + transform.forward * 0.3f;
            }
            else
            {
                rayOrigin = centerPos;
            }

            hitGround = Physics.Raycast(rayOrigin, -Vector3.up, out hit, stepCheckDistance, groundLayer);
            Debug.DrawRay(rayOrigin, -Vector3.up * stepCheckDistance, Color.blue, 0, false);


            if (hitGround && verticalSpeed < 0.1f)
            {
                Vector3 targetPosition = transform.position;
                targetPosition.y = hit.point.y;

                float stepHeightDifference = hit.point.y - transform.position.y;

                if (Mathf.Abs(stepHeightDifference) < groundStepHeight)
                {
                    rb.position = Vector3.Slerp(transform.position, targetPosition, delta * groundStepSpeed);
                }
            }
        }
    }
    public void HandleWallsCollision()
    {
        Vector3 groundPos = transform.position;
        if (controller.isGrounded) groundPos = transform.position + Vector3.up * 0.5f;
        Vector3 centerPos = transform.position + Vector3.up;

        Vector3 forward = transform.forward * wallCheckRayDistance;
        Vector3 forwardLeft = transform.forward * wallCheckRayDistance - transform.right * wallAngle;
        Vector3 forwardRight = transform.forward * wallCheckRayDistance + transform.right * wallAngle;

        Vector3[] rayDirection = { forward, forwardLeft, forwardRight };

        bool wallRaycast = false;

        for (int i = 0; i < rayDirection.Length; i++)
        {
            bool center = Physics.Raycast(centerPos, rayDirection[i], wallCheckRayDistance, groundLayer);
            bool ground = Physics.Raycast(groundPos, rayDirection[i], wallCheckRayDistance, groundLayer);

            Debug.DrawRay(groundPos, rayDirection[i] * wallCheckRayDistance, Color.green, 0, false);
            Debug.DrawRay(centerPos, rayDirection[i] * wallCheckRayDistance, Color.green, 0, false);

            if (center || ground)
            {
                wallRaycast = true;
                Debug.DrawRay(groundPos, rayDirection[i] * wallCheckRayDistance, Color.red, 0, false);
                Debug.DrawRay(centerPos, rayDirection[i] * wallCheckRayDistance, Color.red, 0, false);
            }
        }

        if (wallRaycast)
        {
            controller.wallCollision = true;
        }
        else
        {
            controller.wallCollision = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (controller == null) Gizmos.color = transparentGreen;
        else
        {
            if (controller.isGrounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;
        }

        Vector3 spherePosition = transform.position;
        Gizmos.DrawSphere(spherePosition, groundCheckRadius);


        if (waypoint.Count > 0)
        {
            for (int i = 0; i < waypoint.Count; i++)
            {
                if(i != waypoint.Count - 1)
                {
                    Debug.DrawLine(waypoint[i], waypoint[i + 1], Color.red);
                }
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(waypoint[i], 0.75f);
            }
        }
    }
}
