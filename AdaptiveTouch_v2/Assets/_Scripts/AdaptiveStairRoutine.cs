using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Syntacts;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Threading; 

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

public class DataClass
{
    public List<int> user_response = new List<int>();
    public List<int> trialNumber = new List<int>();
    public List<float> standard_stim = new List<float>();
    public List<float> comparison_stim = new List<float>();
    public List<float> standard_freq = new List<float>();
    public List<float> comp_freq = new List<float>();
    public List<string> correct = new List<string>();

    //public List<int> frameNum = new List<int>();
    //public List<string> gameObjectName = new List<string>();
    //public List<float> xPos = new List<float>();
    //public List<float> yPos = new List<float>();
    //public List<float> zPos = new List<float>();
    //public List<float> xRot = new List<float>();
    //public List<float> yRot = new List<float>();
    //public List<float> zRot = new List<float>();
    //public List<string> targetID = new List<string>();
    //public List<float> xTPos = new List<float>();
    //public List<float> yTPos = new List<float>();
    //public List<float> zTPos = new List<float>();
    //public List<float> time = new List<float>();
}

public class AdaptiveStairRoutine : MonoBehaviour
{

    #region Variables
    private bool endReversals;

    public string participantID = "";
    public TMP_InputField studyInfo;
    public TMP_InputField pathField;

    private DataClass expTrialData = new DataClass();
    public Transform TrackedObjects;

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

    // Initial comparison freq.
    public float[] comFreq = new float[2] { 80f, 400f };
    private float comparisonFrequency = 80f;
    private float comparisonFrequency300 = 400f;
    public int[] frequencies;
    public string[] stimulitypes;
    //public float standardStimulus;
    //public float compStimulus;

    public float stepSize;
    public int[] stepSizeFreq_30 = new int[] { 10, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2};
    public int[] stepSizeFreq_300 = new int[] { 20, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 };
    private int stepDirection = -1;

    public float minAmp;
    public float maxAmp;

    private float amp = 0.05f;

    public int maxNumbTrials = 30;
    private int numbTrials = 10;
    private Coroutine ExpRoutine, InstructRoutine, RunActuatorSeq;
    private int answer = 0;
    private int correctcounter_30 = 0;
    private int correctcounter_300 = 0;
    private int correctcounter = 0; 

    public TMPro.TMP_Text instructionDisplay;
    public TMPro.TMP_Text pathDisplay;
    private string path; // = "H:/Project/Adaptive-Touch-Testing-System/ATTS_Data/";

    private int reversalCnt_30 = 0;
    private int reversalCnt_300 = 0;
    private int[] correctCntHist = new int[500];
    private float next_stimulus;
    private bool firstTime30 = true;
    private bool firstTime300 = true;
    private bool recording = false;
    private float currTime, startTime = 0f;
    private int frameNum = 0;
    private string checkedAnswer = "";

    private Thread dataSaveThread;
    private int standardFrequency = 0;
    private bool done30, done300;

    public int reversals; 
    public Slider ProgressSlider, ReversalSlider;

    private int[] correctInARow_30 = new int[2] { 0, 0 };
    private int[] correctInARow_300 = new int[2] { 0, 0 };

    public bool firstResponse, secondResponse, confirm, endNsave;

    // Audio
    public int position = 0;
    public int samplerate = 44100;
    private float frequency, amplitude;
    AudioSource aud;

    #endregion

    void Awake()
    {
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        //path = Application.persistentDataPath; 
        pathDisplay.text = path; 

        numbTrials = maxNumbTrials;

        ProgressSlider.minValue = 0;
        ProgressSlider.maxValue = maxNumbTrials;
        ReversalSlider.minValue = 0;
        ReversalSlider.maxValue = reversals;

        //amp = compStimulus; 

        // Initial the correctCntHist

        for (int i = 0; i < correctCntHist.Length; i++)
        {
            correctCntHist[i] = 0;
            // Debug.Log(correctCntHist[i]);
        }

        StimSequence = new int[numbTrials];
        StimSequence = CreateStimSequeces(StimSequence, 1);
        Shuffle(StimSequence);

        FreqOrder = new int[numbTrials];
        FreqOrder = CreateStimSequeces(FreqOrder, 1);
        Shuffle(FreqOrder);

        // Instructions sequence ==>> This leads to the main experiment sequence
        InstructRoutine = StartCoroutine(InstructionSequence());

        // For continously recording data uncomment this init function below and make sure "TrackedObjects" is filled in correctly in the inspector 
        //Init();
    }

    private void Init()
    {
        dataSaveThread = new Thread(new ThreadStart(RecordDataFunc));
        dataSaveThread.IsBackground = true;
        dataSaveThread.Start();
    }

    void RecordDataFunc()
    {
        // ... record data as fast as possible while waiting for participant to hit the target 
        if (recording)
        {
            currTime = UnityEngine.Time.time - startTime;

            foreach (Transform trckdObj in TrackedObjects)
            {
                //// Repeated numbers 
                //expTrialData.time.Add(currTime);
                //expTrialData.frameNum.Add(frameNum);
                //expTrialData.targetID.Add(targets[randTargetIndex].gameObject.name);
                //expTrialData.xTPos.Add(targets[randTargetIndex].position.x);
                //expTrialData.yTPos.Add(targets[randTargetIndex].position.y);
                //expTrialData.zTPos.Add(targets[randTargetIndex].position.z);

                //// Frame-by-frame numbers 
                //expTrialData.gameObjectName.Add(trckdObj.gameObject.name);
                //expTrialData.xPos.Add(trckdObj.position.x);
                //expTrialData.yPos.Add(trckdObj.position.y);
                //expTrialData.zPos.Add(trckdObj.position.z);
                //expTrialData.xRot.Add(trckdObj.eulerAngles.x);
                //expTrialData.yRot.Add(trckdObj.eulerAngles.y);
                //expTrialData.zRot.Add(trckdObj.eulerAngles.z);
            }
            frameNum++;
        }
    }

