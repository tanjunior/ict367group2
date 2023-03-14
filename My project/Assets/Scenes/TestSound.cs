using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TestSound : MonoBehaviour
{
    private AudioSource audioSource;
    private float frequency = 300.0f; // Adjust this to change the pitch of the sound.
    private float duration = 0.2f; // Adjust this to change the length of the sound.

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space)) // Change this to whatever input you want to trigger the sound.
        {
            float[] samples = new float[(int)(duration * audioSource.clip.frequency)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / audioSource.clip.frequency;
                samples[i] = Mathf.Sin(2.0f * Mathf.PI * frequency * t) * Mathf.Exp(-10.0f * t);
            }

            AudioClip clip = AudioClip.Create("BrakingSound", samples.Length, 1, audioSource.clip.frequency, false);
            clip.SetData(samples, 0);

            audioSource.PlayOneShot(clip);
        }
    }
}
