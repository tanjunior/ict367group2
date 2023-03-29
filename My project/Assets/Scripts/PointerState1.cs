using UnityEngine;
using Zinnia.Action;

public class PointerState1 : BooleanAction
{
    [SerializeField] private ControllablesTest controllablesTest;

    // Update is called once per frame
    void Update()
    {
        //if (!levelManager.isVR) return;
        bool showPointer = controllablesTest.showPointer;
        Receive(showPointer);
    }
}
