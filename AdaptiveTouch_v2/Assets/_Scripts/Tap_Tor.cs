using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Syntacts;
using UnityEngine.UI;

public class Tap_Tor : MonoBehaviour
{
    public SyntactsHub syntacts;
    public Transform highlighter;
    public int collisionChannel = 0;
    public float collisionFreq = 500;
    public Slider frequencySlider;
    public Slider amplitudeSlider;
    public Slider durationSlider;
    public Slider waveSlider;
    public AudioSource audio_vib_1;

    public int sa1_freq = 30;
    public int ra1_freq = 80;
    public int ra2_freq = 300;
    public int res_freq = 174;

    //public Button SA_Button;
    //public Button RA_Button;
    //public Button Pac_Button;

    //public Button Save_Button;
    //public Button Load_Button;

    public bool AddDebugSound;

    private Vector3 worldPosition;
    private Plane plane = new Plane(Vector3.forward, 0);

    //public int velocityChannel = 1;
    //public float velocityFreq = 200;


    void Start()
    {
        //syntacts.session.Play(velocityChannel, new Sine(velocityFreq) * new Sine(5));
    }

    void Update()
    {
        //syntacts.session.SetPitch(velocityChannel, 1 + rb.velocity.magnitude * 0.1);

        if (Input.GetMouseButtonDown(0))
        {
            //Vector3 mousePos = Input.mousePosition;
            //Debug.Log("x: " + mousePos.x.ToString("F1") + "\t y: " + mousePos.y.ToString("F1"));

            //Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log("x: " + worldPosition.x.ToString("F1") + " y: " + worldPosition.y.ToString("F1") + " z: " + worldPosition.z.ToString("F1"));


            float distance;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out distance))
            {
                worldPosition = ray.GetPoint(distance);
            }
            //Debug.Log("x: " + worldPosition.x.ToString("F1") + " y: " + worldPosition.y.ToString("F1") + " z: " + worldPosition.z.ToString("F1"));

            if (worldPosition.x < 0.0f)
            {
                highlighter.transform.position = worldPosition;

                int wave_index = Mathf.RoundToInt(waveSlider.value);

                Signal collision = new Sine(50);
                float signalDuration = durationSlider.value;

                if (wave_index == 0)
                {
                    collision = new Sine(frequencySlider.value) * new ASR(0.05, signalDuration, 0.05) * amplitudeSlider.value;
                }
                else if (wave_index == 1)
                {
                    collision = new Square(frequencySlider.value) * new ASR(0.05, signalDuration, 0.05) * amplitudeSlider.value;
                }
                else if (wave_index == 2)
                {
                    collision = new Saw(frequencySlider.value) * new ASR(0.05, signalDuration, 0.05) * amplitudeSlider.value;
                }
                else if (wave_index == 3)
                {
                    collision = new Triangle(frequencySlider.value) * new ASR(0.05, signalDuration, 0.05) * amplitudeSlider.value;
                }

                syntacts.session.Play(collisionChannel, collision);
                if (AddDebugSound)
                {
                    audio_vib_1.Play();
                }
            }



        }
    }

    public void ReceptorParams(int recept)
    {
        if(recept == 0)
        {
            frequencySlider.value = sa1_freq; // SA-1
        }
        if (recept == 1)
        {
            frequencySlider.value = ra1_freq; // RA=1
        }
        if (recept == 2)
        {
            frequencySlider.value = ra2_freq; // RA=2
        }
        if (recept == 3)
        {
            frequencySlider.value = res_freq; // Resonant frequency = 3
        }
    }

}
