using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WebXR;
using Tilia.Interactions.Controllables.AngularDriver;

public class CarController : MonoBehaviour
{
    private float horizontalInput = 0, accelInput, brakeInput;
    private float currentTorque, currentSteerAngle, currentbreakForce;
    private bool isHandBrake = true;
    private int gearIndex = 0;

    // devmode
    [SerializeField] private TextMeshProUGUI debug;

    // others
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private AudioSource engineSound, brakeSound;
    [SerializeField] private AngularDriveFacade handbrake, gearShifter, steeringWheel;
    [SerializeField] private Transform rearMirror;

    // WebXR
    [SerializeField] private WebXRController leftController, rightController;

    // UI
    [SerializeField] private TextMeshProUGUI speedometer;

    // Settings
    [SerializeField] private float motorTorque, breakForce, maxSteeringAngle, keyboardRotateRate = 0.005f;
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

    

    // Update is called once per frame
    void Update()
    {
        if (levelManager.isPaused || levelManager.firstStart) {
            engineSound.Pause();
            return;
        }
        engineSound.UnPause();
        
        if (levelManager.isVR) GetVrInput();
        else  GetPcInput();

        UpdateUI();
	}

    private void FixedUpdate() {
        if (levelManager.isPaused || levelManager.firstStart) return;
        Park();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    public void OnSteeringWheelValueChanged(float steeringWheelValue) {
        if (levelManager.isPaused) return;
        if (levelManager.isVR)  currentSteerAngle =  steeringWheelValue * maxSteeringAngle / 486;
    }

    public void OnHandBrakeStepValueChanged(float value) {
        if (levelManager.isPaused) return;
        if (levelManager.isVR) {       
            leftController.Pulse(gearHapticStrength, gearHapticDuration);
            isHandBrake = ((int)value == 1 ? true : false);
        }
    }

    public void OnGearShifterStepValueChanged(float value) {
        if (levelManager.isPaused) return;
        if (levelManager.isVR) {
            if ((int)value-1 == gearIndex) return;
            leftController.Pulse(gearHapticStrength, gearHapticDuration);
            gearIndex = (int)value-1;
        }        
    }

    private void GetPcInput() {
        accelInput = Input.GetButton("Accelerate") ? 1: 0;
        brakeInput = Input.GetKey(KeyCode.Space) ? 1: 0;
        if (Input.GetButton("Turn Left")) horizontalInput -= keyboardRotateRate;
        if (Input.GetButton("Turn Right")) horizontalInput += keyboardRotateRate;
        horizontalInput = Mathf.Clamp(horizontalInput, -1f, 1f);
        if (Input.GetButtonDown("Shift Up") && gearIndex != 1) gearIndex++;
        if (Input.GetButtonDown("Shift Down") && gearIndex != -1) gearIndex--;
        if (Input.GetButtonDown("Hand Brake")) isHandBrake = !isHandBrake;

        if (Input.GetKeyDown(KeyCode.Alpha1)) gearIndex = -1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) gearIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha3)) gearIndex = 1;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f) gearIndex++;
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) gearIndex--;

        rearMirror.Rotate(Vector3.up, Input.GetAxisRaw("Horizontal") * -12 * Time.deltaTime);
        rearMirror.Rotate(Vector3.right, Input.GetAxisRaw("Vertical") * -12 * Time.deltaTime);

        
        SetControllablesMove(true);
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
        frontLeftWheelCollider.motorTorque = currentTorque/2;
        frontRightWheelCollider.motorTorque = currentTorque/2;

        if (isHandBrake) currentbreakForce = 2000;
        else currentbreakForce = brakeInput * breakForce;

        ApplyBraking();
    }

    private void HandleSteering() {
        if (!levelManager.isVR) currentSteerAngle = maxSteeringAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void ApplyBraking() {
        if (brakeInput > 0) brakeSound.Play();
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void UpdateWheels() {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateUI() {
        speedometer.text = string.Format("torque {0:N0}\nspeed {1:N0}", currentTorque, rb.velocity.magnitude);
        if (gearIndex == 0) debug.text = string.Format("P");
        else if (gearIndex == 1) debug.text = string.Format("D");
        else debug.text = string.Format("R");
        //debug.text = string.Format("steering wheel {0:N0}\nwheel angle {1:N0}", steeringWheelRotation, currentSteerAngle);
        //debug.text = string.Format("gear {0:N0}\naccel {1:N0}\nbrake {2:N0}", gearIndex, accelInput, brakeInput);
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
        if (levelManager.isVR) {
            steeringWheel.TargetValue = 0.5f;
            gearShifter.TargetValue = 0.5f;
            handbrake.TargetValue = 1;
            SetControllablesMove(true);
            StartCoroutine(Delay(0.3f));
        }
        gearIndex = 0;
        horizontalInput = 0;
        isHandBrake = true;

        rb.isKinematic = true; //remove all forces from a rigid body
        currentbreakForce = Mathf.Infinity;
        rb.isKinematic = false;
    }

    public void SetControllablesMove(bool b) {
        steeringWheel.MoveToTargetValue = b;
        handbrake.MoveToTargetValue = b;
        gearShifter.MoveToTargetValue = b;
    }

    IEnumerator Delay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SetControllablesMove(false);
    }
}
