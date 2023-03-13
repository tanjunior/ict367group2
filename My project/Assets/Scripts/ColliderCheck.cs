using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderCheck : MonoBehaviour
{
    public int numberOfCollisions = 0;
    private float lastCollisionTime = 0f;

    void OnTriggerEnter(Collider c)
    {
        Debug.Log("number of name=" + c.gameObject.tag);

        if (c.gameObject.tag == "collider" && Time.time > lastCollisionTime + 4f)
        {
            numberOfCollisions++;
            Debug.Log("Number of collisions:" + numberOfCollisions);
            lastCollisionTime = Time.time;
        }
    }

    public int getNumberOfCollisions()
    {
        return numberOfCollisions;
    }

    public void resetNumberOfCollisions()
    {
        numberOfCollisions = 0;
    }
}
