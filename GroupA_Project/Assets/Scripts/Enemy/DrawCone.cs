using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

public class DrawCone : MonoBehaviour
{
    public float angleFromCenter;
    public float rayAngle;

    public float distance;

    void Update()
    {

        float stepAngleDeg = 360 / rayAngle; //angle between two sampled rays
        
        for (int i = 0; i < rayAngle; i++)
        {

            //AVISO> Ahora mismo se aplican angulos a Z, no funciona
            //Se debe tener en cuenta la rotación en Y y luego aplicar ese cálculo
            //Sería mejor utilizar Quaternions

            float angle = stepAngleDeg * i;

            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * angleFromCenter;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * angleFromCenter;

            Vector3 rayTargetDirection = (transform.right * x + transform.up * z) + transform.forward * distance;

            Debug.DrawRay(transform.position, rayTargetDirection, Color.red);
        }
    }
}
