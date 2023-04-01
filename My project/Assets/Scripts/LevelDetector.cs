using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDetector : MonoBehaviour
{
    public GameObject[] LevelStages;

    // Start is called before the first frame update
    void Start()
    {
        if(!PlayerPrefs.HasKey("levelCompleted"))
        {
            Debug.Log("Resetting level");
            PlayerPrefs.SetInt("levelCompleted", 1);
        }

        Debug.Log("current level is "+ PlayerPrefs.GetInt("levelCompleted"));

     for (int i=0;i< PlayerPrefs.GetInt("levelCompleted"); i++)
        {
            LevelStages[i].SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
