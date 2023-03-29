using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;
using WebXR;
using TMPro;

public class VRState : MonoBehaviour
{
    
    //public WebXRState state;
    public TextMeshPro text;
    public WebXRManager xrManager;
    public XRGeneralSettings xrSettings;
    // Start is called before the first frame update
    void Start()
    {
        xrSettings = XRGeneralSettings.Instance;
        //isOculus = instance.Manager.activeLoader != null;
        xrManager = WebXRManager.Instance;
        //state = xrManager.XRState;
        //isVR = (xrManager.isSupportedVR || isOculus);
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = string.Format("isEditor: {0}\nXRState: {1}\nactiveLoader: {2}\nisVR:{3}",Application.isEditor, xrManager.XRState, xrSettings.Manager.activeLoader, CheckVR());
    }

    private bool CheckVR() {
        if (Application.isEditor) {
            if (xrSettings.Manager.activeLoader != null) return true;
            else return false;
        }
        return xrManager.XRState == WebXRState.VR;
    }
}
