using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDetector : MonoBehaviour
{
    public GameObject[] LevelStages;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i <= PlayerPrefs.GetInt("levelCompleted"); i++)
        {
            LevelStages[i].SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
