using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingAccuracy : MonoBehaviour
{

    public BoxCollider carTrigger;
    public BoxCollider parkingTrigger;

    private float accuracyPercentage, parkedAccuracyPercentage;

    private void Start()
    {
        

        
    }


    private void OnTriggerEnter(Collider other)
    {

    }

    // calculate the overlap volume of two box colliders
    private float CalculateOverlapVolume(BoxCollider box1, BoxCollider box2)
    {
        Vector3 overlapSize = Vector3.Min(box1.bounds.max, box2.bounds.max) - Vector3.Max(box1.bounds.min, box2.bounds.min);
        overlapSize = Vector3.Max(overlapSize, Vector3.zero);
        return overlapSize.x * overlapSize.y * overlapSize.z;
    }




    private void Update()
    {
        if (parkingTrigger == null)
            parkingTrigger = GameObject.FindGameObjectWithTag("ParkingSpot").GetComponent<BoxCollider>();

        float overlapVolume = CalculateOverlapVolume(carTrigger, parkingTrigger);
        float carVolume = carTrigger.bounds.size.x * carTrigger.bounds.size.z * carTrigger.bounds.size.y;
        accuracyPercentage = overlapVolume / carVolume * 100f;



        Debug.Log("Parking accuracy: " + accuracyPercentage.ToString("F2") + "%");
    }

    public void setAccuracy()
    {
        parkedAccuracyPercentage = accuracyPercentage;
    }

    public float getAccuracyPercentage()
    {
        return parkedAccuracyPercentage;
    }

    public void resetAccuracyPercentage()
    {
        parkedAccuracyPercentage = 0;
    }
}
