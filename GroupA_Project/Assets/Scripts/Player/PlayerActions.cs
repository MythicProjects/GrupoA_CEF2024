using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    private PlayerController controller;

    private RobotController robot;

    [Header("Aim Settings")]
    public float cameraRotationSpeed;
    public float maxRotationAngle;
    public Transform aimTargetRot;
    public GameObject aimCamera;
    public float rayCameraDistance = 30;
    public LayerMask hitLayer;

    public Transform testobj;
    public void GetPlayerActionsComponents()
    {
        controller = GetComponent<PlayerController>();
        robot = FindObjectOfType<RobotController>();
    }

    public void SetPlayerLocomotion()
    {

    }

    //Aim
    public void SetAimCamera(bool setCamera)
    {
        aimCamera.SetActive(setCamera);
    }

    public void AimControl(float delta)
    {
        AimCameraRotation(delta);

        RaycastHit hit;
        bool wallHit = Physics.Raycast(aimTargetRot.position, aimTargetRot.forward, out hit, rayCameraDistance, hitLayer);
        Debug.DrawRay(aimTargetRot.position, aimTargetRot.forward * rayCameraDistance, Color.red);


        if (wallHit && controller.asignPointInput)
        {
            robot.SetRobotTarget(hit.point, hit.normal);
        }
        
        testobj.position = hit.point;//delete
    }

    private void AimCameraRotation(float delta)
    {
        Vector2 rotCamera = aimTargetRot.eulerAngles;
        rotCamera.x -= controller.aimAxisInput.y * cameraRotationSpeed * delta;

        if (rotCamera.x > 180) rotCamera.x -= 360;
        rotCamera.x = Mathf.Clamp(rotCamera.x, -maxRotationAngle, maxRotationAngle);
        rotCamera.y = transform.rotation.y;


        aimTargetRot.localRotation = Quaternion.Euler(rotCamera);
    }
}
