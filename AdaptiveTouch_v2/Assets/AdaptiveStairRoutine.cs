using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Syntacts;
using System.IO;
using System.Linq;

public class StairCase
{
    // Variables
    int[] trialSequence;
    string path;
    public int[] StimSequence;

    // Init
    public StairCase(string pathin, int numTrials)
    {
        path = pathin;
        StimSequence = new int[numTrials];
        StimSequence = CreateStimSequeces(StimSequence, 2);
        Shuffle(StimSequence);
    }


    // Create Trial Stimulus Sequence
    public int[] CreateStimSequeces(int[] stimseqs, int repeteVal)
    {
        int countr = 0;
        for (int i = 0; i < stimseqs.Length; i++)
        {
            stimseqs[i] = countr;
            countr++;

            if (countr > repeteVal)
            {
                countr = 0;
            }
        }
        return stimseqs;
    }

    // Save files
    public void SaveFile()
    {
        File.AppendAllText(path, trialSequence.ToArray().ToString() + " \n");
    }

    // Load files


    // Shuffle Randomizer
    static System.Random _random = new System.Random();
    public static void Shuffle<T>(T[] array)
    {
        int n = array.Length;
        for (int i = 0; i < (n - 1); i++)
        {
            int r = i + _random.Next(n - i);
            T t = array[r];
            array[r] = array[i];
            array[i] = t;
        }
    }
}

public class AdaptiveStairRoutine : MonoBehaviour
{

    private StairCase stair_30Hz;
    private int[] StimSequence;
    private int[] FreqOrder; 

    public SyntactsHub syntacts;
    private int collisionChannel = 0;

    public int[] frequencies;
    public string[] stimulitypes;
    public float initialAmp;
    public float standardAmp;

    public float stepSize;
    public float minAmp;
    public float maxAmp;

    private float amp = 0.05f; 

    private int numbTrials;
    private Coroutine ExpRoutine, InstructRoutine; 
    private int correctCounter = 0;
    private int answer = 0;
    private int correctcounter = 0; 

    public TMPro.TMP_Text instructionDisplay;
    public string path = "H:/Project/Adaptive-Touch-Testing-System/ATTS_Data/";

    void Start()
    {

        amp = initialAmp; 

        numbTrials = 15;

        StimSequence = new int[numbTrials];
        StimSequence = CreateStimSequeces(StimSequence, 1);
        Shuffle(StimSequence);

        FreqOrder = new int[numbTrials]; 
        FreqOrder = CreateStimSequeces(FreqOrder, 1);
        Shuffle(FreqOrder);

        //stair_30Hz = new StairCase(path, numbTrials);

        // Instructions sequence ==>> This leads to the main experiment sequence
        InstructRoutine = StartCoroutine(InstructionSequence());
    }

    IEnumerator ExperimentSequence()
    {

        for (int i = 0; i < numbTrials; i++)
        {
            float amplitude;

            // Randomly select which one of the two frequencies to use
            int stimulus_frequency = 0; 
            if (FreqOrder[i] == 0)
            {
                stimulus_frequency = frequencies[0];
            }
            else if (FreqOrder[i] == 1)
            {
                stimulus_frequency = frequencies[1];
            }

            // Select which stimulus amplitude first (standard(reference) or stimulus amplitude first? 
            if (StimSequence[i] == 0)
            {
                instructionDisplay.text = "1st stimulus";
                yield return new WaitForSeconds(0.5f);
                Signal collision1 = new Sine(50);
                amplitude = amp; // Random.Range(0.05f, 0.95f);
                collision1 = new Square(stimulus_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision1);
                yield return new WaitForSeconds(0.5f);

                instructionDisplay.text = "2nd stimulus";
                yield return new WaitForSeconds(0.5f);
                Signal collision2 = new Sine(50);
                amplitude = standardAmp; // Random.Range(0.05f, 0.95f);
                collision2 = new Square(stimulus_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision2);
                yield return new WaitForSeconds(0.5f);
            }
            else if(StimSequence[i] == 1)
            {
                instructionDisplay.text = "1st stimulus";
                yield return new WaitForSeconds(0.5f);
                Signal collision2 = new Sine(50);
                amplitude = standardAmp; // Random.Range(0.05f, 0.95f);
                collision2 = new Square(stimulus_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision2);
                yield return new WaitForSeconds(0.5f);

                instructionDisplay.text = "2nd stimulus";
                yield return new WaitForSeconds(0.5f);
                Signal collision1 = new Sine(50);
                amplitude = amp; // Random.Range(0.05f, 0.95f);
                collision1 = new Square(stimulus_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision1);
                yield return new WaitForSeconds(0.5f);
            }

            instructionDisplay.text = "Which of the two stimuli felt more intense? \n\nPress A for 1st and D for 2nd";
            yield return new WaitForSeconds(0.1f);

            while (true)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    answer = 0;
                    break;
                }
                if (Input.GetKeyDown(KeyCode.D))
                {
                    answer = 1;
                    break;
                }
                yield return null;
            }

