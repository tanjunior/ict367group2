using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public bool control;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (control) transform.Translate(Input.GetAxis("Horizontal")*Time.deltaTime, 0f, Input.GetAxis("Vertical")*Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("from: " + transform.gameObject.name + " OnTriggerEnter: "+other.transform.gameObject.name);
    }

    private void OnCollisionEnter(Collision other) {
        Debug.Log("from: " + transform.gameObject.name + " OnCollisionEnter: "+other.transform.gameObject.name);
    }
}
