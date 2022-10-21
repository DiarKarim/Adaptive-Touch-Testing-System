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

    public enum myExpTypeEnum
    {
        FrequencyDiscrimination,
        AmplitudeDiscrimination
    }

    public myExpTypeEnum experimentType = myExpTypeEnum.FrequencyDiscrimination;

    //private StairCase stair_30Hz;
    private int[] StimSequence;
    private int[] FreqOrder;

    public SyntactsHub syntacts;
    private int collisionChannel = 0;

    public int[] frequencies;
    public string[] stimulitypes;
    //public float standardStimulus;
    //public float compStimulus;


    public float stepSize;
    private int[] stepSizeFreq_300 = new int[]{20, 10, 8, 5, 2};
    private int[] stepSizeFreq_30 = new int[] {10, 7, 5, 3, 1};
    private int stepDirection = -1; 

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

    private float comparisonFrequency = 0f;
    private int reversalCnt_30 = 0;
    private int reversalCnt_300 = 0;
    private int[] correctCntHist = new int[500];
    private float next_stimulus; 

    void Start()
    {
        //amp = compStimulus; 

        // Initial the correctCntHist

        for (int i = 0; i < correctCntHist.Length; i++)
        {
            correctCntHist[i] = 0;
            Debug.Log(correctCntHist[i]);
        }


        numbTrials = 15;

        StimSequence = new int[numbTrials];
        StimSequence = CreateStimSequeces(StimSequence, 1);
        Shuffle(StimSequence);

        FreqOrder = new int[numbTrials]; 
        FreqOrder = CreateStimSequeces(FreqOrder, 1);
        Shuffle(FreqOrder);

        // Instructions sequence ==>> This leads to the main experiment sequence
        InstructRoutine = StartCoroutine(InstructionSequence());
    }

    IEnumerator ExperimentSequenceFreq()
    {
        for (int i = 0; i < numbTrials; i++)
        {
            float amplitude;

            // Randomly select which one of the two frequencies to use
            int standard_frequency = 0;
            if (FreqOrder[i] == 0)
            {
                standard_frequency = frequencies[0];
                if (i == 0)
                {
                    comparisonFrequency = 80; // Initial comparison freq.
                }
            }
            else if (FreqOrder[i] == 1)
            {
                standard_frequency = frequencies[1];
                if (i == 0)
                {
                    comparisonFrequency = 400f; // Initial comparison freq.
                }
            }

            Debug.Log("Comparison frequency: " + comparisonFrequency);


            // Select which stimulus amplitude first (standard(reference) or stimulus amplitude first? 
            if (StimSequence[i] == 0)
            {
                // Comparison
                instructionDisplay.text = "1st stimulus";
                yield return new WaitForSeconds(0.5f);
                Signal collision1 = new Sine(50);
                amplitude = MapFreq2Amp(comparisonFrequency); // Random.Range(0.05f, 0.95f);
                collision1 = new Sine(comparisonFrequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision1);
                yield return new WaitForSeconds(0.5f);

                // Standard
                instructionDisplay.text = "2nd stimulus";
                yield return new WaitForSeconds(0.5f);
                Signal collision2 = new Sine(50);
                amplitude = 1f; // Random.Range(0.05f, 0.95f);
                collision2 = new Sine(standard_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision2);
                yield return new WaitForSeconds(0.65f);
            }
            else //if (StimSequence[i] == 1)
            {
                // Standard
                instructionDisplay.text = "1st stimulus";
                yield return new WaitForSeconds(0.65f);
                Signal collision2 = new Sine(50);
                amplitude = 3.5f; // Random.Range(0.05f, 0.95f);
                collision2 = new Sine(standard_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision2);
                yield return new WaitForSeconds(0.65f);

                // Comparison
                instructionDisplay.text = "2nd stimulus";
                yield return new WaitForSeconds(0.65f);
                Signal collision1 = new Sine(50);
                amplitude = MapFreq2Amp(comparisonFrequency); // Random.Range(0.05f, 0.95f);
                collision1 = new Sine(comparisonFrequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision1);
                yield return new WaitForSeconds(0.5f);
            }

            instructionDisplay.text = "Which of the two stimuli had a higher frequency? \n\nPress A for 1st and D for 2nd";
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

            next_stimulus = CheckAnswerFreq(StimSequence[i], i, comparisonFrequency);
            comparisonFrequency = comparisonFrequency + next_stimulus;
            comparisonFrequency = Mathf.Sqrt(comparisonFrequency * comparisonFrequency);
            //amp += next_stimulus;
        }

        instructionDisplay.text = "End \n\nThanks for your participation";

        yield return null;
    }


    public float UpdateStimulusFreq(float standardFreq, float compFreq)
    {
        float newFreq = 0f; 



        return newFreq;
    }

    IEnumerator ExperimentSequenceAmp()
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
                collision1 = new Sine(stimulus_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision1);
                yield return new WaitForSeconds(0.5f);

                instructionDisplay.text = "2nd stimulus";
                yield return new WaitForSeconds(0.5f);
                Signal collision2 = new Sine(50);
                amplitude = 1.0f; // Random.Range(0.05f, 0.95f);
                collision2 = new Sine(stimulus_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision2);
                yield return new WaitForSeconds(0.5f);
            }
            else if(StimSequence[i] == 1)
            {
                instructionDisplay.text = "1st stimulus";
                yield return new WaitForSeconds(0.5f);
                Signal collision2 = new Sine(50);
                amplitude = 1.0f; // Random.Range(0.05f, 0.95f);
                collision2 = new Sine(stimulus_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision2);
                yield return new WaitForSeconds(0.5f);

                instructionDisplay.text = "2nd stimulus";
                yield return new WaitForSeconds(0.5f);
                Signal collision1 = new Sine(50);
                amplitude = amp; // Random.Range(0.05f, 0.95f);
                collision1 = new Sine(stimulus_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision1);
                yield return new WaitForSeconds(0.5f);
            }

            instructionDisplay.text = "Which of the two stimuli had a higher frequency? \n\nPress A for 1st and D for 2nd";
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

    // Check Answers amp
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

    // Check Answers freq
    public float CheckAnswerFreq(int stimulus_position, int idx, float compStim)
    {
        //float nextStimulus = 0f;
        if (answer == stimulus_position & correctcounter == 0)
        {
            correctcounter++;
            correctCntHist[idx + 3] = 1;
            Debug.Log("Correct");
        }
        else if (answer == stimulus_position & correctcounter == 1)
        {
            correctCntHist[idx + 3] = 1;
            //nextStimulus -= stepSize;
            correctcounter = 0;
            Debug.Log("Correct");
        }
        else
        {
            //nextStimulus += stepSize;
            correctcounter = 0;
            correctCntHist[idx + 3] = 0;
            Debug.Log("Wrong");
        }

        // Reversal counter 
        if (correctCntHist[idx + 3] == 0 & correctCntHist[(idx + 3) - 1] == 1 & correctCntHist[(idx + 3) - 2] == 1)
        {
            print("Reversal: One down");
            reversalCnt_30++;
            reversalCnt_300++;
            stepDirection = -1;
        }
        if (correctCntHist[idx + 3] == 1 & correctCntHist[(idx + 3) - 1] == 1 & correctCntHist[(idx + 3) - 2] == 0)
        {
            print("Reversal: Two up");
            reversalCnt_30++;
            reversalCnt_300++;
            stepDirection = 1;
        }
        if (correctCntHist[idx + 3] == 0 & correctCntHist[(idx + 3) - 1] == 1 & correctCntHist[(idx + 3) - 2] == 0)
        {
            stepDirection = 1;
            print("Not reversal");
            // Make it easier 
        }
        if (correctCntHist[idx + 3] == 1 & correctCntHist[(idx + 3) - 1] == 1 & correctCntHist[(idx + 3) - 2] == 1)
        {
            stepDirection = 1;
            print("Not reversal");
        }


        float nextStimulus = 0f;

        if (compStim > 200)
        {
            nextStimulus = stepDirection * (compStim + stepSizeFreq_300[reversalCnt_300]);
        }
        else
        {
            nextStimulus = stepDirection * (compStim + stepSizeFreq_30[reversalCnt_30]);
        }

        // if answer is wrong once then update the stimulus 
        //comparisonFrequency = UpdateStimulusFreq(standard_frequency, comparisonFrequency);

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
        if(experimentType == myExpTypeEnum.AmplitudeDiscrimination)
            ExpRoutine = StartCoroutine(ExperimentSequenceAmp());
        if (experimentType == myExpTypeEnum.FrequencyDiscrimination)
            ExpRoutine = StartCoroutine(ExperimentSequenceFreq());

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


    public float MapFreq2Amp(float frequency)
    {
        float amp = 0f;

        if (frequency >= 185f)
        {
            if (frequency < 420 & frequency >= 370)
                amp = 30f;
            else if (frequency < 370 & frequency >= 350)
                amp = 15f;
            else if (frequency < 350 & frequency > 345)
                amp = 6f;
            else if (frequency <= 345 & frequency > 310)
                amp = 2f;
            else if (frequency <= 310 & frequency > 300)
                amp = 1.1f;
            else if (frequency <= 300 & frequency > 290)
                amp = 1;
            else if (frequency <= 290 & frequency > 280)
                amp = 0.9f;
            else if (frequency <= 280 & frequency > 270)
                amp = 0.8f;
            else if (frequency <= 270 & frequency > 250)
                amp = 0.6f;
            else if (frequency < 250 & frequency >= 230)
                amp = 0.4f;
            else if (frequency < 230 & frequency >= 220)
                amp = 0.25f;
            else if (frequency < 220 & frequency >= 200)
                amp = 0.2f;
        }
        if (frequency < 150f)
        {
            if (frequency < 100 & frequency >= 90)
                amp = 1.1f;
            else if (frequency < 80 & frequency >= 90)
                amp = 1.3f;
            else if (frequency < 70 & frequency > 80)
                amp = 1.4f;
            else if (frequency <= 60 & frequency > 70)
                amp = 1.5f;
            else if (frequency <= 50 & frequency > 60)
                amp = 1.6f;
            else if (frequency <= 42 & frequency > 50)
                amp = 1.8f;
            else if (frequency <= 38 & frequency > 42)
                amp = 2.4f;
            else if (frequency <= 35 & frequency > 38)
                amp = 2.5f;
            else if (frequency <= 30 & frequency > 35)
                amp = 3.2f;
            else if (frequency < 25 & frequency >= 30)
                amp = 3.3f;
            else if (frequency < 22 & frequency >= 25)
                amp = 3.9f;
            else if (frequency < 20 & frequency >= 22)
                amp = 4.7f;
            else if (frequency < 10 & frequency >= 20)
                amp = 5.7f;
        }

        return amp;
    }

}