            Debug.Log("User Resp: " + answer + " Stimulus: " + StimSequence[i] + " Amp: " + amp + " Freq: " + FreqOrder[i]);

            yield return new WaitForSeconds(0.1f);
            instructionDisplay.text = "Press S to continue";
            yield return new WaitForSeconds(0.5f);
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.S))
                    break;
                yield return null;
            }

            // Check answer and adjust next stimuli step size based on this
            int trialNum = i;
            
            float next_stimulus = CheckAnswer(StimSequence[i]);
            amp += next_stimulus; 

        }

        instructionDisplay.text = "End \n\nThanks for your participation";

        yield return null; 
    }

    // Check Answers
    public float CheckAnswer(int stimulus_position)
    {
        float nextStimulus = 0f;

        if (answer == stimulus_position & correctcounter == 0)
        {
            correctcounter++;
            Debug.Log("Correct");
        }
        else if (answer == stimulus_position & correctcounter == 1)
        {
            nextStimulus -= stepSize;
            correctcounter = 0;
            Debug.Log("Correct");
        }
        else
        {
            nextStimulus += stepSize;
            correctcounter = 0;
            Debug.Log("Wrong");
        }
        // if answer if correct twice then update the stimulus intensity

        // if answer is wrong once then update the stimulus 


        return nextStimulus;
    }

    IEnumerator InstructionSequence()
    {
        float delayBetweenInstructions = 2f; 

        instructionDisplay.text = "Thaks for taking part in ATTS \n Please follow these instructions to complete this experiment";
        yield return new WaitForSeconds(delayBetweenInstructions);
        instructionDisplay.text = "Thaks for taking part in ATTS \n Please follow these instructions to complete this experiment \n \n Press the 'S' key to continue!";
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.S))
                break;
            yield return null;
        }

        instructionDisplay.text = "You will receive a pair of stimuli seperated by a 1 second pause, then answer the displayed question.";
        yield return new WaitForSeconds(delayBetweenInstructions);
        instructionDisplay.text = "You will receive a pair of stimuli seperated by a 1 second pause, then answer the displayed question. \n \n Press the 'S' key to continue!";
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.S))
                break;
            yield return null;
        }

        instructionDisplay.text = "This process repeats until the experiment comes to an end.";
        yield return new WaitForSeconds(delayBetweenInstructions);
        instructionDisplay.text = "This process repeats until the experiment comes to an end. \n \n Press the 'S' key to start this experiment!";
        while(true)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                break;
            }
            yield return null;
        }

        // Start experiment coroutine 
        if (ExpRoutine != null)
            StopCoroutine(ExpRoutine);
        ExpRoutine = StartCoroutine(ExperimentSequence());

        yield return null;
    }

    // Create Trial Stimulus Sequence
    public int[] CreateStimSequeces(int[] stimseqs, int repeteVal)
    {
        int countr = 0;
        for (int i = 0; i < stimseqs.Length; i++)
        {
            stimseqs[i] = countr;
            countr++;

            if (countr > repeteVal)
            {
                countr = 0;
            }
        }
        return stimseqs;
    }

    // Shuffle Randomizer
    static System.Random _random = new System.Random();
    public static void Shuffle<T>(T[] array)
    {
        int n = array.Length;
        for (int i = 0; i < (n - 1); i++)
        {
            int r = i + _random.Next(n - i);
            T t = array[r];
            array[r] = array[i];
            array[i] = t;
        }
    }

}
