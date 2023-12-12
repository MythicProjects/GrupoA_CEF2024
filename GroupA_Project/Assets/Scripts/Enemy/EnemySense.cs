using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySense : MonoBehaviour
{
    EnemyController controller;

    [Header("Sense Settings")]
    public Transform sensePoint;
    public LayerMask hitLayers;
    public bool detectPlayer;
    public bool chasePlayer;

    [Header("Sight Settings")]
    public float sightDistance;
    public float viewAngle;
    public int sightRaysNumber;
    

    [Header("Hearing Settings")]
    public float hearingRadius;

    [Header("Player Data")]
    private Transform playerTransform;
    public LayerMask playerLayer;
    public Vector3 targetToPlayer;
    public float searchForPlayer;
    private float searchTimer;


    public void GetSenseComponents()
    {
        controller = GetComponent<EnemyController>();
    }
    public void SetSenseComponents()
    {

    }
    public void DetectPlayer(float delta)
    {

        UpdateSight();
        UpdateHearing();

        if (playerTransform == null)
            return;

        Vector3 playerTarget;

        if (detectPlayer)
        {
            targetToPlayer = playerTransform.position;

            playerTarget = (playerTransform.position + Vector3.up) - sensePoint.position;
            playerTarget.Normalize();


            RaycastHit hit;
            bool hitObstacle = Physics.Raycast(sensePoint.position, playerTarget, out hit, sightDistance, hitLayers);
            Debug.DrawRay(sensePoint.position, playerTarget * sightDistance, Color.red);

            if (Vector3.Angle(sensePoint.forward, playerTarget) > viewAngle * 2 || (hitObstacle && hit.collider.tag != "Player"))
            {
                detectPlayer = false;
            }

            chasePlayer = true;
        }
        else
        {
            searchTimer += delta;

            if(searchTimer > searchForPlayer)
            {
                chasePlayer = false;
                searchTimer = 0;
            }
        }

        controller.playerDetected = chasePlayer;
    }

    private void UpdateSight()
    {
        if (!detectPlayer)
        {
            RaycastHit hit;

            float stepAngleDeg = 360 / sightRaysNumber;

            //Add central raycast??

            for (int i = 0; i < sightRaysNumber; i++)
            {
                Vector3 localRotation = new Vector3(0, 0, i * stepAngleDeg);
                Vector3 direction = (Quaternion.Euler(localRotation) * sensePoint.up).normalized;

                bool hitPlayer = Physics.Raycast(sensePoint.position, sensePoint.forward * sightDistance + (direction * viewAngle), out hit, sightDistance, playerLayer);
                Debug.DrawRay(sensePoint.position, sensePoint.forward * sightDistance + (direction * viewAngle), Color.blue);
                
                if (hitPlayer)
                {
                    if (playerTransform == null) playerTransform = hit.collider.GetComponent<Transform>();
                    detectPlayer = true;
                }
            }
        }
    }

    private void UpdateHearing()
    {
        if (!detectPlayer)
        {
            Collider[] colliders = Physics.OverlapSphere(sensePoint.position, hearingRadius);

            foreach (Collider collider in colliders)
            {
                
                //Comprobar si hace ruido algo para activarlo

                if (collider.tag == "Player")
                {
                    if (playerTransform == null) playerTransform = collider.GetComponent<Transform>();
                    detectPlayer = true;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawSphere(sensePoint.position, hearingRadius);

    }
}
