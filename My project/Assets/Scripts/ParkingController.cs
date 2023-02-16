using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Position {
    front,
    rear
}

public class ParkingController : MonoBehaviour
{
    [SerializeField]Position position;
    private string last;

    private void OnTriggerEnter(Collider other) {
        //last = other.transform.gameObject.name;
        Debug.Log("OnTriggerEnter: "+other.transform.gameObject.name);
    }

    private void OnCollisionEnter(Collision other) {
        //last = other.transform.gameObject.name;
        Debug.Log("OnCollisionEnter: "+other.transform.gameObject.name);
    }

    // private void OnCollisionStay(Collision other) {
    //     last = other.transform.gameObject.name;
    //     Debug.Log("OnCollisionStay: "+other);
    // }

    // private void OnTriggerStay(Collider other) {
    //     last = other.transform.gameObject.name;
    //     Debug.Log("OnTriggerStay: "+other);
    // }

    public bool GetValidation() {
        string positionString;
        switch (position)
        {
            case Position.front:
                positionString = "front";
                break;
            case Position.rear:
                positionString = "rear";
                break;
            default:
                return false;
        }
        return (last == positionString);
    }
}
