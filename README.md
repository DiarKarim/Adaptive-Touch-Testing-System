# Adaptive-Touch-Testing-System

## About

Adaptive-Touch-Testing-System (ATTS) is an application for testing vibration frequency
discrimination via a staircase method, targeting the rapidly adapting sensory receptors
in human skin, e.g. on the palm side fingertips.
The application is made with the Unity game engine, and this repository allows you to
either run the test either directly in Unity or via a build executable that can run and has
been tested on Windows 10, see image below for an overview over the user-interface
(UI) of the application.

<img src="https://github.com/DiarKarim/Adaptive-Touch-Testing-System/blob/windows-version/Figures%20and%20Images/ATTS%20UI.png" height="300" width="400" >
Figure 1: User interface 

In the above image you will be able to adjust several of the stimuli parameters for
prototyping and quick testing, including playing with the frequencies, amplitudes,
stiulation duration (time) and signal forms, such as sine, square, saw and triangular
waves. Changing each of these parameters affects the signal. Once you have found and
noted down your parameters on a piece of paper you can input these for your
experiment.

In the Unity environment we also provide code-free access to the main function of the
testing system (see ExperimentManager gameobject in the hierarchy), so that other
users can adapt the test to their needs, such that critical parameters of the experiment
can be changed, including the number of frequencies, the frequencies, number of trials
etc, see image below:

<img src="https://github.com/DiarKarim/Adaptive-Touch-Testing-System/blob/windows-version/Figures%20and%20Images/ExperimentManager.png" height="300" width="400" >
Figure 2: Unity code-free ExperimentManager and inspector interface for changing experimental parameters 

## Analysis and Results

Below is an image of the results from the staircase procedure for one example
participants. The average value over the last few trials in these figures determines the
discrimination ability of that participants. These figures were produced through the
accompanying python script ("StairCase_Analysis.ipynb") inside the Python_Codes
folder in the root of this reposity, see Adaptive-Touch-TestingSystem/Python_Codes/StairCase_Analysis.ipynb.

Low 30 Hz Standard             |  High 300 Hz Standard 
:-------------------------:|:-------------------------:
<img src="https://github.com/DiarKarim/Adaptive-Touch-Testing-System/blob/windows-version/Figures%20and%20Images/Freq_30Hz_728.png" height="300" width="400" > |  <img src="https://github.com/DiarKarim/Adaptive-Touch-Testing-System/blob/windows-version/Figures%20and%20Images/Freq_300Hz_613.png" height="300" width="400" >

Figure 3: Results for low (left) and high standard frequency comparisons 

## Requirements (Software)
- [x] Python 3.8.8
- [x] Jupyter-Lab through Anaconda3
- [x] Unity 2020.1.9f1
- [x] Syntacts Unity package, see https://github.com/mahilab/Syntacts/releases

## Requirements (Hardware)
- [x] Windows 10 PC or laptop with i3 processor, 8GB of RAM and at least 20GB of free disk space
- [x] Vybronics tactile actuator (LRA), see https://www.digikey.co.uk/en/products/detail/jinlong-machinery-electronicsinc/G1040003D/10285886
- [x] Syntacts Amplifier v3.1, see https://www.syntacts.org/hardware/
- [x] Various jumber cables
- [x] Audio AUX 3.5mm male-to-male stereo audio cable, see https://www.amazon.co.uk/AmazonBasics-3-5mm-Stereo-AudioCable/dp/B00NO73MUQ

## How to get started with the basic demo

Press play in Unity then press the "S" button and follow the on-screen instructions

## Running the Experiment

Two-Interval-Forced-Choice (2-IFC) task, i.e. on each trial you are presented with two
consequentive stimuli, with a 1 second delay between them. You are then asked to
select which of the two stimuli, i.e. the first (A) or second (D) had a higher frequency.
This carries on until either one of two stopping conditions is reached, i.e. (1) either you
reach the maximum number of trial (100 by default) or you reach 6 reversals.
Watch out for the right side of the UI, which displays where the data is saved on your
computer.




