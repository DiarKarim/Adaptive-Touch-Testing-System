/*
 *** Oculus Haptic Latency Testing *** 
 * Experiment designed to test latency perception in a haptic task 
 * Participants are asked to respond to interactions with a robotic spring with a artificial delay
 * 
 * Experimental paradigm: 3 Alternative Forced Choice Task (3-AFC task) 
 * 
 * 
 * Author: Diar Karim
 * Date: 01/10/2019-25/01/2019
 * Contact: diarkarim@gmail.com
 * Version 7.0 
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

public class Stair
{
    public float up;
    public float down; // down means down the stimulus intensity i.e. more difficult 
    public float newValue;
    public float oldValue;
    public float factor;
    public bool correctAnswer;
    public int correctB4Change;
    public int correctCounter;
    public float maxValue;
    public int direction;
    public int reversalCounter;
    public float stepSize;
    public float x2_StepSize;
    public int prevDirection;
    public float stepValue;
    public float oldStepValue;
    public float minStepSize;
    public float maxStepSize;
    public int conseqCrrctCntr;
    public int conseqFalseCntr;

    public Stair()
    {
        this.up = 1.0f;
        this.down = 2.0f;
        this.factor = 1.25f;
        this.newValue = 0.045f;
        this.oldValue = 0.045f;
        this.maxValue = 0.08f;
        this.correctAnswer = false;
        this.correctB4Change = 3;
        this.correctCounter = 0;
        this.reversalCounter = 0;
        this.stepSize = 0.015f;
        this.x2_StepSize = this.stepSize;
        this.direction = 0; // 0 = increasing stimulus intensity (easier) and 1 = decreasing stimulus intensity (more difficult)
        this.prevDirection = 0;
        this.stepValue = 0.05f;
        this.oldStepValue = 0.05f;
        this.conseqCrrctCntr = 0;
        this.conseqFalseCntr = 0;
    }

    public float GetNewValue()
    {
        if (correctAnswer)
        {
            correctCounter++;
            if (correctCounter == correctB4Change)
            {
                newValue = oldValue / (Mathf.Pow(factor, 1 / down));
                correctCounter = 0;
            }
            if (newValue >= maxValue)
            {
                newValue = maxValue;
            }
        }
        else if (!correctAnswer)
        {
            newValue = oldValue * factor;
            correctCounter = 0;
            if (newValue >= maxValue)
            {
                newValue = maxValue;
            }
        }
        if (newValue >= maxValue)
        {
            newValue = maxValue;
            correctCounter = 0;
        }

        return newValue;
    }

    public float GetStepValue()
    {
        if (correctAnswer)
        {
            //direction = 1; // *** Either put this line here or *
            correctCounter++;
            if (correctCounter >= correctB4Change && conseqCrrctCntr < 3)
            {
                direction = 1; // *** Put the direction line here see above tripple start comment line * 
                stepValue = oldStepValue - stepSize;
                correctCounter = 0;
                conseqFalseCntr = 0;
                conseqCrrctCntr++;
            }
            if (correctCounter >= correctB4Change && conseqCrrctCntr >= 3)
            {
                x2_StepSize = stepSize * 2;
                stepValue = oldStepValue - x2_StepSize; // If ptx keeps guessing correctly, then accelerate the step size by a factor of two
                conseqFalseCntr = 0;
                correctCounter = 0;
            }
        }
        else if (!correctAnswer)
        {
            if (conseqFalseCntr < 3)
            {
                direction = 0;
                stepValue = oldStepValue + stepSize;
                correctCounter = 0;
                conseqCrrctCntr = 0;
                conseqFalseCntr++;
            }
            else if (conseqFalseCntr >= 3)
            {
                x2_StepSize = stepSize * 2;
                stepValue = oldStepValue + x2_StepSize; // If ptx keeps guessing incorrectly, then accelerate the step size by a factor of two 
                correctCounter = 0;
                conseqCrrctCntr = 0;
                conseqFalseCntr = 0;
            }

        }

        if (direction != prevDirection) // Detect change in direction and count it
        {
            if (direction == prevDirection) // && reversalCounter > 0 // Getting worse i.e. increase stimulus intensity (less difficult)
            {
                stepSize = stepSize;
            }
            else if (direction > prevDirection) //  && reversalCounter > 0 //Getting better i.e. decrease stimulus intensity (more difficult)
            {
                stepSize = stepSize / 2;
            }
            reversalCounter++;

        }


        // Limit step size to a max set step size value 
        if (stepSize <= minStepSize)
        {
            stepSize = minStepSize;
        }
        if (stepSize >= maxStepSize)
        {
            stepSize = maxStepSize;
        }
        // Limit step value to a max delay value 
        if (stepValue > maxValue)
        {
            stepValue = maxValue;
        }
        // Limit step value to a min delay value 
        if (stepValue <= 0.0f)
        {
            stepValue = 0.0f;
        }

        return stepValue;
    }

}

public class AdaptiveStaircase : MonoBehaviour
{

    //public GameObject[] stimuli;
    public Stair stair_1N = new Stair();
    //public Stair stair_5N = new Stair();

    public float stepSize;
    public float minStepSize = 0.005f;
    public float maxStepSize = 0.015f;
    public float initialDelay;
    public float decayFactor = 1.1f;
    public float maxVal = 0.065f;

    public int endAfterReversal = 8;
    public string directoryPath = "Assets/Resources/";
    public int numTrials = 15; // Total number of trials 

    public float TargetForce;
    public string fileName = "pilot_00";
    public int ptxID = 0;


    private int BlockSize = 40;

    private string path_file;
    private float[] Forces;


    private int repeatValue = 2;
    private int repeatForces = 1;

    private int[] StimSequence;
    private float[] ForceSequence;

    private string[] stimSeqOrder;

    private string[] stim;
    private int trialNum = 0;

    void Start()
    {
        //directoryPath = directoryPath + "0"+ptxID.ToString(); 
        stim = new string[3] { "A", "S", "D" };

        fileName = fileName + "_force_" + TargetForce.ToString() + "N";
        path_file = directoryPath + fileName + "_ptx_" + ptxID.ToString() + ".txt";

        // Stimulation sequence 
        StimSequence = new int[numTrials];
        StimSequence = CreateStimSequeces(StimSequence, repeatValue);
        StimSequence = Shuffle(StimSequence); // Randomise stim sequence array 

        //ForceSequence = new float[numTrials];
        //ForceSequence = CreateStimBlocks();

        //foreach (int x in ForceSequence)
        //{
        //    Debug.Log("Force: " + x);
        //}

        //int cnt = 0; 
        //foreach (int stimIdx in StimSequence)
        //{
        //    Debug.Log("Idx: " + cnt.ToString() + "\tStim: " + stimIdx.ToString());
        //    cnt++;
        //}

        //// Defire stair parameters
        // 1N Staircase 

        //stair_1N.factor = decayFactor; 
        //stair_1N.oldValue = 0.045f;
        //stair_1N.newValue = 0.045f;
        stair_1N.correctB4Change = 3;
        stair_1N.correctCounter = 0;
        stair_1N.maxValue = maxVal / 1000.0f; ;
        stair_1N.minStepSize = minStepSize / 1000.0f;
        stair_1N.maxStepSize = maxStepSize / 1000.0f;

        stair_1N.oldStepValue = initialDelay / 1000.0f;
        stair_1N.stepValue = initialDelay / 1000.0f;
        stair_1N.stepSize = stepSize / 1000.0f;

        //// 5N Staircase 
        //stair_5N.factor = decayFactor;
        //stair_5N.oldValue = 0.045f;
        //stair_5N.newValue = 0.045f;
        //stair_5N.correctB4Change = 3;
        //stair_5N.correctCounter = 0;
        //stair_5N.maxValue = maxVal;
        //stair_5N.minStepSize = minStepSize;
        //stair_1N.maxStepSize = maxStepSize;

        //stair_5N.oldStepValue = 0.02f;
        //stair_5N.stepValue = initialDelay;
        //stair_5N.stepSize = stepSize;

        File.AppendAllText(path_file, "Trial Number: " + " Step value: " + " Correct answer:  \n");

    }

    private void Update()
    {
        // Initialize experiment (very important) 
        if (Input.GetKeyDown(KeyCode.N))
        {
            PresentStimuli();
            WriteToFile();
            trialNum++;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            string userAnswer = "A";
            CheckAnswer(userAnswer);

            WriteToFile();

            trialNum++;
            PresentStimuli();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            string userAnswer = "S";
            CheckAnswer(userAnswer);

            WriteToFile();
            trialNum++;
            PresentStimuli();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            string userAnswer = "D";
            CheckAnswer(userAnswer);

            WriteToFile();

            trialNum++;
            PresentStimuli();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Trial number: " + trialNum.ToString());
            Debug.Log("Reversal count 1N: " + stair_1N.reversalCounter);
            //Debug.Log("Reversal count 5N: " + stair_5N.reversalCounter);

            //trialNum++;
            //PresentStimuli();
        }

        // End experiment
        if (trialNum >= numTrials) //(stair_1N.reversalCounter == endAfterReversal || trialNum == (numTrials / 2))
        {
            Debug.Log("*** Experiment ended after set number of trials: " + trialNum.ToString());
            UnityEditor.EditorApplication.isPlaying = false;
        }
        //if (stair_5N.reversalCounter == endAfterReversal || trialNum == numTrials / 2)
        //{
        //    Debug.Log("*** Experiment ended after set number of reversals: " + trialNum.ToString());
        //    UnityEditor.EditorApplication.isPlaying = false;
        //}

        if (trialNum == numTrials - 1)
        {
            Debug.Log("*** Experiment Ended. Thank you :) ***");
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }

    // ************************************************************
    // My functions ***********************************************

    public void WriteToFile()
    {
        // If force Block 1N
        //if (ForceSequence[trialNum] == Forces[0])
        //{
        File.AppendAllText(path_file, trialNum.ToString() + " " + stair_1N.stepValue.ToString() + " " + stair_1N.correctAnswer.ToString() + " " + TargetForce.ToString() + " \n");
        //}
        //// If force Block 5N
        //if (ForceSequence[trialNum] == Forces[1])
        //{
        //    File.AppendAllText(path_file, trialNum.ToString() + " " + stair_5N.stepValue.ToString() + " " + stair_5N.correctAnswer.ToString() + " " + ForceSequence[trialNum] + " \n");
        //}
    }

    public void CheckAnswer(string UserAnswer)
    {
        //// If force Block 1N
        //if (ForceSequence[trialNum] == Forces[0])
        //{
        if (stim[StimSequence[trialNum]] == UserAnswer)
        {
            stair_1N.correctAnswer = true;
        }
        else
        {
            stair_1N.correctAnswer = false;
        }

        //stair_1N.oldValue = stair_1N.newValue;
        //stair_1N.newValue = stair_1N.GetNewValue();

        stair_1N.prevDirection = stair_1N.direction;
        stair_1N.oldStepValue = stair_1N.stepValue;
        stair_1N.stepValue = stair_1N.GetStepValue();
        //if (stair_1N.stepValue >= stair_1N.maxValue)
        //{
        //    stair_1N.stepValue = stair_1N.maxValue;
        //}

        float new_delay = stair_1N.stepValue * 1000.0f;
        float prev_delay = stair_1N.oldStepValue * 1000.0f;
        float curr_step = stair_1N.stepSize * 1000f;
        Debug.Log("Answer: " + stair_1N.correctAnswer.ToString() + " \n Delay: " + new_delay.ToString() + " ms \n");
        Debug.Log("Prev_Del: " + prev_delay.ToString() + " ms \n Step_size: " + curr_step.ToString());

        //}
        //// If force Block 5N
        //if (ForceSequence[trialNum] == Forces[1])
        //{
        //    if (stim[StimSequence[trialNum]] == UserAnswer)
        //    {
        //        stair_5N.correctAnswer = true;
        //    }
        //    else
        //    {
        //        stair_5N.correctAnswer = false;
        //    }

        //    stair_5N.oldValue = stair_5N.newValue;
        //    stair_5N.newValue = stair_5N.GetNewValue();

        //    stair_5N.prevDirection = stair_5N.direction;
        //    stair_5N.oldStepValue = stair_5N.stepValue;
        //    stair_5N.stepValue = stair_5N.GetStepValue();
        //    //if (stair_5N.stepValue >= stair_5N.maxValue)
        //    //{
        //    //    stair_5N.stepValue = stair_5N.maxValue;
        //    //}

        //    Debug.Log("Answer: " + stair_5N.correctAnswer.ToString() + " New Value: " + stair_5N.stepValue.ToString() + " Old Value: " + stair_5N.oldStepValue.ToString() + " Step Size: " + stair_5N.stepSize.ToString());

        //}

    }


    //public void CheckAnswer(string UserAnswer)
    //{
    //    if (stim[StimSequence[trialNum]] == UserAnswer)
    //    {
    //        stair.correctAnswer = true;
    //        UDP_Messenger.msg = "*** User is correct: " + stair.newValue.ToString();
    //        UDP_Messenger.sendMessage = true;

    //    }
    //    else
    //    {
    //        stair.correctAnswer = false;
    //        UDP_Messenger.msg = "*** User is wrong: " + stair.newValue.ToString();
    //        UDP_Messenger.sendMessage = true;
    //    }

    //    stair.oldValue = stair.newValue;
    //    stair.newValue = stair.GetNewValue();

    //    Debug.Log("Answer: " + stair.correctAnswer.ToString() + "\t New Value: " + stair.newValue.ToString() + "\t Old Value: " + stair.oldValue.ToString());
    //}

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

    public int[] CreateStimSequeces(int[] stimseqs, int repeteVal, int blockSize)
    {
        int countr = 0;
        for (int i = 0; i < stimseqs.Length; i++)
        {
            stimseqs[i] = countr;
            countr++;

            if (countr > blockSize)
            {
                countr = 0;
            }
        }
        return stimseqs;
    }

    public float[] CreateStimBlocks()
    {
        int numBlocks = numTrials / BlockSize;
        Debug.Log("Number of Blocks: " + numBlocks);

        int[] trialBlocks = new int[] { 0, BlockSize, BlockSize * 2, BlockSize * 3, BlockSize * 4, BlockSize * 5 };
        Debug.Log("Trial of Blocks: " + trialBlocks[3]);

        for (int i = 0; i < numBlocks; i++)
        {
            var blockNums = Enumerable.Repeat(i, BlockSize);

            int cnrt = 0;
            foreach (int blockVal in blockNums)
            {
                if (i % 2 == 0)
                {
                    ForceSequence[cnrt + trialBlocks[i]] = Forces[0];
                }
                else
                {
                    ForceSequence[cnrt + trialBlocks[i]] = Forces[1];
                }
                //Debug.Log(blockStims[cnrt + trialBlocks[i]]); 

                cnrt++;
            }
        }

        return ForceSequence;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    PresentStimuli(true);
    //}
    //private void OnTriggerExit(Collider other)
    //{
    //    PresentStimuli(false);
    //}

    public void PresentStimuli()
    {
        //Debug.Log(ForceSequence[trialNum]);
        //Debug.Log(Forces[ForceSequence[trialNum]]);

        string seq1 = stair_1N.stepValue.ToString() + ", 0.0, 0.0";
        string seq2 = "0.0, " + stair_1N.stepValue.ToString() + ", 0.0";
        string seq3 = "0.0, 0.0, " + stair_1N.stepValue.ToString();
        stimSeqOrder = new string[3] { seq1, seq2, seq3 };

        UDP_Messenger.msg = trialNum.ToString() + ", " + ptxID.ToString() + ", " + stimSeqOrder[StimSequence[trialNum]].ToString() + ", " + TargetForce.ToString() + " \n";
        UDP_Messenger.sendMessage = true;

        //if (activate)
        //{
        //    foreach (GameObject obj in stimuli)
        //    {
        //        if (!obj.activeInHierarchy)
        //        {
        //            obj.SetActive(true);
        //        }
        //    }
        //}
        //if (!activate)
        //{
        //    foreach (GameObject obj in stimuli)
        //    {
        //        if (obj.activeInHierarchy)
        //        {
        //            obj.SetActive(false);
        //        }
        //    }
        //}
    }

    public static int[] Shuffle(int[] a)
    {
        // Loops through array
        for (int i = a.Length - 1; i > 0; i--)
        {
            // Randomize a number between 0 and i (so that the range decreases each time)
            int rnd = UnityEngine.Random.Range(0, i);

            // Save the value of the current i, otherwise it'll overright when we swap the values
            int temp = a[i];

            // Swap the new and old values
            a[i] = a[rnd];
            a[rnd] = temp;
        }

        return a;
    }


}
