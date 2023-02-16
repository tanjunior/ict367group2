using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    float elapsedTime = 0.0f;
    [SerializeField]TextMeshProUGUI timer;
    [SerializeField]ParkingController frontLeft, frontRight, rearLeft, rearRight;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        //Park();
    }

    public void Reset()
    {
        elapsedTime = 0.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Park()
    {
        if (frontLeft.GetValidation() && frontRight.GetValidation() && rearLeft.GetValidation() && rearRight.GetValidation()) Debug.Log("parked");
    }
}