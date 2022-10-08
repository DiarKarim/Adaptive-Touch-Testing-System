using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChangeValue : MonoBehaviour
{

    public TMPro.TMP_Text disp_text;

    void Start()
    {
        disp_text = GetComponent<TMPro.TMP_Text>();
        disp_text.text = "";
    }

    public void Text_Update_Freq(float val)
    {
        disp_text.text = Mathf.RoundToInt(val).ToString() + " Hz";
    }

    public void Text_Update_Amp(float val)
    {
        disp_text.text = val.ToString("F1") + " dB";
    }

    public void Text_Update_Dur(float val)
    {
        disp_text.text = val.ToString("F2") + " s";
    }

    public void Text_Update_Wav(float val)
    {
        string[] wave_types = new string[] { "Sine" ,"Square", "Saw", "Triangle"};
        int wave_index = Mathf.RoundToInt(val);

        disp_text.text = wave_types[wave_index];
    }

}
