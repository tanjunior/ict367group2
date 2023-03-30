using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Fps : MonoBehaviour
{
    
    private float fps = 0;
    private float framesCount = 0;
    private float lastCheck = 0;
    private float rate = 0.5f;
    [SerializeField] private TextMeshProUGUI fpsText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FpsCounter();
    }

    private void FpsCounter() {
        framesCount++;
      if (Time.time >= lastCheck + rate)
      {
        fps = framesCount / (Time.time - lastCheck);
        lastCheck = Time.time;
        framesCount = 0;
        fpsText.text = string.Format("{0:N0}",fps.ToString("F0"));
      }
    }
}
