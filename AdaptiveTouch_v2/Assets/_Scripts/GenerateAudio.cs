using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class GenerateAudio : MonoBehaviour
{
    public Slider slider_freq;
    public Slider slider_duration;
    public Slider slider_amp;
    public Slider slider_wav;

    public int position = 0;
    public int samplerate = 44100;
    //public float frequency = 100;

    AudioSource aud; 

    //void Start()
    //{
    //    AudioClip myClip = AudioClip.Create("MySinusoid", samplerate * 2, 1, samplerate, true, OnAudioRead, OnAudioSetPosition);
    //    aud = GetComponent<AudioSource>();
    //    aud.clip = myClip;
    //}

    public void PlayAudio()
    {
        int duration = Mathf.RoundToInt(slider_duration.value); 

        AudioClip myClip = AudioClip.Create("MySinusoid", samplerate * duration, 1, samplerate, true, OnAudioRead, OnAudioSetPosition);
        aud = GetComponent<AudioSource>();
        aud.clip = myClip;

        aud.Play();
    }

    void OnAudioRead(float[] data)
    {

        float frequency = slider_freq.value;
        float amplitude = slider_amp.value;

        int count = 0;
        while (count < data.Length)
        {
            data[count] = Mathf.Sin(2 * Mathf.PI * frequency * position / samplerate) * amplitude;
            position++;
            count++;
        }
    }

    void OnAudioSetPosition(int newPosition)
    {
        position = newPosition;
    }
}