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
    private int[] stepSizeFreq_300 = new int[] { 20, 10, 8, 5, 2 };
    private int[] stepSizeFreq_30 = new int[] { 10, 7, 5, 3, 1 };
    private int stepDirection = -1;

    public float minAmp;
    public float maxAmp;

    private float amp = 0.05f;

    private int numbTrials;
    private Coroutine ExpRoutine, InstructRoutine, RunActuatorSeq;
    private int correctCounter = 0;
    private int answer = 0;
    private int correctcounter = 0;

    public TMPro.TMP_Text instructionDisplay;
    public string path = "H:/Project/Adaptive-Touch-Testing-System/ATTS_Data/";

    private float comparisonFrequency30 = 0f;
    private float comparisonFrequency300 = 0f;
    private int reversalCnt_30 = 0;
    private int reversalCnt_300 = 0;
    private int[] correctCntHist = new int[500];
    private float next_stimulus;
    private bool firstTime30 = true;
    private bool firstTime300 = true;

    void Start()
    {
        //amp = compStimulus; 

        // Initial the correctCntHist

        for (int i = 0; i < correctCntHist.Length; i++)
        {
            correctCntHist[i] = 0;
            // Debug.Log(correctCntHist[i]);
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

    #region Debug stuff
    //public void DebugActuator()
    //{
    //    //if (RunActuatorSeq != null)
    //    //    StopCoroutine(RunActuatorSeq);
    //    //RunActuatorSeq = StartCoroutine(RunActuator());

    //    float amplitude = 1f;

    //    // Comparison
    //    instructionDisplay.text = "1st stimulus";
    //    yield return new WaitForSeconds(0.5f);
    //    Signal collision1 = new Sine(50);
    //    amplitude = MapFreq2Amp(350); // Random.Range(0.05f, 0.95f);
    //    collision1 = new Sine(350) * new ASR(0.05, 0.075, 0.05) * amplitude;
    //    syntacts.session.Play(collisionChannel, collision1);
    //    yield return new WaitForSeconds(0.5f);

    //    // Standard
    //    instructionDisplay.text = "2nd stimulus";
    //    yield return new WaitForSeconds(0.5f);
    //    Signal collision2 = new Sine(50);
    //    amplitude = 1f; // Random.Range(0.05f, 0.95f);
    //    collision2 = new Sine(300) * new ASR(0.05, 0.075, 0.05) * amplitude;
    //    syntacts.session.Play(collisionChannel, collision2);
    //    yield return new WaitForSeconds(0.65f);

    //    instructionDisplay.text = "Which of the two stimuli had a higher frequency? \n\nPress A for 1st and D for 2nd";
    //    yield return new WaitForSeconds(0.1f);
    //    while (true)
    //    {
    //        if (Input.GetKeyDown(KeyCode.A))
    //        {
    //            answer = 0;
    //            break;
    //        }
    //        if (Input.GetKeyDown(KeyCode.D))
    //        {
    //            answer = 1;
    //            break;
    //        }
    //        yield return null;
    //    }

    //    yield return new WaitForSeconds(0.1f);
    //    instructionDisplay.text = "Press S to continue";
    //    while (true)
    //    {
    //        if (Input.GetKeyDown(KeyCode.S))
    //            break;
    //        yield return null;
    //    }
    //}
    #endregion

    IEnumerator ExperimentSequenceFreq2()
    {
        comparisonFrequency30 = 80; // Initial comparison freq.

        for (int i = 0; i < numbTrials; i++)
        {
            if (StimSequence[i] == 0) // Standard first then comparison stimulus 
            {
                // Standard
                instructionDisplay.text = "1st stimulus";
                yield return new WaitForSeconds(0.5f);
                float amplitude = 3.5f; // Random.Range(0.05f, 0.95f);
                Signal collision1 = new Sine(30) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision1);

                yield return new WaitForSeconds(1f);

                // Comparison
                instructionDisplay.text = "2nd stimulus";
                yield return new WaitForSeconds(0.5f);
                amplitude = 2f; // MapFreq2Amp(comparisonFrequency30); // Random.Range(0.05f, 0.95f);
                Signal collision2 = new Sine(comparisonFrequency30) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision2);
                yield return new WaitForSeconds(0.65f);
            }
            else if (StimSequence[i] == 1) // Comparison stimulus first then standard 
            {
                // Comparison
                instructionDisplay.text = "1st stimulus";
                yield return new WaitForSeconds(0.5f);
                float amplitude = 2f; // MapFreq2Amp(comparisonFrequency30); // Random.Range(0.05f, 0.95f);
                Signal collision2 = new Sine(comparisonFrequency30) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision2);
                yield return new WaitForSeconds(1f);

                // Standard
                instructionDisplay.text = "2nd stimulus";
                yield return new WaitForSeconds(0.5f);
                amplitude = 3.5f; // Random.Range(0.05f, 0.95f);
                Signal collision1 = new Sine(30) * new ASR(0.05, 0.075, 0.05) * amplitude;
                syntacts.session.Play(collisionChannel, collision1);
                yield return new WaitForSeconds(0.65f);
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

            next_stimulus = CheckAnswerFreq(StimSequence[i], i, comparisonFrequency30);
            comparisonFrequency30 = comparisonFrequency30 + next_stimulus;
            comparisonFrequency30 = Mathf.Sqrt(comparisonFrequency30 * comparisonFrequency30);

            yield return new WaitForSeconds(0.2f);
            instructionDisplay.text = "Press S to continue";
            yield return new WaitForSeconds(0.2f);
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.S))
                    break;
                yield return null;
            }
        }

        instructionDisplay.text = "End \n\nThanks for your participation";
        yield return null;
    }

    IEnumerator ExperimentSequenceFreq()
    {
        for (int i = 0; i < numbTrials; i++)
        {
            //DebugActuator();
            #region Original code (not working, because second stimulus never runs)
            float amplitude = 1f;

            // Randomly select which one of the two frequencies to use
            int standard_frequency = 0;

            // Select which stimulus amplitude first (standard(reference) or stimulus amplitude first? 

            if (StimSequence[i] == 0)
            {
                if (FreqOrder[i] == 0)
                {
                    standard_frequency = frequencies[0];
                    // Set initial comparison stimuli
                    if (firstTime30)
                    {
                        comparisonFrequency30 = 80; // Initial comparison freq.
                        firstTime30 = false;
                    }

                    // Standard
                    instructionDisplay.text = "1st stimulus";
                    yield return new WaitForSeconds(0.5f);
                    Signal collision2 = new Sine(50);
                    amplitude = 3.5f; // Random.Range(0.05f, 0.95f);
                    collision2 = new Sine(standard_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
                    syntacts.session.Play(collisionChannel, collision2);
                    yield return new WaitForSeconds(0.65f);

                    // Comparison
                    instructionDisplay.text = "2nd stimulus";
                    yield return new WaitForSeconds(0.5f);
                    Signal collision1 = new Sine(50);
                    amplitude = 1f; // MapFreq2Amp(comparisonFrequency30); // Random.Range(0.05f, 0.95f);
                    collision1 = new Sine(comparisonFrequency30) * new ASR(0.05, 0.075, 0.05) * amplitude;
                    syntacts.session.Play(collisionChannel, collision1);
                    yield return new WaitForSeconds(0.5f);

                    instructionDisplay.text = "Which of the two stimuli had a higher frequency? \n\nPress A for 1st and D for 2nd";
                    yield return new WaitForSeconds(0.1f);
                    // Check answer and adjust next stimuli step size based on this
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
                    Debug.Log("User Resp: " + answer + " Stimulus: " + FreqOrder[i] + " Amp: " + amp);
                    int trialNum = i;
                    next_stimulus = CheckAnswerFreq(FreqOrder[i], i, comparisonFrequency30);
                    comparisonFrequency30 = comparisonFrequency30 + next_stimulus;
                    comparisonFrequency30 = Mathf.Sqrt(comparisonFrequency30 * comparisonFrequency30);
                    //amp += next_stimulus;
                    Debug.Log("Comparison frequency 300: " + comparisonFrequency30 + " Standard freq: " + standard_frequency + " Amp: " + amplitude);
                }
                else if (FreqOrder[i] == 1)
                {
                    standard_frequency = frequencies[1];
                    // Set initial comparison stimuli
                    if (firstTime300)
                    {
                        comparisonFrequency300 = 400f; // Initial comparison freq.
                        firstTime300 = false;
                    }

                    // Standard
                    instructionDisplay.text = "1st stimulus";
                    yield return new WaitForSeconds(0.5f);
                    Signal collision2 = new Sine(50);
                    amplitude = 1f; // Random.Range(0.05f, 0.95f);
                    collision2 = new Sine(standard_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
                    syntacts.session.Play(collisionChannel, collision2);
                    yield return new WaitForSeconds(0.65f);

                    // Comparison
                    instructionDisplay.text = "2nd stimulus";
                    yield return new WaitForSeconds(0.5f);
                    Signal collision1 = new Sine(50);
                    amplitude = 1f; // MapFreq2Amp(comparisonFrequency300); // Random.Range(0.05f, 0.95f);
                    collision1 = new Sine(comparisonFrequency300) * new ASR(0.05, 0.075, 0.05) * amplitude;
                    syntacts.session.Play(collisionChannel, collision1);
                    yield return new WaitForSeconds(0.5f);

                    instructionDisplay.text = "Which of the two stimuli had a higher frequency? \n\nPress A for 1st and D for 2nd";
                    yield return new WaitForSeconds(0.1f);
                    // Check answer and adjust next stimuli step size based on this
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
                    Debug.Log("User Resp: " + answer + " Stimulus: " + FreqOrder[i] + " Amp: " + amp);
                    int trialNum = i;
                    next_stimulus = CheckAnswerFreq(FreqOrder[i], i, comparisonFrequency300);
                    comparisonFrequency300 = comparisonFrequency300 + next_stimulus;
                    comparisonFrequency300 = Mathf.Sqrt(comparisonFrequency300 * comparisonFrequency300);
                    //amp += next_stimulus;
                    Debug.Log("Comparison frequency 300: " + comparisonFrequency300 + " Standard freq: " + standard_frequency + " Amp: " + amplitude);
                }
            }
            if (StimSequence[i] == 1)
            {
                if (FreqOrder[i] == 0)
                {
                    standard_frequency = frequencies[0];
                    // Set initial comparison stimuli
                    if (firstTime30)
                    {
                        comparisonFrequency30 = 80; // Initial comparison freq.
                        firstTime30 = false;
                    }

                    // Comparison
                    instructionDisplay.text = "1st stimulus";
                    yield return new WaitForSeconds(0.5f);
                    Signal collision1 = new Sine(50);
                    amplitude = 1f; // MapFreq2Amp(comparisonFrequency30); // Random.Range(0.05f, 0.95f);
                    collision1 = new Sine(comparisonFrequency30) * new ASR(0.05, 0.075, 0.05) * amplitude;
                    syntacts.session.Play(collisionChannel, collision1);
                    yield return new WaitForSeconds(0.5f);

                    // Standard
                    instructionDisplay.text = "2nd stimulus";
                    yield return new WaitForSeconds(0.5f);
                    Signal collision2 = new Sine(50);
                    amplitude = 3.5f; // Random.Range(0.05f, 0.95f);
                    collision2 = new Sine(standard_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
                    syntacts.session.Play(collisionChannel, collision2);
                    yield return new WaitForSeconds(0.65f);


                    instructionDisplay.text = "Which of the two stimuli had a higher frequency? \n\nPress A for 1st and D for 2nd";
                    yield return new WaitForSeconds(0.1f);
                    // Check answer and adjust next stimuli step size based on this
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
                    Debug.Log("User Resp: " + answer + " Stimulus: " + FreqOrder[i] + " Amp: " + amp);
                    int trialNum = i;
                    next_stimulus = CheckAnswerFreq(FreqOrder[i], i, comparisonFrequency30);
                    comparisonFrequency30 = comparisonFrequency30 + next_stimulus;
                    comparisonFrequency30 = Mathf.Sqrt(comparisonFrequency30 * comparisonFrequency30);
                    //amp += next_stimulus;
                    Debug.Log("Comparison frequency 300: " + comparisonFrequency30 + " Standard freq: " + standard_frequency + " Amp: " + amplitude);
                }
                else if (FreqOrder[i] == 1)
                {
                    standard_frequency = frequencies[1];
                    // Set initial comparison stimuli
                    if (firstTime300)
                    {
                        comparisonFrequency300 = 400f; // Initial comparison freq.
                        firstTime300 = false;
                    }

                    // Comparison
                    instructionDisplay.text = "1st stimulus";
                    yield return new WaitForSeconds(0.5f);
                    Signal collision1 = new Sine(50);
                    amplitude = 1f; // MapFreq2Amp(comparisonFrequency300); // Random.Range(0.05f, 0.95f);
                    collision1 = new Sine(comparisonFrequency300) * new ASR(0.05, 0.075, 0.05) * amplitude;
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


                    instructionDisplay.text = "Which of the two stimuli had a higher frequency? \n\nPress A for 1st and D for 2nd";
                    yield return new WaitForSeconds(0.1f);
                    // Check answer and adjust next stimuli step size based on this
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
                    Debug.Log("User Resp: " + answer + " Stimulus: " + FreqOrder[i] + " Amp: " + amp);
                    int trialNum = i;
                    next_stimulus = CheckAnswerFreq(FreqOrder[i], i, comparisonFrequency300);
                    comparisonFrequency300 = comparisonFrequency300 + next_stimulus;
                    comparisonFrequency300 = Mathf.Sqrt(comparisonFrequency300 * comparisonFrequency300);
                    //amp += next_stimulus;
                    Debug.Log("Comparison frequency 300: " + comparisonFrequency300 + " Standard freq: " + standard_frequency + " Amp: " + amplitude);
                }
            }


            yield return new WaitForSeconds(0.1f);
            instructionDisplay.text = "Press S to continue";
            yield return new WaitForSeconds(0.5f);
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.S))
                    break;
                yield return null;
            }
            #endregion
        }

        instructionDisplay.text = "End \n\nThanks for your participation";

        yield return null;
    }

    public float UpdateStimulusFreq(float standardFreq, float compFreq)
    {
        float newFreq = 0f;



        return newFreq;
    }

    IEnumerator RunActuator()
    {
        float amplitude = 1f;

        // Comparison
        instructionDisplay.text = "1st stimulus";
        yield return new WaitForSeconds(0.5f);
        Signal collision1 = new Sine(50);
        amplitude = MapFreq2Amp(comparisonFrequency300); // Random.Range(0.05f, 0.95f);
        collision1 = new Sine(comparisonFrequency300) * new ASR(0.05, 0.075, 0.05) * amplitude;
        syntacts.session.Play(collisionChannel, collision1);
        yield return new WaitForSeconds(0.5f);

        // Standard
        instructionDisplay.text = "2nd stimulus";
        yield return new WaitForSeconds(0.5f);
        Signal collision2 = new Sine(50);
        amplitude = 1f; // Random.Range(0.05f, 0.95f);
        collision2 = new Sine(300) * new ASR(0.05, 0.075, 0.05) * amplitude;
        syntacts.session.Play(collisionChannel, collision2);
        yield return new WaitForSeconds(0.65f);

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

        yield return null;
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
            else if (StimSequence[i] == 1)
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
            // print("Reversal: One down");
            reversalCnt_30++;
            reversalCnt_300++;
            stepDirection = -1;
        }
        if (correctCntHist[idx + 3] == 1 & correctCntHist[(idx + 3) - 1] == 1 & correctCntHist[(idx + 3) - 2] == 0)
        {
            // print("Reversal: Two up");
            reversalCnt_30++;
            reversalCnt_300++;
            stepDirection = 1;
        }
        if (correctCntHist[idx + 3] == 0 & correctCntHist[(idx + 3) - 1] == 1 & correctCntHist[(idx + 3) - 2] == 0)
        {
            stepDirection = 1;
            // print("Not reversal");
            // Make it easier 
        }
        if (correctCntHist[idx + 3] == 1 & correctCntHist[(idx + 3) - 1] == 1 & correctCntHist[(idx + 3) - 2] == 1)
        {
            stepDirection = 1;
            // print("Not reversal");
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
        float delayBetweenInstructions = 0.5f;

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
        while (true)
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
        if (experimentType == myExpTypeEnum.AmplitudeDiscrimination)
            ExpRoutine = StartCoroutine(ExperimentSequenceAmp());
        if (experimentType == myExpTypeEnum.FrequencyDiscrimination)
            ExpRoutine = StartCoroutine(ExperimentSequenceFreq2());

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

        //if (frequency >= 185f)
        //{
        if (frequency < 420 & frequency >= 370)
            amp = 30f;
        if (frequency < 370 & frequency >= 350)
            amp = 15f;
        if (frequency < 350 & frequency > 345)
            amp = 6f;
        if (frequency <= 345 & frequency > 310)
            amp = 2f;
        if (frequency <= 310 & frequency > 300)
            amp = 1.1f;
        if (frequency <= 300 & frequency > 290)
            amp = 1;
        if (frequency <= 290 & frequency > 280)
            amp = 0.9f;
        if (frequency <= 280 & frequency > 270)
            amp = 0.8f;
        if (frequency <= 270 & frequency > 250)
            amp = 0.6f;
        if (frequency < 250 & frequency >= 230)
            amp = 0.4f;
        if (frequency < 230 & frequency >= 220)
            amp = 0.25f;
        if (frequency < 220 & frequency >= 200)
            amp = 0.2f;
        if (frequency < 100 & frequency >= 90)
            amp = 1.1f;
        if (frequency > 80 & frequency <= 90)
            amp = 1.3f;
        if (frequency > 70 & frequency < 80)
            amp = 1.4f;
        if (frequency >= 60 & frequency < 70)
            amp = 1.5f;
        if (frequency >= 50 & frequency < 60)
            amp = 1.6f;
        if (frequency >= 42 & frequency < 50)
            amp = 1.8f;
        if (frequency >= 38 & frequency < 42)
            amp = 2.4f;
        if (frequency >= 35 & frequency < 38)
            amp = 2.5f;
        if (frequency >= 30 & frequency < 35)
            amp = 3.2f;
        if (frequency > 25 & frequency <= 30)
            amp = 3.3f;
        if (frequency > 22 & frequency <= 25)
            amp = 3.9f;
        if (frequency > 20 & frequency <= 22)
            amp = 4.7f;
        if (frequency > 10 & frequency <= 20)
            amp = 5.7f;
        //}
        //if (frequency < 150f)
        //{

        //}

        //Debug.Log("Amp: " + amp);
        return amp;
    }

}



