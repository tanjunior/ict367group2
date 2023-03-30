using WebXR;
using Zinnia.Action;

public enum actions {
    button,
    buttonDown,
    buttonUp,
    buttonTouched
};


public class WebXRControllerBoolListener : BooleanAction
{

    public WebXRController controller;
    public actions actionType = new actions();
    public WebXRController.ButtonTypes button;
    private bool action;

    // Update is called once per frame
    void Update()
    {
        if (actionType == actions.button) action = controller.GetButton(button);
        if (actionType == actions.buttonDown) action = controller.GetButtonDown(button);
        if (actionType == actions.buttonUp) action = controller.GetButtonUp(button);
        if (actionType == actions.buttonTouched) action = controller.GetButtonTouched(button);
        Receive(action);
    }
}
