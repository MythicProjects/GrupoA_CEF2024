using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraVigilancia : MonoBehaviour
{
    [Header("Camera Construction")]
    public Transform cameraBase;
    public Transform cameraObject;
    public Transform raycastOrigin;
    public Light luzRoja;

    [Header("Detection Settings")]
    private Transform playerTransform;
    public float viewAngle;
    public float searchForPlayer;
    private float searchTimer;
    private bool searchPlayer = false;
    private bool isPlayerDetected = false;

    [Header("Camera Movement Settings")]
    public float cameraSpeed = 30f;
    public float minRotationTime = 2f;
    public float maxRotationTime = 5f;
    private float nextRotationTime;
    private float nextRotationTimer;
    public Vector3 targetPoint;
    Vector3 direction;

    [Header("Raycast Settings")]
    public int raysNumberCamera;
    public float maxDetectionDistance = 10f;
    public float angleFromCenter;


    private void Start()
    {
        direction = transform.forward;
    }
    void Update()
    {
        DetectPlayer();
        Rotation();
    }
    private void DetectPlayer()
    {
        SearchPlayer();

        if (playerTransform == null)
            return;


        if (isPlayerDetected)
        {
            searchPlayer = true;

            //GetRotation
            targetPoint = playerTransform.position;
            direction = (playerTransform.position + Vector3.up) - cameraObject.position;
            direction.Normalize();

            RaycastHit hit;
            bool hitObstacle = Physics.Raycast(raycastOrigin.position, direction, out hit, maxDetectionDistance);
            Debug.DrawRay(raycastOrigin.position, direction * maxDetectionDistance, Color.red);

            Debug.Log(Vector3.Angle(cameraBase.forward, direction));
            if (Vector3.Angle(cameraBase.forward, direction) > viewAngle || (hitObstacle && hit.collider.tag != "Player"))
            {
                isPlayerDetected = false;
                return;
            }

            luzRoja.enabled = true;
        }
        else
        {
            searchTimer += Time.deltaTime;

            if (searchTimer > searchForPlayer)
            {
                searchPlayer = false;
                SetNewRotationState();
                searchTimer = 0;
            }

            luzRoja.enabled = false;
        }
    }


    private void Rotation()
    {
        Quaternion targetRotation;

        float distance = Vector3.Distance(cameraObject.forward, direction);

        if (!searchPlayer)
        {
            if (Mathf.Abs(distance) < 0.1f)
            {
                nextRotationTimer += Time.deltaTime;

                if (nextRotationTimer > nextRotationTime)
                {
                    SetNewRotationState();
                }
                return;
            }
        }
        else
        {
            direction = targetPoint - cameraBase.position;
        }

        targetRotation = Quaternion.LookRotation(direction);
        cameraObject.transform.rotation = Quaternion.RotateTowards(cameraObject.transform.rotation, targetRotation, cameraSpeed * Time.deltaTime);
    }

    private void SetNewRotationState()
    {
        nextRotationTime = Random.Range(minRotationTime, maxRotationTime);
        Vector3 randomPoint = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 0f), 0);
        direction = cameraBase.forward + randomPoint;
        direction.Normalize();

        nextRotationTimer = 0;
    }

    private void SearchPlayer()
    {
        if(!isPlayerDetected)
        {
            RaycastHit hit;

            float stepAngleDeg = 360 / raysNumberCamera;

            for (int i = 0; i < raysNumberCamera; i++)
            {
                float angle = stepAngleDeg * i;

                float x = Mathf.Sin(Mathf.Deg2Rad * angle) * angleFromCenter;
                float z = Mathf.Cos(Mathf.Deg2Rad * angle) * angleFromCenter;

                Vector3 rayTargetDirection = (raycastOrigin.right * x + raycastOrigin.up * z) + raycastOrigin.forward * maxDetectionDistance;

                bool hitPlayer = Physics.Raycast(raycastOrigin.position, rayTargetDirection, out hit, maxDetectionDistance);
                Debug.DrawRay(raycastOrigin.position, rayTargetDirection, Color.red);

                if (hitPlayer && hit.collider.tag == "Player")
                {
                    if(playerTransform == null) playerTransform = hit.collider.GetComponent<Transform>();
                    isPlayerDetected = true;
                }
            }
        }
    }    
}
