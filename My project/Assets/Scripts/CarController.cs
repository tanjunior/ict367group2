using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WebXR;

public class CarController : MonoBehaviour
{
    private float horizontalInput, accelInput, brakeInput;
    private float currentTorque, currentSteerAngle, currentbreakForce;
    private bool isVR, mouseLook = false, isHandBrake = true;
    private int gearIndex = 0;
    private WebXRState state = WebXRState.NORMAL;
    private Rigidbody rb;

    // devmode
    [SerializeField] private bool enableVRControlsInEditor = false;
    [SerializeField] private TextMeshProUGUI debug;

    // others
    [SerializeField] private LevelManager levelManager;

    // WebXR
    [SerializeField] private WebXRController leftController, rightController;

    // UI
    [SerializeField] private TextMeshProUGUI speedometer;

    // Settings
    [SerializeField] private float motorTorque, breakForce, maxSteeringAngle;
    [SerializeField] private bool animateWheels;
    [SerializeField] private AnimationCurve torqueCurve, gearCurve;
    [SerializeField] private float finalDriveRatio = 3;
    [SerializeField] private int gearHapticDuration = 100;
    [SerializeField] private float gearHapticStrength = 0.08f;

    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;
    [SerializeField] private ParkingController FL, FR, RL, RR;

    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        isVR = WebXRManager.Instance.isSupportedVR;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (levelManager.isPaused) return;
        // public enum WebXRState { VR, AR, NORMAL }
        if (isVR) state = WebXRManager.Instance.XRState;
        if (Application.isEditor) {
            if (enableVRControlsInEditor) {
                Cursor.lockState = CursorLockMode.None;
                GetVrInput();
            }
            else {
                GetPcInput();
            }
        } else if (state == WebXRState.NORMAL) {
            GetPcInput();
        } else if (state == WebXRState.VR) {
            GetVrInput();
        }
        UpdateUI();
        Park();
	}

    private void FixedUpdate() {
        if (levelManager.isPaused) return;
        HandleMotor();
        HandleSteering();
        if (animateWheels) UpdateWheels();
    }

    public void OnSteeringWheelValueChanged(float steeringWheelValue) {
        if (levelManager.isPaused) return;
        if (state == WebXRState.VR || enableVRControlsInEditor) {
            if (steeringWheelValue > 486) {
                currentSteerAngle = maxSteeringAngle * (360 - steeringWheelValue) / 486;
            } else {
                currentSteerAngle = -maxSteeringAngle * steeringWheelValue / 486;
            }
        }
    }

    public void OnHandBrakeStepValueChanged(float value) {
        if (levelManager.isPaused) return;
        if (state == WebXRState.VR || enableVRControlsInEditor) {       
            leftController.Pulse(gearHapticStrength, gearHapticDuration);
            isHandBrake = ((int)value == 1 ? true : false);
        }
    }

    public void OnGearShifterStepValueChanged(float value) {
        if (levelManager.isPaused) return;
        if (state == WebXRState.VR || enableVRControlsInEditor) {
            if ((int)value-1 == gearIndex) return;
            leftController.Pulse(gearHapticStrength, gearHapticDuration);
            gearIndex = (int)value-1;
        }        
    }

    private void GetPcInput() {
        accelInput = Input.GetButton("Accelerate")? 1: 0;
        brakeInput = Input.GetKey(KeyCode.Space) ? 1 : 0;
        horizontalInput = Input.GetAxis("Horizontal");
        if (Input.GetButtonDown("Shift Up")) gearIndex++;
        if (Input.GetButtonDown("Shift Down")) gearIndex--;
        if (Input.GetButtonDown("Hand Brake")) isHandBrake = !isHandBrake;
    }

    private void GetVrInput() {
        accelInput = rightController.GetAxis(WebXRController.AxisTypes.Trigger);
        brakeInput = leftController.GetAxis(WebXRController.AxisTypes.Trigger);
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
        if (state == WebXRState.NORMAL && !enableVRControlsInEditor) currentSteerAngle = maxSteeringAngle * horizontalInput;
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
        if (gearIndex == 0 && isHandBrake) levelManager.Park(FL.GetValidation(), FR.GetValidation(), RL.GetValidation(), RR.GetValidation());
    }
}
