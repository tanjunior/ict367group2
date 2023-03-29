using UnityEngine;
using Zinnia.Action;

public class PointerState : BooleanAction
{
    [SerializeField] private LevelManager levelManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool showPointer = levelManager.showPointer;
        Receive(showPointer);
    }
}
