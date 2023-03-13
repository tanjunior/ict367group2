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
        last = other.transform.gameObject.name;
        //Debug.Log("from: " + transform.gameObject.name + " OnTriggerEnter: "+other.transform.gameObject.name);
    }

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
        bool isValid = last == positionString;
        last = null;
        Debug.Log(positionString + " " + isValid);
        return isValid;
    }
}
