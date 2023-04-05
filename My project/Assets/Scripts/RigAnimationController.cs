using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigAnimationController : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCamera() {
        //Debug.Log("SetCamera");
        
        mainCamera.localEulerAngles = Vector3.zero;
        mainCamera.Rotate(Vector3.zero, Space.World);
    }
}
