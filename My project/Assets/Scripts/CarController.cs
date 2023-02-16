using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//using UnityEngine.UI;
using WebXR;

public class CarController : MonoBehaviour
{
    private float horizontalInput, verticalInput, accelInput, brakeInput;
    private float currentTorque, currentSteerAngle, currentbreakForce;
    private bool isHandBrake = true;
    private float steeringWheelRotation;
    private int gearIndex = 0;
    private Rigidbody rb;
    private Vector2 headRotation = Vector2.zero;
    [SerializeField] private float finalDriveRatio = 3;
    [SerializeField] private WebXRController leftController, rightController;
    [SerializeField] private int gearHapticDuration = 100;
    [SerializeField] private float gearHapticStrength = 0.08f;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject cameras;

    // UI
    [SerializeField] private TextMeshProUGUI speedometer;
    [SerializeField] private TextMeshProUGUI debug;

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
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUI();
        Park();

        
		headRotation.y += Input.GetAxis ("Mouse X");
		headRotation.x += -Input.GetAxis ("Mouse Y");
		cameras.transform.eulerAngles = headRotation * 3;
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

    public void OnHandBrakeStepValueChanged(float value) {
        //Debug.Log("HandBrakeStepValue: " + value);        
        leftController.Pulse(gearHapticStrength, gearHapticDuration);
        isHandBrake = ((int)value == 1 ? true : false);
    }

    // public void OnHandBrakeTargetValueReached(float value) {
    //     //Debug.Log("HandBrakeValue: " + value);
    //     if (value >= 0.55 && value <= 1) {
    //         isHandBrake = false;
    //     } else if (value >= 0 && value <= 0.45) {
    //         isHandBrake = true;
    //     }
    // }

    public void OnGearShifterStepValueChanged(float value) {
        //Debug.Log("GearShifterStepValue: " + value);
        if ((int)value-1 == gearIndex) return;
        
        leftController.Pulse(gearHapticStrength, gearHapticDuration);
        gearIndex = (int)value-1;
    }

    // public void OnGearShifterTargetValueReached(float value) {
    //     //Debug.Log("GearShifter: " + value);
    //     if (value >= 0.7 && value <= 1) {
    //         gearIndex = -1;
    //     } else if (value >= 0.35 && value <= 0.65) {
    //         gearIndex = 0;
    //     } else if (value >= 0 && value <= 0.3) {
    //         gearIndex = 1;
    //     }
    // }

    private void FixedUpdate() {
        HandleMotor();
        HandleSteering();
        if (animateWheels) UpdateWheels();
        GetInput();
    }

    private void GetInput() {
        // Acceleration Input
        if (rightController.GetButton(WebXRController.ButtonTypes.Trigger)) accelInput = rightController.GetAxis(WebXRController.AxisTypes.Trigger);
        else accelInput = Input.GetKey(KeyCode.W)? 1: 0;

        
        // Steering Input
        horizontalInput = Input.GetAxis("Horizontal");


        // public enum ButtonTypes {
        //     Trigger = 0,
        //     Grip = 1,
        //     Thumbstick = 2,
        //     Touchpad = 3,
        //     ButtonA = 4,
        //     ButtonB = 5
        // }
        if (Input.GetKeyDown(KeyCode.UpArrow)) gearIndex++;
        if (Input.GetKeyDown(KeyCode.DownArrow)) gearIndex--;
        if (Input.GetKeyDown(KeyCode.H)) isHandBrake = !isHandBrake;
        
        if (leftController.GetButton(WebXRController.ButtonTypes.Trigger)) brakeInput = leftController.GetAxis(WebXRController.AxisTypes.Trigger);
        else brakeInput = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    private void HandleMotor() {
        float wheelRpm = (frontLeftWheelCollider.rpm + frontRightWheelCollider.rpm) / 2;
        float motorRpm = motorTorque + (wheelRpm * finalDriveRatio * gearCurve.Evaluate(gearIndex));
        currentTorque = torqueCurve.Evaluate(motorRpm) * gearCurve.Evaluate(gearIndex) * finalDriveRatio * accelInput;
        
        if (!isHandBrake) {
            frontLeftWheelCollider.motorTorque = currentTorque/2;
            frontRightWheelCollider.motorTorque = currentTorque/2;
        }

        currentbreakForce = brakeInput * breakForce;
        ApplyBreaking();
    }

    private void ApplyBreaking() {
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering() {
        if (horizontalInput != 0) currentSteerAngle = maxSteeringAngle * horizontalInput;
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
        debug.text = string.Format("gear {0:N0}\naccel {1:N0}\nbrake {2:N0}", gearIndex, accelInput, brakeInput);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform) {
        Vector3 pos;
        Quaternion rot; 
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private void Park() {
        if (gearIndex == 0 && isHandBrake) gameManager.Park();
    }
}
