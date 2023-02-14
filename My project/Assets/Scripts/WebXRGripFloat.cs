using WebXR;
using Zinnia.Action;

public class WebXRGripFloat : FloatAction
{

    public WebXRController controller;

    // Update is called once per frame
    void Update()
    {
        Receive(controller.GetAxis(WebXRController.AxisTypes.Grip));
    }
}
