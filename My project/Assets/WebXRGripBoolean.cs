using Zinnia.Action;
using WebXR;

public class WebXRGripBoolean : BooleanAction
{
    public WebXRController controller;

    void Update()
    {
        Receive(controller.GetButton(WebXRController.ButtonTypes.Grip));
    }
}
