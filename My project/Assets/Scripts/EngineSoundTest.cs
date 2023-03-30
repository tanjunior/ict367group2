using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineSoundTest : MonoBehaviour
{
    [Range(-2000, 5000)]
    public float motorRPM = 500;
    public float pitch;
    public AudioSource engineSound;
    public AnimationCurve engineSoundCurve;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        pitch = engineSoundCurve.Evaluate(motorRPM);
        engineSound.pitch = pitch;
    }
}
