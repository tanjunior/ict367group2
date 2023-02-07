using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatePos : MonoBehaviour
{
    public GameObject car;
    public Vector3 offset;
    private GameObject driverHeadPos;
    // Start is called before the first frame update
    void Start()
    {
        driverHeadPos = car.transform.Find("DriverHeadPos").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = driverHeadPos.transform.position + offset;
        transform.rotation = driverHeadPos.transform.rotation;
    }
}