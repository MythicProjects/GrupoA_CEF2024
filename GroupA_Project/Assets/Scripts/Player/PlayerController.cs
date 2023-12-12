using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.CinemachineFreeLook;

public class PlayerController : MonoBehaviour
{
    private PlayerLocomotion locomotion;
    private PlayerActions actions;
    private RobotController robot;

    [Header("Flags")]
    public bool stopMovement;
    public bool isGrounded;
    public bool wallCollision;
    public bool isAiming; //Also Input

    [Header("Movement Inputs")]
    public float hAxisInput, vAxisInput;
    public float axisInputAmount;
    [Header("Action Inputs")]
    public bool sprintInput;
    public bool jumpInput;
    [Header("Aim Inputs")]
    public bool asignPointInput;
    private bool aimInput;
    public Vector2 aimAxisInput;

    private void Awake() 
    {
        locomotion = GetComponent<PlayerLocomotion>();
        actions = GetComponent<PlayerActions>();
        robot = GetComponent<RobotController>();

        locomotion.GetPlayerLocomotionComponents();
        actions.GetPlayerActionsComponents();
    }

    private void Start()
    {
        locomotion.SetPlayerLocomotion();
    }

    private void Update()
    {
        float delta = Time.deltaTime; 

        InputController();



        //Aim
        if (aimInput) actions.SetAimCamera(isAiming);

        if (isAiming)
        {
            //Select Robot
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                robot.SelectRobot();
            }

            locomotion.HandleAimRotation(delta); //Utilizar en FixedUpdate???
            actions.AimControl(delta);
        }

        //Locomotion
        locomotion.HandleJump();
    }

    private void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;
        
        locomotion.HandleWallCollision();
        locomotion.HandleGroundCollision(delta);
        locomotion.HandleGravity(delta);
        locomotion.HandleMovement(delta);

        if (!isAiming)
            locomotion.HandleRotation(delta);
        //else
        //locomotion.HandleAimRotation(delta); //Utilizar en Update???
    }

    //----Input Controller----

    private void InputController()
    {
        MovementInputs();
        ActionInputs();
        AimingInput();
    }

    private void MovementInputs() 
    {
        hAxisInput = Input.GetAxis("Horizontal");
        vAxisInput = Input.GetAxis("Vertical");

        axisInputAmount = Mathf.Clamp01(Mathf.Abs(hAxisInput) + Mathf.Abs(vAxisInput));
    }

    private void AimingInput()
    {
        asignPointInput = Input.GetKeyDown(KeyCode.Mouse0);
        aimInput = Input.GetKeyDown(KeyCode.Mouse1);
        isAiming = aimInput ? !isAiming : isAiming ;

        float aimAxisX = Input.GetAxis("Mouse X");
        float aimAxisY = Input.GetAxis("Mouse Y");
        aimAxisInput = new Vector2(aimAxisX, aimAxisY);
    }

    private void ActionInputs()
    {
        jumpInput = Input.GetButtonDown("Jump");

        sprintInput = Input.GetKey(KeyCode.LeftShift);


    }
}
