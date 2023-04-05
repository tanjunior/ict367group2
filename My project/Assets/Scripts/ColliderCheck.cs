using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderCheck : MonoBehaviour
{
    [SerializeField] LevelManager levelManager;
    [SerializeField] private AudioSource collisionSound;
    private float lastCollisionTime = 0f;
    public int numberOfCollisions = 0;

    void OnTriggerEnter(Collider c)
    {
        if (c.name == "Level1Trigger") {
            levelManager.LoadScene(1);
        } else if (c.name == "Level2Trigger") {
            levelManager.LoadScene(2);
        } else if (c.name == "Level3Trigger") {
            levelManager.LoadScene(3);
        } else if (c.name == "QuitGameTrigger") {
            levelManager.QuitGame();
        }

        if (c.gameObject.tag == "collider" && Time.time > lastCollisionTime + 4f)
        {
            numberOfCollisions++;
            Debug.Log("Number of collisions:" + numberOfCollisions);
            lastCollisionTime = Time.time;
            collisionSound.Play();
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
