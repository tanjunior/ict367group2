using WebXR;
using Zinnia.Action;

public class WebXRGrip : BooleanAction
{

    public WebXRController controller;

    // Update is called once per frame
    void Update()
    {
        Receive(controller.GetButton(WebXRController.ButtonTypes.Grip));
    }
}
