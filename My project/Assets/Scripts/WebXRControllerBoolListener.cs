using WebXR;
using Zinnia.Action;

public class WebXRControllerBoolListener : BooleanAction
{

    public WebXRController controller;
    public WebXRController.ButtonTypes button;

    // Update is called once per frame
    void Update()
    {
        Receive(controller.GetButton(button));
    }
}