    private IEnumerator Upload2(string trialName)
    {
        // Convert to json and send to another site on the server
        //expData.conditionInfo = userIDPost;
        //expData.dataTable = recordDataList.ToArray();

        //string jsonString = JsonConvert.SerializeObject(expData, Formatting.Indented);
        string jsonString = JsonConvert.SerializeObject(expTrialData, Formatting.Indented);

        // Save jsonString variable in a file 
        File.WriteAllText(path + "/" + trialName, jsonString);
        yield return new WaitForSecondsRealtime(0.5f);

        // Empty text fields for next trials (potential for issues with next trial)
        //recordDataList.Clear();
        expTrialData = new DataClass(); // Clear class 

        yield return null;
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

    public void ResponseFunction(int res)
    {
        if (res == 0)
            firstResponse = true;
        else if (res == 1)
            secondResponse = true;

        Invoke("ResetResponse", 0.5f);
    }
    public void ConfirmFunction()
    {
        confirm = true;
        Invoke("ResetConfirm", 0.5f);
    }
    void ResetConfirm()
    {
        confirm = false; 
    }
    void ResetResponse()
    {
        firstResponse = false;
        secondResponse = false;
    }

    public void End_Save()
    {
        StartCoroutine(Upload2(participantID + "_standard_" + standardFrequency + "comparisonFrequency" + comparisonFrequency + "_" + UnityEngine.Time.time.ToString("F2") + "_Trial_" + numbTrials.ToString() + "_.json"));
        instructionDisplay.text = "End \n\nThanks for your participation";
        endNsave = true;
        Invoke("ResetEndSave", 2f);
    }
    void ResetEndSave()
    {
        Application.Quit();
        endNsave = false;
    }

    public void ConfirmInfo()
    {
        participantID = studyInfo.text;

        //if (string.IsNullOrEmpty(pathField.text))
        //    path = Application.persistentDataPath;
        //else
        path = pathField.text;
    }

    public void PlayAudio(float dur)
    {
        //int duration = samplerate / Mathf.RoundToInt(1f /dur);
        int duration = 4410;
        //int duration = Mathf.RoundToInt(slider_duration.value);

        AudioClip myClip = AudioClip.Create("MySinusoid", duration, 1, samplerate, true, OnAudioRead, OnAudioSetPosition);
        aud = GetComponent<AudioSource>();
        aud.clip = myClip;

        aud.Play();
    }

    void OnAudioRead(float[] data)
    {
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

    IEnumerator ExperimentSequenceFreq2()
    {
        // Roberta suggestion (3) Breaktime forced at 2.5 minute mark 
        float startTime = UnityEngine.Time.time; 

        for (int i = 0; i < numbTrials; i++)
        {
            ProgressSlider.value = i; // Update progress bar on slider 

            // 1. Set standard frequency based on frequency order i.e. either 30 or 300 
            // 2. Set the current comparison frequency based on the last relevant recorded one i.e.
            // if previous trial had standard frequency of 30 Hz but currenty standard is 300, then compFreq[1] should be used for the current trial 
            if (FreqOrder[i] == 0)
            {
                standardFrequency = 30;
                //if (done30)
                //{
                    comparisonFrequency = comFreq[0];
                //    done30 = false; 
                //}
            }
            else
            {
                standardFrequency = 300;
                //if (done300)
                //{
                    comparisonFrequency = comFreq[1];
                //    done300 = false; 
                //}
            }

            if (StimSequence[i] == 0) // Standard first then comparison stimulus 
            {
                //Debug.Log("Stim first i.e. press A for correct, Freq: " + comparisonFrequency.ToString());

                // Comparison
                instructionDisplay.text = "1st stimulus";
                yield return new WaitForSeconds(0.1f);
                float amp = 2f; // MapFreq2Amp(comparisonFrequency); // Random.Range(0.05f, 0.95f);
                //Signal collision2 = new Sine(comparisonFrequency) * new ASR(0.05, 0.075, 0.05) * amp;
                //syntacts.session.Play(collisionChannel, collision2);
                amplitude = amp;
                frequency = comparisonFrequency;
                PlayAudio(0.08f);
                yield return new WaitForSeconds(1f);

                // Standard
                instructionDisplay.text = "2nd  stimulus";
                yield return new WaitForSeconds(0.1f);
                amp = 3.5f; // Random.Range(0.05f, 0.95f);
                //Signal collision1 = new Sine(standardFrequency) * new ASR(0.05, 0.075, 0.05) * amp;
                //syntacts.session.Play(collisionChannel, collision1);
                amplitude = amp;
                frequency = standardFrequency;
                PlayAudio(0.08f);
                yield return new WaitForSeconds(1f);
            }
            else if (StimSequence[i] == 1) // Comparison stimulus first then standard 
            {
                //Debug.Log("Comparison first i.e. press D for correct, Freq: " + comparisonFrequency.ToString());

                // Standard
                instructionDisplay.text = "1st stimulus";
                yield return new WaitForSeconds(0.1f);
                float amp = 3.5f; // Random.Range(0.05f, 0.95f);
                //Signal collision1 = new Sine(standardFrequency) * new ASR(0.05, 0.075, 0.05) * amp;
                //syntacts.session.Play(collisionChannel, collision1);
                amplitude = amp;
                frequency = standardFrequency;
                PlayAudio(0.08f);
                yield return new WaitForSeconds(1f);

                // Comparison
                instructionDisplay.text = "2nd  stimulus";
                yield return new WaitForSeconds(0.1f);
                amp = 2f; // MapFreq2Amp(comparisonFrequency); // Random.Range(0.05f, 0.95f);
                //Signal collision2 = new Sine(comparisonFrequency) * new ASR(0.05, 0.075, 0.05) * amp;
                //syntacts.session.Play(collisionChannel, collision2);
                amplitude = amp;
                frequency = comparisonFrequency;
                PlayAudio(0.08f);
                yield return new WaitForSeconds(1f);
            }
            
            instructionDisplay.text = "Which of the two stimuli had a higher frequency? \n\nPress A for 1st and D for 2nd";
            //yield return new WaitForSeconds(0.1f);

            // User response choice 
            float startResponseTimer = UnityEngine.Time.time;
            float allowedResponseTime = 5f; 
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.A) | firstResponse)
                {
                    answer = 0;
                    firstResponse = false; 
                    break;
                }
                if (Input.GetKeyDown(KeyCode.D) | secondResponse)
                {
                    answer = 1;
                    secondResponse = false; 
                    break;
                }
                // Roberta suggestion (5) Repeat trial if ppts failes to respond within 5 seconds 
                if((UnityEngine.Time.time - startResponseTimer) > allowedResponseTime)
                {
                    i--;
                    break;
                }
                yield return null;
            }

            // Make a note of the previous comparison freq based on the standard frequency 
            try
            {
                if (FreqOrder[i] == 0) // 30 Hz
                {
                    // Check answer and update stimulus 
                    next_stimulus = CheckAnswerUpdateStimulus_30(StimSequence[i], answer, i, comparisonFrequency);
                    comparisonFrequency = comFreq[0] + next_stimulus;
                    comparisonFrequency = Mathf.Sqrt(comparisonFrequency * comparisonFrequency);
                    //done30 = true;

                    comFreq[0] = comparisonFrequency;
                    //Debug.Log("Corr Row 30: " + correctInARow_30[0] + " : " + correctInARow_30[1]);
                }
                if (FreqOrder[i] == 1) // 300 Hz
                {
                    // Check answer and update stimulus 
                    next_stimulus = CheckAnswerUpdateStimulus_300(StimSequence[i], answer, i, comparisonFrequency);
                    comparisonFrequency = comFreq[1] + next_stimulus;
                    comparisonFrequency = Mathf.Sqrt(comparisonFrequency * comparisonFrequency);
                    //done300 = true;

                    comFreq[1] = comparisonFrequency;
                    //Debug.Log("Corr Row 300: " + correctInARow_300[0] + " : " + correctInARow_300[1]);
                }
            }
            catch
            {
                //Debug.Log("Index out of range exception!!!");
            }

            // Record data 
            expTrialData.user_response.Add(answer);
            expTrialData.correct.Add(checkedAnswer);
            expTrialData.standard_stim.Add(StimSequence[i]);
            expTrialData.comparison_stim.Add(comparisonFrequency);
            expTrialData.standard_freq.Add(standardFrequency);
            expTrialData.comp_freq.Add(comparisonFrequency);
            expTrialData.trialNumber.Add(i);

            yield return new WaitForSeconds(0.01f);
            instructionDisplay.text = "Press S to continue";
            yield return new WaitForSeconds(0.1f);
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.S) | confirm)
                {
                    confirm = false;
                    break;
                }
                yield return null;
            }


            // Roberta suggestion (3) Breaktime 
            if ((UnityEngine.Time.time - startTime) > (2.5f * 60f) &&
                (UnityEngine.Time.time - startTime) < (3f * 60f))
            {
                instructionDisplay.text = "Please take a short break!";
            }

            instructionDisplay.text = "Next stimulus about to start...";
            float waitRandon = Random.Range(0.8f, 1.2f); // Roberta suggestion (1) with random wait time between trials 
            yield return new WaitForSeconds(waitRandon);

            if (endReversals)
                break; 
        }

        StartCoroutine(Upload2(participantID + "_standard_" + standardFrequency + "comparisonFrequency" + comparisonFrequency + "_" + UnityEngine.Time.time.ToString("F2") + "_Trial_" + numbTrials.ToString() + "_.json"));

        instructionDisplay.text = "End \n\nThanks for your participation";
        yield return null;
    }

    // Check Answers 30 freq 
    public float CheckAnswerUpdateStimulus_30(int stimulus_position, int answer, int trial, float compStim)
    {
        float nextStimulus = 0f;

        trial = trial + 4; // Add 4 to avoid crash at the start when trial is 0 (zero) 

        //Debug.Log("Answer: " + "StimPos: " + stimulus_position + " Answer: " + answer + " ComparisonFreq: " + compStim);

        #region Check Answer
        if (answer == stimulus_position & correctcounter_30 == 0)
        {
            correctcounter_30 += 1;
            correctCntHist[trial] = 1;
            //Debug.Log("Correct");
            checkedAnswer = "Correct";

            correctInARow_30[0] = 1;

            //correctInARow[trial % 2] = 1;
            //Debug.Log("Trial: " + trial + " Mod: " + (trial % 2).ToString()); 
        }
        else if (answer == stimulus_position & correctcounter_30 == 1)
        {
            correctCntHist[trial] = 1;
            //nextStimulus -= stepSize;
            correctcounter_30 = 0;
            //Debug.Log("Correct");
            checkedAnswer = "Correct";

            correctInARow_30[1] = 1;

            //correctInARow[trial % 2] = 1;
            //Debug.Log("Trial: " + trial + " Mod: " + (trial % 2).ToString());
        }
        else
        {
            //nextStimulus += stepSize;
            correctcounter_30 = 0;
            correctCntHist[trial] = 0;
            //Debug.Log("Wrong");
            checkedAnswer = "Wrong";

            // Reset correct in a row counter when one wrong 
            correctInARow_30[0] = 0;
            correctInARow_30[1] = 0;
        }
        #endregion

        #region New Reversal Logic
        if (correctCntHist[trial] == 0)
        {
            // Update next stimulus by making it easier i.e. going up in frequency i.e. making the difference bigger between standard and comparison  
            nextStimulus = stepSizeFreq_30[reversalCnt_30];
        }
        if (correctInARow_30[0] == 1 & correctInARow_30[1] == 1)
        {
            // Update next stimulus by making it easier i.e. going up in frequency i.e. making the difference bigger between standard and comparison  
            nextStimulus = -stepSizeFreq_30[reversalCnt_30];

            // Reset correct in a row counter when two correct in a row 
            correctInARow_30[0] = 0;
            correctInARow_30[1] = 0;
        }

        // Create reversal logic 
        if (correctCntHist[trial] == 0 & correctCntHist[trial - 1] == 1 & correctCntHist[trial - 2] == 1)
            reversalCnt_30++;
        if (correctCntHist[trial] == 1 & correctCntHist[trial - 1] == 1 & correctCntHist[trial - 2] == 0)
            reversalCnt_30++;

        #endregion

        #region Reversal counter and Stimulus Update
        //if (correctCntHist[trial + 3] == 0 & correctCntHist[(trial + 3) - 1] == 1 & correctCntHist[(trial + 3) - 2] == 1)
        //{
        //    // print("Reversal: One down"); i.e. make it easier 
        //    reversalCnt_30++;
        //    reversalCnt_300++;
        //    stepDirection = -1;

        //    // Update next stimulus by making it easier i.e. going up in frequency i.e. making the difference bigger between standard and comparison  
        //    if (compStim > 200)
        //    {
        //        nextStimulus = stepSizeFreq_300[reversalCnt_300];
        //    }
        //    else
        //    {
        //        nextStimulus = stepSizeFreq_30[reversalCnt_30];
        //    }
        //}
        //if (correctCntHist[trial + 3] == 0 & correctCntHist[(trial + 3) - 1] == 0 & correctCntHist[(trial + 3) - 2] == 1)
        //{
        //    // print("Reversal: One down"); i.e. make it easier 
        //    stepDirection = -1;

        //    // Update next stimulus by making it easier i.e. going up in frequency i.e. making the difference bigger between standard and comparison  
        //    if (compStim > 200)
        //    {
        //        //nextStimulus = stepDirection * (compStim + stepSizeFreq_300[reversalCnt_300]);
        //        nextStimulus = stepSizeFreq_300[reversalCnt_300];
        //    }
        //    else
        //    {
        //        //nextStimulus = stepDirection * (compStim + stepSizeFreq_30[reversalCnt_30]);
        //        nextStimulus = stepSizeFreq_30[reversalCnt_30];
        //    }
        //}
        //if (correctCntHist[trial + 3] == 0 & correctCntHist[(trial + 3) - 1] == 0) // & correctCntHist[(trial + 3) - 2] == 0)
        //{
        //    // print("Reversal: One down"); i.e. make it easier 
        //    stepDirection = -1;

        //    // Update next stimulus by making it easier i.e. going up in frequency i.e. making the difference bigger between standard and comparison  
        //    if (compStim > 200)
        //    {
        //        //nextStimulus = stepDirection * (compStim + stepSizeFreq_300[reversalCnt_300]);
        //        nextStimulus = stepSizeFreq_300[reversalCnt_300];
        //    }
        //    else
        //    {
        //        //nextStimulus = stepDirection * (compStim + stepSizeFreq_30[reversalCnt_30]);
        //        nextStimulus = stepSizeFreq_30[reversalCnt_30];
        //    }

        //}
        //if (correctCntHist[trial + 3] == 1 & correctCntHist[(trial + 3) - 1] == 1 & correctCntHist[(trial + 3) - 2] == 0)
        //{
        //    // print("Reversal: Two up");
        //    reversalCnt_30++;
        //    reversalCnt_300++;
        //    stepDirection = 1;

        //    // Update next stimulus by making it harder i.e. going down in frequency i.e. making the difference smaller between standard and comparison 
        //    if (compStim > 200)
        //    {
        //        //nextStimulus = stepDirection * (compStim + stepSizeFreq_300[reversalCnt_300]);
        //        nextStimulus = -stepSizeFreq_300[reversalCnt_300];
        //    }
        //    else
        //    {
        //        //nextStimulus = stepDirection * (compStim + stepSizeFreq_30[reversalCnt_30]);
        //        nextStimulus = -stepSizeFreq_30[reversalCnt_30];
        //    }
        //}
        ////if (correctCntHist[trial + 3] == 1 & correctCntHist[(trial + 3) - 1] == 1) // & correctCntHist[(trial + 3) - 2] == 1 & correctCntHist[(trial + 3) - 3] == 1)
        ////{
        ////    // print("Reversal: Two up");
        ////    //reversalCnt_30++;
        ////    //reversalCnt_300++;
        ////    stepDirection = 1;

        ////    // Update next stimulus by making it harder i.e. going down in frequency i.e. making the difference smaller between standard and comparison 
        ////    if (compStim > 200)
        ////    {
        ////        //nextStimulus = stepDirection * (compStim + stepSizeFreq_300[reversalCnt_300]);
        ////        nextStimulus = -stepSizeFreq_300[reversalCnt_300];
        ////    }
        ////    else
        ////    {
        ////        //nextStimulus = stepDirection * (compStim + stepSizeFreq_30[reversalCnt_30]);
        ////        nextStimulus = -stepSizeFreq_30[reversalCnt_30];
        ////    }
        ////}
        ////if (correctCntHist[trial + 3] == 0 & correctCntHist[(trial + 3) - 1] == 1 & correctCntHist[(trial + 3) - 2] == 0)
        ////{
        ////    stepDirection = 1;
        ////    // print("Not reversal");
        ////    // Make it easier 
        ////}
        ////if (correctCntHist[trial + 3] == 1 & correctCntHist[(trial + 3) - 1] == 1 & correctCntHist[(trial + 3) - 2] == 1)
        ////{
        ////    stepDirection = 1;
        ////    // print("Not reversal");
        ////}
        #endregion

        #region Reversal Slider for Display purposes
        // if answer is wrong once then update the stimulus 
        //comparisonFrequency = UpdateStimulusFreq(standard_frequency, comparisonFrequency);
        if (reversalCnt_30 < reversalCnt_300)
            ReversalSlider.value = reversalCnt_30; 
        else
            ReversalSlider.value = reversalCnt_300;

        if (reversalCnt_30 >= reversals & reversalCnt_300 >= reversals)
            endReversals = true;
        #endregion

        return nextStimulus;
    }

    public float CheckAnswerUpdateStimulus_300(int stimulus_position, int answer, int trial, float compStim)
    {
        float nextStimulus = 0f;
        trial = trial + 4; // Add 4 to avoid crash at the start when trial is 0 (zero) 

        //Debug.Log("Answer: " + "StimPos: " + stimulus_position + " Answer: " + answer + " ComparisonFreq: " + compStim);

        #region Check Answer
        if (answer == stimulus_position & correctcounter_300 == 0)
        {
            correctcounter_300 += 1;
            correctCntHist[trial] = 1;
            //Debug.Log("Correct");
            checkedAnswer = "Correct";

            correctInARow_300[0] = 1;

            //correctInARow[trial % 2] = 1;
            //Debug.Log("Trial: " + trial + " Mod: " + (trial % 2).ToString()); 
        }
        else if (answer == stimulus_position & correctcounter_300 == 1)
        {
            correctCntHist[trial] = 1;
            //nextStimulus -= stepSize;
            correctcounter_300 = 0;
            //Debug.Log("Correct");
            checkedAnswer = "Correct";

            correctInARow_300[1] = 1;

            //correctInARow[trial % 2] = 1;
            //Debug.Log("Trial: " + trial + " Mod: " + (trial % 2).ToString());
        }
        else
        {
            //nextStimulus += stepSize;
            correctcounter_300 = 0;
            correctCntHist[trial] = 0;
            //Debug.Log("Wrong");
            checkedAnswer = "Wrong";

            // Reset correct in a row counter when one wrong 
            correctInARow_300[0] = 0;
            correctInARow_300[1] = 0;
        }
        #endregion

        #region New Reversal Logic
        if (correctCntHist[trial] == 0)
        {
            // Update next stimulus by making it easier i.e. going up in frequency i.e. making the difference bigger between standard and comparison  
            nextStimulus = stepSizeFreq_300[reversalCnt_300];
        }
        if (correctInARow_300[0] == 1 & correctInARow_300[1] == 1)
        {
            // Update next stimulus by making it easier i.e. going up in frequency i.e. making the difference bigger between standard and comparison  
            nextStimulus = -stepSizeFreq_300[reversalCnt_300];


            // Reset correct in a row counter when two correct in a row 
            correctInARow_300[0] = 0;
            correctInARow_300[1] = 0;
        }

        // Create reversal logic 
        if (correctCntHist[trial] == 0 & correctCntHist[trial - 1] == 1 & correctCntHist[trial - 2] == 1)
            reversalCnt_300++;
        if (correctCntHist[trial] == 1 & correctCntHist[trial - 1] == 1 & correctCntHist[trial - 2] == 0)
            reversalCnt_300++;
        #endregion

        #region Reversal counter and Stimulus Update
        //if (correctCntHist[trial + 3] == 0 & correctCntHist[(trial + 3) - 1] == 1 & correctCntHist[(trial + 3) - 2] == 1)
        //{
        //    // print("Reversal: One down"); i.e. make it easier 
        //    reversalCnt_30++;
        //    reversalCnt_300++;
        //    stepDirection = -1;

        //    // Update next stimulus by making it easier i.e. going up in frequency i.e. making the difference bigger between standard and comparison  
        //    if (compStim > 200)
        //    {
        //        nextStimulus = stepSizeFreq_300[reversalCnt_300];
        //    }
        //    else
        //    {
        //        nextStimulus = stepSizeFreq_30[reversalCnt_30];
        //    }
        //}
        //if (correctCntHist[trial + 3] == 0 & correctCntHist[(trial + 3) - 1] == 0 & correctCntHist[(trial + 3) - 2] == 1)
        //{
        //    // print("Reversal: One down"); i.e. make it easier 
        //    stepDirection = -1;

        //    // Update next stimulus by making it easier i.e. going up in frequency i.e. making the difference bigger between standard and comparison  
        //    if (compStim > 200)
        //    {
        //        //nextStimulus = stepDirection * (compStim + stepSizeFreq_300[reversalCnt_300]);
        //        nextStimulus = stepSizeFreq_300[reversalCnt_300];
        //    }
        //    else
        //    {
        //        //nextStimulus = stepDirection * (compStim + stepSizeFreq_30[reversalCnt_30]);
        //        nextStimulus = stepSizeFreq_30[reversalCnt_30];
        //    }
        //}
        //if (correctCntHist[trial + 3] == 0 & correctCntHist[(trial + 3) - 1] == 0) // & correctCntHist[(trial + 3) - 2] == 0)
        //{
        //    // print("Reversal: One down"); i.e. make it easier 
        //    stepDirection = -1;

        //    // Update next stimulus by making it easier i.e. going up in frequency i.e. making the difference bigger between standard and comparison  
        //    if (compStim > 200)
        //    {
        //        //nextStimulus = stepDirection * (compStim + stepSizeFreq_300[reversalCnt_300]);
        //        nextStimulus = stepSizeFreq_300[reversalCnt_300];
        //    }
        //    else
        //    {
        //        //nextStimulus = stepDirection * (compStim + stepSizeFreq_30[reversalCnt_30]);
        //        nextStimulus = stepSizeFreq_30[reversalCnt_30];
        //    }

        //}
        //if (correctCntHist[trial + 3] == 1 & correctCntHist[(trial + 3) - 1] == 1 & correctCntHist[(trial + 3) - 2] == 0)
        //{
        //    // print("Reversal: Two up");
        //    reversalCnt_30++;
        //    reversalCnt_300++;
        //    stepDirection = 1;

        //    // Update next stimulus by making it harder i.e. going down in frequency i.e. making the difference smaller between standard and comparison 
        //    if (compStim > 200)
        //    {
        //        //nextStimulus = stepDirection * (compStim + stepSizeFreq_300[reversalCnt_300]);
        //        nextStimulus = -stepSizeFreq_300[reversalCnt_300];
        //    }
        //    else
        //    {
        //        //nextStimulus = stepDirection * (compStim + stepSizeFreq_30[reversalCnt_30]);
        //        nextStimulus = -stepSizeFreq_30[reversalCnt_30];
        //    }
        //}
        ////if (correctCntHist[trial + 3] == 1 & correctCntHist[(trial + 3) - 1] == 1) // & correctCntHist[(trial + 3) - 2] == 1 & correctCntHist[(trial + 3) - 3] == 1)
        ////{
        ////    // print("Reversal: Two up");
        ////    //reversalCnt_30++;
        ////    //reversalCnt_300++;
        ////    stepDirection = 1;

        ////    // Update next stimulus by making it harder i.e. going down in frequency i.e. making the difference smaller between standard and comparison 
        ////    if (compStim > 200)
        ////    {
        ////        //nextStimulus = stepDirection * (compStim + stepSizeFreq_300[reversalCnt_300]);
        ////        nextStimulus = -stepSizeFreq_300[reversalCnt_300];
        ////    }
        ////    else
        ////    {
        ////        //nextStimulus = stepDirection * (compStim + stepSizeFreq_30[reversalCnt_30]);
        ////        nextStimulus = -stepSizeFreq_30[reversalCnt_30];
        ////    }
        ////}
        ////if (correctCntHist[trial + 3] == 0 & correctCntHist[(trial + 3) - 1] == 1 & correctCntHist[(trial + 3) - 2] == 0)
        ////{
        ////    stepDirection = 1;
        ////    // print("Not reversal");
        ////    // Make it easier 
        ////}
        ////if (correctCntHist[trial + 3] == 1 & correctCntHist[(trial + 3) - 1] == 1 & correctCntHist[(trial + 3) - 2] == 1)
        ////{
        ////    stepDirection = 1;
        ////    // print("Not reversal");
        ////}
        #endregion

        #region Reversal Slider for Display purposes
        // if answer is wrong once then update the stimulus 
        //comparisonFrequency = UpdateStimulusFreq(standard_frequency, comparisonFrequency);
        if (reversalCnt_30 < reversalCnt_300)
            ReversalSlider.value = reversalCnt_30;
        else
            ReversalSlider.value = reversalCnt_300;

        if (reversalCnt_30 >= reversals & reversalCnt_300 >= reversals)
            endReversals = true;
        #endregion

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
            if (Input.GetKeyDown(KeyCode.S) | confirm)
            {
                confirm = false; 
                break;
            }
            yield return null;
        }

        instructionDisplay.text = "You will receive a pair of stimuli seperated by a 1 second pause, then answer the displayed question.";
        yield return new WaitForSeconds(delayBetweenInstructions);
        instructionDisplay.text = "You will receive a pair of stimuli seperated by a 1 second pause, then answer the displayed question. \n \n Press the 'S' key to continue!";
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.S) | confirm)
            {
                confirm = false;
                break;
            }
            yield return null;
        }

        instructionDisplay.text = "This process repeats until the experiment comes to an end.";
        yield return new WaitForSeconds(delayBetweenInstructions);
        instructionDisplay.text = "This process repeats until the experiment comes to an end. \n \n Press the 'S' key to start this experiment!";
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.S) | confirm)
            {
                confirm = false;
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
            amp = 5.7f;
        if (frequency < 370 & frequency >= 350)
            amp = 4.7f;
        if (frequency < 350 & frequency > 345)
            amp = 3.9f;
        if (frequency <= 345 & frequency > 310)
            amp = 3.3f;
        if (frequency <= 310 & frequency > 300)
            amp = 3.2f;
        if (frequency <= 300 & frequency > 290)
            amp = 2f;
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













    //**************************************************************************************
    //************************************ Old codes ***************************************
    //**************************************************************************************
    #region Old Code
    //IEnumerator ExperimentSequenceFreq()
    //{
    //    for (int i = 0; i < numbTrials; i++)
    //    {
    //        //DebugActuator();
    //        #region Original code (not working, because second stimulus never runs)
    //        float amplitude = 1f;

    //        // Randomly select which one of the two frequencies to use
    //        int standard_frequency = 0;

    //        // Select which stimulus amplitude first (standard(reference) or stimulus amplitude first? 

    //        if (StimSequence[i] == 0)
    //        {
    //            if (FreqOrder[i] == 0)
    //            {
    //                standard_frequency = frequencies[0];
    //                // Set initial comparison stimuli
    //                if (firstTime30)
    //                {
    //                    comparisonFrequency = 80; // Initial comparison freq.
    //                    firstTime30 = false;
    //                }

    //                // Standard
    //                instructionDisplay.text = "1st stimulus";
    //                yield return new WaitForSeconds(0.5f);
    //                Signal collision2 = new Sine(50);
    //                amplitude = 3.5f; // Random.Range(0.05f, 0.95f);
    //                collision2 = new Sine(standard_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
    //                syntacts.session.Play(collisionChannel, collision2);
    //                yield return new WaitForSeconds(0.65f);

    //                // Comparison
    //                instructionDisplay.text = "2nd stimulus";
    //                yield return new WaitForSeconds(0.5f);
    //                Signal collision1 = new Sine(50);
    //                amplitude = 1f; // MapFreq2Amp(comparisonFrequency); // Random.Range(0.05f, 0.95f);
    //                collision1 = new Sine(comparisonFrequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
    //                syntacts.session.Play(collisionChannel, collision1);
    //                yield return new WaitForSeconds(0.5f);

    //                instructionDisplay.text = "Which of the two stimuli had a higher frequency? \n\nPress A for 1st and D for 2nd";
    //                yield return new WaitForSeconds(0.1f);
    //                // Check answer and adjust next stimuli step size based on this
    //                while (true)
    //                {
    //                    if (Input.GetKeyDown(KeyCode.A))
    //                    {
    //                        answer = 0;
    //                        break;
    //                    }
    //                    if (Input.GetKeyDown(KeyCode.D))
    //                    {
    //                        answer = 1;
    //                        break;
    //                    }
    //                    yield return null;
    //                }
    //                Debug.Log("User Resp: " + answer + " Stimulus: " + FreqOrder[i] + " Amp: " + amp);
    //                int trialNum = i;
    //                next_stimulus = CheckAnswerUpdateStimulus(FreqOrder[i], answer, i, comparisonFrequency);
    //                comparisonFrequency = comparisonFrequency + next_stimulus;
    //                comparisonFrequency = Mathf.Sqrt(comparisonFrequency * comparisonFrequency);
    //                //amp += next_stimulus;
    //                Debug.Log("Comparison frequency 300: " + comparisonFrequency + " Standard freq: " + standard_frequency + " Amp: " + amplitude);
    //            }
    //            else if (FreqOrder[i] == 1)
    //            {
    //                standard_frequency = frequencies[1];
    //                // Set initial comparison stimuli
    //                if (firstTime300)
    //                {
    //                    comparisonFrequency300 = 400f; // Initial comparison freq.
    //                    firstTime300 = false;
    //                }

    //                // Standard
    //                instructionDisplay.text = "1st stimulus";
    //                yield return new WaitForSeconds(0.5f);
    //                Signal collision2 = new Sine(50);
    //                amplitude = 1f; // Random.Range(0.05f, 0.95f);
    //                collision2 = new Sine(standard_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
    //                syntacts.session.Play(collisionChannel, collision2);
    //                yield return new WaitForSeconds(0.65f);

    //                // Comparison
    //                instructionDisplay.text = "2nd stimulus";
    //                yield return new WaitForSeconds(0.5f);
    //                Signal collision1 = new Sine(50);
    //                amplitude = 1f; // MapFreq2Amp(comparisonFrequency300); // Random.Range(0.05f, 0.95f);
    //                collision1 = new Sine(comparisonFrequency300) * new ASR(0.05, 0.075, 0.05) * amplitude;
    //                syntacts.session.Play(collisionChannel, collision1);
    //                yield return new WaitForSeconds(0.5f);

    //                instructionDisplay.text = "Which of the two stimuli had a higher frequency? \n\nPress A for 1st and D for 2nd";
    //                yield return new WaitForSeconds(0.1f);
    //                // Check answer and adjust next stimuli step size based on this
    //                while (true)
    //                {
    //                    if (Input.GetKeyDown(KeyCode.A))
    //                    {
    //                        answer = 0;
    //                        break;
    //                    }
    //                    if (Input.GetKeyDown(KeyCode.D))
    //                    {
    //                        answer = 1;
    //                        break;
    //                    }
    //                    yield return null;
    //                }
    //                Debug.Log("User Resp: " + answer + " Stimulus: " + FreqOrder[i] + " Amp: " + amp);
    //                int trialNum = i;
    //                next_stimulus = CheckAnswerUpdateStimulus(FreqOrder[i], answer, i, comparisonFrequency300);
    //                comparisonFrequency300 = comparisonFrequency300 + next_stimulus;
    //                comparisonFrequency300 = Mathf.Sqrt(comparisonFrequency300 * comparisonFrequency300);
    //                //amp += next_stimulus;
    //                Debug.Log("Comparison frequency 300: " + comparisonFrequency300 + " Standard freq: " + standard_frequency + " Amp: " + amplitude);
    //            }
    //        }
    //        if (StimSequence[i] == 1)
    //        {
    //            if (FreqOrder[i] == 0)
    //            {
    //                standard_frequency = frequencies[0];
    //                // Set initial comparison stimuli
    //                if (firstTime30)
    //                {
    //                    comparisonFrequency = 80; // Initial comparison freq.
    //                    firstTime30 = false;
    //                }

    //                // Comparison
    //                instructionDisplay.text = "1st stimulus";
    //                yield return new WaitForSeconds(0.5f);
    //                Signal collision1 = new Sine(50);
    //                amplitude = 1f; // MapFreq2Amp(comparisonFrequency); // Random.Range(0.05f, 0.95f);
    //                collision1 = new Sine(comparisonFrequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
    //                syntacts.session.Play(collisionChannel, collision1);
    //                yield return new WaitForSeconds(0.5f);

    //                // Standard
    //                instructionDisplay.text = "2nd stimulus";
    //                yield return new WaitForSeconds(0.5f);
    //                Signal collision2 = new Sine(50);
    //                amplitude = 3.5f; // Random.Range(0.05f, 0.95f);
    //                collision2 = new Sine(standard_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
    //                syntacts.session.Play(collisionChannel, collision2);
    //                yield return new WaitForSeconds(0.65f);


    //                instructionDisplay.text = "Which of the two stimuli had a higher frequency? \n\nPress A for 1st and D for 2nd";
    //                yield return new WaitForSeconds(0.1f);
    //                // Check answer and adjust next stimuli step size based on this
    //                while (true)
    //                {
    //                    if (Input.GetKeyDown(KeyCode.A))
    //                    {
    //                        answer = 0;
    //                        break;
    //                    }
    //                    if (Input.GetKeyDown(KeyCode.D))
    //                    {
    //                        answer = 1;
    //                        break;
    //                    }
    //                    yield return null;
    //                }
    //                Debug.Log("User Resp: " + answer + " Stimulus: " + FreqOrder[i] + " Amp: " + amp);
    //                int trialNum = i;
    //                next_stimulus = CheckAnswerUpdateStimulus(FreqOrder[i], answer, i, comparisonFrequency);
    //                comparisonFrequency = comparisonFrequency + next_stimulus;
    //                comparisonFrequency = Mathf.Sqrt(comparisonFrequency * comparisonFrequency);
    //                //amp += next_stimulus;
    //                Debug.Log("Comparison frequency 300: " + comparisonFrequency + " Standard freq: " + standard_frequency + " Amp: " + amplitude);
    //            }
    //            else if (FreqOrder[i] == 1)
    //            {
    //                standard_frequency = frequencies[1];
    //                // Set initial comparison stimuli
    //                if (firstTime300)
    //                {
    //                    comparisonFrequency300 = 400f; // Initial comparison freq.
    //                    firstTime300 = false;
    //                }

    //                // Comparison
    //                instructionDisplay.text = "1st stimulus";
    //                yield return new WaitForSeconds(0.5f);
    //                Signal collision1 = new Sine(50);
    //                amplitude = 1f; // MapFreq2Amp(comparisonFrequency300); // Random.Range(0.05f, 0.95f);
    //                collision1 = new Sine(comparisonFrequency300) * new ASR(0.05, 0.075, 0.05) * amplitude;
    //                syntacts.session.Play(collisionChannel, collision1);
    //                yield return new WaitForSeconds(0.5f);

    //                // Standard
    //                instructionDisplay.text = "2nd stimulus";
    //                yield return new WaitForSeconds(0.5f);
    //                Signal collision2 = new Sine(50);
    //                amplitude = 1f; // Random.Range(0.05f, 0.95f);
    //                collision2 = new Sine(standard_frequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
    //                syntacts.session.Play(collisionChannel, collision2);
    //                yield return new WaitForSeconds(0.65f);


    //                instructionDisplay.text = "Which of the two stimuli had a higher frequency? \n\nPress A for 1st and D for 2nd";
    //                yield return new WaitForSeconds(0.1f);
    //                // Check answer and adjust next stimuli step size based on this
    //                while (true)
    //                {
    //                    if (Input.GetKeyDown(KeyCode.A))
    //                    {
    //                        answer = 0;
    //                        break;
    //                    }
    //                    if (Input.GetKeyDown(KeyCode.D))
    //                    {
    //                        answer = 1;
    //                        break;
    //                    }
    //                    yield return null;
    //                }
    //                Debug.Log("User Resp: " + answer + " Stimulus: " + FreqOrder[i] + " Amp: " + amp);
    //                int trialNum = i;
    //                next_stimulus = CheckAnswerUpdateStimulus(FreqOrder[i], answer, i, comparisonFrequency300);
    //                comparisonFrequency300 = comparisonFrequency300 + next_stimulus;
    //                comparisonFrequency300 = Mathf.Sqrt(comparisonFrequency300 * comparisonFrequency300);
    //                //amp += next_stimulus;
    //                Debug.Log("Comparison frequency 300: " + comparisonFrequency300 + " Standard freq: " + standard_frequency + " Amp: " + amplitude);
    //            }
    //        }


    //        yield return new WaitForSeconds(0.1f);
    //        instructionDisplay.text = "Press S to continue";
    //        yield return new WaitForSeconds(0.5f);
    //        while (true)
    //        {
    //            if (Input.GetKeyDown(KeyCode.S))
    //                break;
    //            yield return null;
    //        }
    //        #endregion
    //    }

    //    instructionDisplay.text = "End \n\nThanks for your participation";

    //    yield return null;
    //}

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

            //Debug.Log("User Resp: " + answer + " Stimulus: " + StimSequence[i] + " Amp: " + amp + " Freq: " + FreqOrder[i]);

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
            //Debug.Log("Correct");
        }
        else if (answer == stimulus_position & correctcounter == 1)
        {
            nextStimulus -= stepSize;
            correctcounter = 0;
            //Debug.Log("Correct");
        }
        else
        {
            nextStimulus += stepSize;
            correctcounter = 0;
            //Debug.Log("Wrong");
        }
        // if answer if correct twice then update the stimulus intensity

        // if answer is wrong once then update the stimulus 

        return nextStimulus;
    }

    #endregion
}



//else if (StimSequence[i] == 1)
//{
//    standard_frequency = frequencies[0];

//    if (firstTime30)
//    {
//        comparisonFrequency = 80f; // Initial comparison freq.
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
//    amplitude = /*MapFreq2Amp*/(comparisonFrequency); // Random.Range(0.05f, 0.95f);
//    collision1 = new Sine(comparisonFrequency) * new ASR(0.05, 0.075, 0.05) * amplitude;
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
//    next_stimulus = CheckAnswerUpdateStimulus(FreqOrder[i], i, comparisonFrequency);
//    comparisonFrequency = comparisonFrequency + next_stimulus;
//    comparisonFrequency = Mathf.Sqrt(comparisonFrequency * comparisonFrequency);
//    //amp += next_stimulus;
//    Debug.Log("Comparison frequency 30: " + comparisonFrequency + " Standard freq: " + standard_frequency + " Amp: " + amplitude);

//}