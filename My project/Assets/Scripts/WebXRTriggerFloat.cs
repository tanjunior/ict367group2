using WebXR;
using Zinnia.Action;

public class WebXRTriggerFloat : FloatAction
{

    public WebXRController controller;

    // Update is called once per frame
    void Update()
    {
        Receive(controller.GetAxis(WebXRController.AxisTypes.Trigger));
    }
}
