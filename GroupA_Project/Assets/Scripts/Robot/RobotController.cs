using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Robot Settings")]
    public Transform robotObj;
    public RobotData[] robotData;
    private int robotCount;


    private Vector3 colisionTarget;
    private Vector3 colisionNormal;

    private void Update()
    {
        robotObj.position = transform.position + transform.right + Vector3.up;
    }

    public void SelectRobot()
    {
        robotCount++;

        if (robotCount > robotData.Length -1) robotCount = 0;
        UpdateRobot(robotCount);
    }

    private void UpdateRobot(int count)
    {
        foreach (Transform obj in robotObj)
        {
            Destroy(obj.gameObject);
        }

        GameObject robot = Instantiate(robotData[count].robotPrefab, robotObj);
    }

    public void SetRobotTarget (Vector3 targetPoint, Vector3 targetNormal)
    {
        colisionTarget = targetPoint;
        colisionNormal = targetNormal;

        Debug.Log(Vector3.Dot(Vector3.up, targetNormal));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(colisionTarget, 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(colisionTarget, colisionNormal * 2);
    }
}
