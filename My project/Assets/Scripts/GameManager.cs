using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    
    [SerializeField]TextMeshProUGUI timer;
    [SerializeField]ParkingController frontLeft, frontRight, rearLeft, rearRight;
    [SerializeField]TextMesh text;
    float elapsedTime = 0.0f;
    bool isParked;
    private float fps = 0;
    private float framesCount = 0;
    private float lastCheck = 0;
    private float rate = 0.5f;

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
        FpsCounter();
    }

    public void Reset()
    {
        elapsedTime = 0.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Park()
    {
        isParked = (frontLeft.GetValidation() && frontRight.GetValidation() && rearLeft.GetValidation() && rearRight.GetValidation());
        if (isParked) Debug.Log("isParked:"+isParked);
    }

    private void FpsCounter() {
        framesCount++;
      if (Time.time >= lastCheck + rate)
      {
        fps = framesCount / (Time.time - lastCheck);
        lastCheck = Time.time;
        framesCount = 0;
        text.text = string.Format("FPS: {0:N0}\nisParked: {1:N0}", fps.ToString("F0"), isParked);
      }
    }
}