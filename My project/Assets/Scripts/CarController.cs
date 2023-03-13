﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WebXR;
using UnityEngine.XR.Management;
using Tilia.Interactions.Controllables.AngularDriver;

public class CarController : MonoBehaviour
{
    private float horizontalInput = 0, accelInput, brakeInput;
    private float currentTorque, currentSteerAngle, currentbreakForce;
    private bool isHandBrake = true;
    private int gearIndex = 0;
    private WebXRState state = WebXRState.NORMAL;
    [SerializeField] private Rigidbody rb;

    // devmode
    [SerializeField] private bool enableVRControlsInEditor = true;
    [SerializeField] private TextMeshProUGUI debug;

    // others
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private AudioSource engineSound;
    [SerializeField] private AngularDriveFacade handbrake, gearShifter, steeringWheel;

    // WebXR
    [SerializeField] private WebXRController leftController, rightController;

    // UI
    [SerializeField] private TextMeshProUGUI speedometer;

    // Settings
    [SerializeField] private float motorTorque, breakForce, maxSteeringAngle, keyboardRotateRate = 0.005f;
    [SerializeField] private bool animateWheels;
    [SerializeField] private AnimationCurve torqueCurve, gearCurve, engineSoundCurve;
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
        XRGeneralSettings instance = XRGeneralSettings.Instance;
        if (instance.Manager.activeLoader == null) enableVRControlsInEditor = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (levelManager.isPaused) return;
        if (Application.isEditor) {
            if (enableVRControlsInEditor) {
                Cursor.lockState = CursorLockMode.None;
                GetVrInput();
            }
            else {
                GetPcInput();
            }
        } else if (levelManager.state == WebXRState.NORMAL) {
            GetPcInput();
        } else if (levelManager.state == WebXRState.VR) {
            GetVrInput();
        }
        UpdateUI();
	}

    private void FixedUpdate() {
        if (levelManager.isPaused) return;
        Park();
        HandleMotor();
        HandleSteering();
        if (animateWheels) UpdateWheels();
    }

    public void OnSteeringWheelValueChanged(float steeringWheelValue) {
        if (levelManager.isPaused) return;
        if (levelManager.state == WebXRState.VR || enableVRControlsInEditor) {
            currentSteerAngle =  steeringWheelValue * maxSteeringAngle / 486;
        }
    }

    public void OnHandBrakeStepValueChanged(float value) {
        if (levelManager.isPaused) return;
        if (levelManager.state == WebXRState.VR || enableVRControlsInEditor) {       
            leftController.Pulse(gearHapticStrength, gearHapticDuration);
            isHandBrake = ((int)value == 1 ? true : false);
        }
    }

    public void OnGearShifterStepValueChanged(float value) {
        if (levelManager.isPaused) return;
        if (levelManager.state == WebXRState.VR || enableVRControlsInEditor) {
            if ((int)value-1 == gearIndex) return;
            leftController.Pulse(gearHapticStrength, gearHapticDuration);
            gearIndex = (int)value-1;
        }        
    }

    private void GetPcInput() {
        accelInput = Input.GetButton("Accelerate") ? 1: 0;
        brakeInput = Input.GetKey(KeyCode.Space) ? 1 : 0;
        //horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetButton("Turn Left")) horizontalInput -= keyboardRotateRate *1f;
        if (Input.GetButton("Turn Right")) horizontalInput += keyboardRotateRate *1f;
        horizontalInput = Mathf.Clamp(horizontalInput, -1f, 1f);
        if (Input.GetButtonDown("Shift Up")) if (gearIndex != 1) gearIndex++;
        if (Input.GetButtonDown("Shift Down")) if (gearIndex != -1) gearIndex--;
        if (Input.GetButtonDown("Hand Brake")) isHandBrake = !isHandBrake;

        if (Input.GetKeyDown(KeyCode.Alpha1)) gearIndex = -1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) gearIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha3)) gearIndex = 1;

        if (Input.GetMouseButtonDown(0) && gearIndex != 1) gearIndex++;
        if (Input.GetMouseButtonDown(1) && gearIndex != -1) gearIndex--;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f) gearIndex++;
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) gearIndex--;

        steeringWheel.MoveToTargetValue = true;
        handbrake.MoveToTargetValue = true;
        gearShifter.MoveToTargetValue = true;
        float steeringWheelValue = ((486 / maxSteeringAngle) * currentSteerAngle) / 972;
        steeringWheel.TargetValue = steeringWheelValue + 0.5f;
        handbrake.TargetValue = isHandBrake ? 1 : 0;
        gearShifter.TargetValue = gearIndex == 0 ? gearIndex + 0.5f : gearIndex;
    }

    private void GetVrInput() {
        accelInput = rightController.GetAxis(WebXRController.AxisTypes.Trigger);
        brakeInput = leftController.GetAxis(WebXRController.AxisTypes.Trigger);
    }

    private void HandleMotor() {
        float wheelRpm = (frontLeftWheelCollider.rpm + frontRightWheelCollider.rpm) / 2;
        float motorRpm = motorTorque + (wheelRpm * finalDriveRatio * gearCurve.Evaluate(gearIndex));
        
        currentTorque = torqueCurve.Evaluate(motorRpm) * gearCurve.Evaluate(gearIndex) * finalDriveRatio * accelInput;

        float rpmForAudio = Mathf.Clamp(motorRpm * accelInput, -1000, 1000);
        float pitch = engineSoundCurve.Evaluate(rpmForAudio);
        engineSound.pitch = pitch;
        
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
        if (levelManager.state == WebXRState.NORMAL) currentSteerAngle = maxSteeringAngle * horizontalInput;
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

    public void Reset() {
        steeringWheel.TargetValue = 0.5f;
        steeringWheel.MoveToTargetValue = true; 
        gearShifter.TargetValue = 0.5f;
        gearShifter.MoveToTargetValue = true;
        handbrake.TargetValue = 1;       
        handbrake.MoveToTargetValue = true;
        StartCoroutine(Delay(0.5f));
        gearIndex = 0;
        horizontalInput = 0;
        isHandBrake = true;
    }

    IEnumerator Delay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        steeringWheel.MoveToTargetValue = false;
    }

    public void OnSteeringWheelTargetValueReached(float value) {
        if (levelManager.state == WebXRState.NORMAL) return;
        if (value != 0.5f) {
            steeringWheel.TargetValue = 0.5f;
            return;
        }
        steeringWheel.MoveToTargetValue = false;
    }

    public void OnGearShifterTargetValueReached() {
        if (levelManager.state == WebXRState.NORMAL) return;
        gearShifter.MoveToTargetValue = false;
    }

    public void OnHandbrakeTargetValueReached() {
        if (levelManager.state == WebXRState.NORMAL) return;
        handbrake.MoveToTargetValue = false;
        
    }
}
