using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tilia.Interactions.Controllables.AngularDriver;
using WebXR;
using UnityEngine.XR.Management;
using Zinnia.Action;

public class ControllablesTest : MonoBehaviour
{
    [SerializeField] private WebXRController leftController;
    [SerializeField] private AngularDriveFacade handbrake, gearShifter, steeringWheel, testShifter;
    [SerializeField] private Camera mainCamera;

    // Settings
    [SerializeField] private float maxSteeringAngle, keyboardRotateRate = 0.005f;

    private Vector2 headRotation = Vector2.zero;
    private float horizontalInput;
    private bool isHandBrake = true;
    private float currentTorque, currentSteerAngle, currentbreakForce;
    private int gearIndex = 0;
    private WebXRManager xrManager;
    private XRGeneralSettings xrSettings; 
    public bool isVR;
    public bool showPointer = false;
    // Start is called before the first frame update
    void Start()
    {
        xrSettings = XRGeneralSettings.Instance;
        xrManager = WebXRManager.Instance;
        isVR = CheckVR();
    }

    // Update is called once per frame
    void Update()
    {
        isVR = CheckVR();
        if (leftController.GetButtonDown(WebXRController.ButtonTypes.ButtonB) || Input.GetKeyDown(KeyCode.Escape)) {
            showPointer = !showPointer;
        }
        if (!isVR) {
            GetPcInput();
            Cursor.lockState = CursorLockMode.Locked;
            MouseLook();
        } else if (isVR) {
            Cursor.lockState = CursorLockMode.None;
        }
	}

    private void FixedUpdate() {
        HandleSteering();
    }

    private bool CheckVR() {
        if (Application.isEditor) {
            if (xrSettings.Manager.activeLoader != null) return true;
            else return false;
        }
        return xrManager.XRState == WebXRState.VR;
    }

    private void MouseLook() {
        headRotation.y += Input.GetAxis ("Mouse X");
		headRotation.x += -Input.GetAxis ("Mouse Y");

        headRotation.x = Mathf.Clamp(headRotation.x, -30, 30);

        mainCamera.transform.localEulerAngles = headRotation * 3;
    }

    public void OnSteeringWheelValueChanged(float steeringWheelValue) {
        if (isVR) {
            currentSteerAngle =  steeringWheelValue * maxSteeringAngle / 486;
        }
    }

    public void OnHandBrakeStepValueChanged(float value) {
        if (isVR) {       
            //leftController.Pulse(gearHapticStrength, gearHapticDuration);
            isHandBrake = ((int)value == 1 ? true : false);
        }
    }

    public void OnGearShifterStepValueChanged(float value) {
        if (isVR) {
            if ((int)value-1 == gearIndex) return;
            //leftController.Pulse(gearHapticStrength, gearHapticDuration);
            gearIndex = (int)value-1;
        }        
    }

    private void GetPcInput() {
        //accelInput = Input.GetButton("Accelerate") ? 1: 0;
        //if (Input.GetKey(KeyCode.Space)) {
            //brakeSound.Play();
            //brakeInput = 1;
        //} else brakeInput = 0;
        //horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetButton("Turn Left")) horizontalInput -= keyboardRotateRate;
        if (Input.GetButton("Turn Right")) horizontalInput += keyboardRotateRate;
        horizontalInput = Mathf.Clamp(horizontalInput, -1f, 1f);
        if (Input.GetButtonDown("Shift Up")) if (gearIndex != 1) gearIndex++;
        if (Input.GetButtonDown("Shift Down")) if (gearIndex != -1) gearIndex--;
        if (Input.GetButtonDown("Hand Brake")) isHandBrake = !isHandBrake;

        if (Input.GetKeyDown(KeyCode.Alpha1)) gearIndex = -1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) gearIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha3)) gearIndex = 1;

        // if (Input.GetMouseButtonDown(0) && gearIndex != 1) gearIndex++;
        // if (Input.GetMouseButtonDown(1) && gearIndex != -1) gearIndex--;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f) gearIndex++;
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) gearIndex--;

        steeringWheel.MoveToTargetValue = true;
        handbrake.MoveToTargetValue = true;
        gearShifter.MoveToTargetValue = true;
        testShifter.MoveToTargetValue = true;

        float steeringWheelValue = ((486 / maxSteeringAngle) * currentSteerAngle) / 972;
        steeringWheel.TargetValue = steeringWheelValue + 0.5f;
        handbrake.TargetValue = isHandBrake ? 1 : 0;
        gearShifter.TargetValue = gearIndex == 0 ? gearIndex + 0.5f : gearIndex;
        testShifter.TargetValue = gearIndex == 0 ? gearIndex + 0.5f : gearIndex;
    }

    private void HandleSteering() {
        if (!isVR) currentSteerAngle = maxSteeringAngle * horizontalInput;
    }

    public void ResetSteeringWheel() {
        steeringWheel.TargetValue = 0.5f;
        steeringWheel.MoveToTargetValue = true; 
        horizontalInput = 0;

    }

    public void ResetHandBrake() {
        handbrake.TargetValue = 1;       
        handbrake.MoveToTargetValue = true;
        isHandBrake = true;
    }

    public void ResetGearShifter() {
        gearShifter.TargetValue = 0.5f;
        gearShifter.MoveToTargetValue = true;
        testShifter.TargetValue = 0.5f;
        testShifter.MoveToTargetValue = true;
        gearIndex = 0;
    }

    // public void Reset() {
    //     StartCoroutine(Delay(0.5f));
    // }

    // IEnumerator Delay(float seconds)
    // {
    //     yield return new WaitForSeconds(seconds);
    //     steeringWheel.MoveToTargetValue = false;
    // }

    // public void OnSteeringWheelTargetValueReached(float value) {
    //     if (!isVR) return;
    //     if (value != 0.5f) {
    //         steeringWheel.TargetValue = 0.5f;
    //         return;
    //     }
    //     steeringWheel.MoveToTargetValue = false;
    // }

    public void OnGearShifterTargetValueReached() {
        if (!isVR) return;
        gearShifter.MoveToTargetValue = false;
    }

    // public void OnHandbrakeTargetValueReached() {
    //     if (!isVR) return;
    //     handbrake.MoveToTargetValue = false;
        
    // }
}
