using WebXR;
using Zinnia.Action;

public class WebXRControllerFloatListener : FloatAction
{

    public WebXRController controller;
    public WebXRController.AxisTypes axis;

    // Update is called once per frame
    void Update()
    {
        Receive(controller.GetAxis(axis));
    }
}
