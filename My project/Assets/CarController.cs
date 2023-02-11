using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebXR;

public class CarController : MonoBehaviour
{
    private float horizontalInput, verticalInput;
    private float currentTorque, currentSteerAngle, currentbreakForce;
    private bool isBrake, isAccel, isShiftUp, isShiftDown;
    private float steeringWheelRotation;
    private Rigidbody rb;
    public float finalDriveRatio = 1;
    public int gearIndex = 1;
    public WebXRController leftController, rightController;

    // UI
    [SerializeField] private Text speedometer;
    [SerializeField] private Text debug;

    // Settings
    [SerializeField] private float motorTorque, breakForce, maxSteeringAngle;
    [SerializeField] private bool animateWheels;
    [SerializeField] private AnimationCurve torqueCurve, gearCurve;

    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;
    // Start is called before the first frame update
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        if (isShiftUp && gearIndex < 5) {
            gearIndex++;
        } else if (isShiftDown && gearIndex > -1) {
            gearIndex--;
        }
        UpdateUI();
    }

    public void OnSteeringWheelValueChanged(float steeringWheelValue) {
        steeringWheelRotation = steeringWheelValue;
        if (steeringWheelValue > 486)
        {
            currentSteerAngle = maxSteeringAngle * (360 - steeringWheelValue) / 486;
 
        } else {
            currentSteerAngle = -maxSteeringAngle * steeringWheelValue / 486;
        }
    }

    public void OnHandBrakeValueChanged(float HandBrakeValue) {
        Debug.Log("HandBrake: " + HandBrakeValue);
        //if (HandBrakeValue > ?) isBreaking = true;
    }

    private void FixedUpdate() {
        HandleMotor();
        HandleSteering();
        if (animateWheels) UpdateWheels();
    }

    private void GetInput() {
        // Steering Input
        horizontalInput = Input.GetAxis("Horizontal");

        // Acceleration Input
        verticalInput = Input.GetAxis("Vertical");

        // Breaking Input
        isBrake = Input.GetKey(KeyCode.Space);

        // public enum ButtonTypes {
        //     Trigger = 0,
        //     Grip = 1,
        //     Thumbstick = 2,
        //     Touchpad = 3,
        //     ButtonA = 4,
        //     ButtonB = 5
        // }
        isAccel = rightController.GetButton(WebXRController.ButtonTypes.ButtonB);
        isBrake = rightController.GetButton(WebXRController.ButtonTypes.ButtonA);

        isShiftUp = leftController.GetButtonDown(WebXRController.ButtonTypes.ButtonB);
        isShiftDown = leftController.GetButtonDown(WebXRController.ButtonTypes.ButtonA);
    }

    private void HandleMotor() {
        float wheelRpm = (frontLeftWheelCollider.rpm + frontRightWheelCollider.rpm) / 2;
        float motorRpm = motorTorque + (wheelRpm * finalDriveRatio * gearCurve.Evaluate(gearIndex));
        currentTorque = torqueCurve.Evaluate(motorRpm) * gearCurve.Evaluate(gearIndex) * (isAccel? 1 : 0);
        
        frontLeftWheelCollider.motorTorque = currentTorque/2;
        frontRightWheelCollider.motorTorque = currentTorque/2;

        currentbreakForce = isBrake ? breakForce : 0f;
        ApplyBreaking();
    }

    private void ApplyBreaking() {
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering() {
        //currentSteerAngle = maxSteeringAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels() {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateUI() {
        speedometer.text = string.Format("torque {0:N0}\nspeed {1:N0}", currentTorque, rb.velocity.magnitude);
        //debug.text = string.Format("steering wheel {0:N0}\nwheel angle {1:N0}", steeringWheelRotation, currentSteerAngle);
        debug.text = string.Format("gear {0:N0}\naccel {1:N0}\nbrake {2:N0}", gearIndex, isAccel, isBrake);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform) {
        Vector3 pos;
        Quaternion rot; 
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
}
