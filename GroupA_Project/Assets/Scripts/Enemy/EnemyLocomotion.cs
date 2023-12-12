using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class EnemyLocomotion : MonoBehaviour
{
    EnemyController controller;
    NavMeshAgent agent;


    [Header("Horizontal Movement Settings")]
    public float enemyAcceleration;
    public float rotationSpeed;
    public float stopDistance;


    public void GetLocomotionComponents()
    {
        controller = GetComponent<EnemyController>();
        agent = GetComponent<NavMeshAgent>();
    }
    public void SetLocomotionComponents()
    {
        agent.speed = 0;
        agent.acceleration = enemyAcceleration;
        agent.stoppingDistance = stopDistance;

        agent.updateRotation = false;
    }

    public void HandleEnemyRotation(float delta)
    {
        Vector3 targetDir = agent.velocity.normalized;
        if (targetDir == Vector3.zero) targetDir = transform.forward;

        Quaternion lookRotation = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * delta);

        transform.rotation = targetRotation;
    }

    public void SetAgentDestination(Vector3 newAgentDestination)
    {
        agent.destination = newAgentDestination;
    }
    public void SetAgentSpeed(float newTargetSpeed)
    {
        agent.speed = newTargetSpeed;
    }

    public bool ArrivesToTarget()

    {
        Vector3 enemyPosition = transform.position;
        enemyPosition.y = 0;
        Vector3 agentPosition = agent.destination;
        agentPosition.y = 0;

        float distance = Vector3.Distance(enemyPosition, agentPosition);

        if (Mathf.Abs(distance) < 1f)
        {
            return true;
        }

        return false;
    }
}
