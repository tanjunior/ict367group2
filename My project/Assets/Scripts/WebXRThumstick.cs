using WebXR;
using Zinnia.Action;

public class WebXRThumstick : BooleanAction
{

    public WebXRController controller;

    // Update is called once per frame
    void Update()
    {
        Receive(controller.GetButton(WebXRController.ButtonTypes.Thumbstick));
    }
}