//else if (StimSequence[i] == 1)
//{
//    standard_frequency = frequencies[0];

//    if (firstTime30)
//    {
//        comparisonFrequency30 = 80f; // Initial comparison freq.
//        firstTime30 = false;
//    }

//    // Standard
//    instructionDisplay.text = "1st stimulus";
//    yield return new WaitForSeconds(0.65f);
//    Signal collision2 = new Sine(50);
//    amplitude = 3.5f; // Random.Range(0.05f, 0.95f);
//    collision2 = new Sine(standard_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
//    syntacts.session.Play(collisionChannel, collision2);
//    yield return new WaitForSeconds(0.65f);

//    // Comparison
//    instructionDisplay.text = "2nd stimulus";
//    yield return new WaitForSeconds(0.65f);
//    Signal collision1 = new Sine(50);
//    amplitude = /*MapFreq2Amp*/(comparisonFrequency30); // Random.Range(0.05f, 0.95f);
//    collision1 = new Sine(comparisonFrequency30) * new ASR(0.05, 0.075, 0.05) * amplitude;
//    syntacts.session.Play(collisionChannel, collision1);
//    yield return new WaitForSeconds(0.5f);


//    instructionDisplay.text = "Which of the two stimuli had a higher frequency? \n\nPress A for 1st and D for 2nd";
//    yield return new WaitForSeconds(0.1f);
//    // Check answer and adjust next stimuli step size based on this
//    while (true)
//    {
//        if (Input.GetKeyDown(KeyCode.A))
//        {
//            answer = 0;
//            break;
//        }
//        if (Input.GetKeyDown(KeyCode.D))
//        {
//            answer = 1;
//            break;
//        }
//        yield return null;
//    }
//    Debug.Log("User Resp: " + answer + " Stimulus: " + FreqOrder[i] + " Amp: " + amp);
//    int trialNum = i;
//    next_stimulus = CheckAnswerFreq(FreqOrder[i], i, comparisonFrequency30);
//    comparisonFrequency30 = comparisonFrequency30 + next_stimulus;
//    comparisonFrequency30 = Mathf.Sqrt(comparisonFrequency30 * comparisonFrequency30);
//    //amp += next_stimulus;
//    Debug.Log("Comparison frequency 30: " + comparisonFrequency30 + " Standard freq: " + standard_frequency + " Amp: " + amplitude);

//}