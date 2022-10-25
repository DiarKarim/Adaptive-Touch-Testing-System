#*************************************************************************
#************************** PG Touch Plate Jan 2021 by Diar Karim ********************
#*************************************************************************
# This script runs an experiment (for details see experimental variables below)
# This script will do the following:
#
#	1. Load necessary libraries and define user functions
#	2. Get user input
#	3. Set experimental paramters
#	4. Prepare recording devices (National Instruments and ATI force/torque sensors)
#	5. Run experimental loop until completion
#	6. Clean up program
#	0. Optional network communications parameters have been commented out for future use with MATLAB/others
#   
#   Based on original script:
#	Author: Diar Karim 
#	Contact: diarkarim@gmail.com
# 	Version: 1.0
#	Date: 28/01/2021
#
#   Modified by: 
#   Author: Davide Deflorio 
#   Contact: defloriodavide@gmail.com
#   Date: 10/06/2022

from datetime import datetime
import niati as ni
import aticalibration as cal
import time

from backports.time_perf_counter import perf_counter

import numpy as np
import pandas as pd
import random




# display and enter subject data
#np.disp(':::::::::::::::::::::::::::')
subjID = input('Enter subject number:  ')
subInitials = raw_input('Enter subject initials:  ')
condition = raw_input("Condition (sliding/pressing): ")

#gender = raw_input('Enter gender (f/m):  ')
#subAge = raw_input('Enter subject age:  ')
#np.disp(':::::::::::::::::::::::::::')
#aa = raw_input("PRESS ENTER TO CONTINUE")
#np.disp('---------------------------')

# Create filename and enter headers
dataFolder = "C:\Users\symonpc\Desktop\Grating\Data"
fileName = dataFolder + '\grating_staircase' + '_subj' + str(subjID) + '_' + str(subInitials)  + '.csv'


# initialise some variables
subjNum = []
trialNum = []
targtPos = []
standardFreq = []
compFreq = []
correctR = []
reversals = []

stdStim = 280
compStim = [1440, 1280, 1160, 880, 600, 500, 440, 400, 360, 320, 300]
step = 0  # stepsize
maxTrials = 100 # max number of trials
n_revers = 9 # max number of reversal

# create array with target positions for each trial
trgt_pos = [1, 2]
temp_pos = np.tile(trgt_pos,  int(maxTrials/2))
random.shuffle(temp_pos)
target = np.array(temp_pos)

changeIdx = 0 # index for step size change
correct = 0 # correct response variable
n_reversals = 0 # number of reversal to start with
nDown = 2 # N - Down

trig = 0
count = 0

# initialise some stuff for force sensor
def IdleLoop(pauseTime):
    toc = 0
    tic = perf_counter()
    print('Idling for ', pauseTime, 'seconds...')
    while toc < pauseTime:
        toc = perf_counter()-tic


deviceID = "Dev5"
sR = 1000
trialDur = 2 #input("Trial duration: ")


##### MAIN LOOP ####

for trID in range(maxTrials):

    stimulus = compStim[step]

    # if stimulus <= stdStim:
    #     stimulus = 320
    # elif stimulus > 500:
    #     stimulus = 500

    np.disp('Trial number ' + str(trID) + '..' + '\n')

    np.disp('Comparison Stimulus:' + str(stimulus) + '\n')

    np.disp('Comparison Position: ' + str(target[trID]) + '\n') 

    raw_input('\n' + 'PLACE STIMULI and PRESS ENTER FOR CALIBRATION')

    raw_data = ni.np.array([0,0,0,0,0,0])
    task1 = ni.CreateNewTask(deviceID, sR)

    tic = perf_counter()
    toc = 0
    cnt = 0
    ft_bias_probe = ni.np.zeros(6)
    ft_bias_plate1 = ni.np.zeros(6)

    print ("Calibrating bias for 1s, please do not touch the ft sensor...")
    while toc < 1.0:

        offsetForcePlate = ni.np.array(ni.init_readForce(task1,cal.S15575,toc))
        ft_bias_plate1 += offsetForcePlate

        cnt = cnt+1
        toc = perf_counter() - tic

    ft_bias_plate1 /= cnt

    print ("Calibration done.")


    dataFolderForce = "C:\Users\symonpc\Desktop\Grating\Data\ForceRecordings"

    fileName1 = dataFolderForce + '\subj' + str(subjID)  + '_' + str(subInitials) + "_" + condition  + "_Trial_" + str(trID) + "_t1" +".csv" # or csv
    fileName2 = dataFolderForce + '\subj' + str(subjID)  + '_' + str(subInitials) + "_" + condition  + "_Trial_" + str(trID) + "_t2" +".csv" # or csv

    probeForce, posData, frc_time_a,frc_time_b, pos_time, time_elapsed = [],[],[],[],[],[]
    plateForce1,plateForce2 = [],[]

    sampleRate_frc = sR # this number should be the same as sR defined above 

    frc_frac = 1.0/sampleRate_frc

    trialDuration = trialDur # seconds
    sC_frc = 0


    # record first contact  trialDuration = trialDur # seconds

    task1 = ni.CreateNewTask("Dev5", sR)

    ni.beepSound(500)

    print('\n' + 'Recording trial: ' + str(trID+1) + ' --- First contact')

    toc = 0
    toc1 = 0 
    tic = perf_counter()
    tic1 = perf_counter()
    #toc = time.perf_counter()-tic

    # Main Thread runs experiment 
    while toc < trialDuration:
        toc = perf_counter()-tic
        toc1 = perf_counter()-tic1
        try: 
            if toc1 > 0.001:
                plateForce1.append(ni.readForce(task1,cal.S15575,perf_counter(),ft_bias_plate1)) 

                sC_frc += 1
                frc_time_a.append(toc)
                tic1 = perf_counter()

        except Exception as e:
            print (e)

    ni.beepSound(700)

    task1.stop()

    IdleLoop(1)

    task1 = ni.CreateNewTask("Dev5", sR)


    ni.beepSound(500)

    print('\n' + 'Recording trial: ' + str(trID+1) + ' --- Second contact')

    sC_frc = 0

    toc = 0
    toc1 = 0 
    tic = perf_counter()
    tic1 = perf_counter()

    while toc < trialDuration:
        toc = perf_counter()-tic
        toc1 = perf_counter()-tic1
        try: 
            if toc1 > 0.001:
                plateForce2.append(ni.readForce(task1,cal.S15575,perf_counter(),ft_bias_plate1)) 

                sC_frc += 1
                frc_time_b.append(toc)
                tic1 = perf_counter()

        except Exception as e:
            print (e)

    

    print('Total force samples collected:', sC_frc, ' samples')
    print('Time elapsed', toc, ' seconds')
    print('Observed Force Frequency', sC_frc/toc, ' Hz' + '\n')

    ni.beepSound(700)

    task1.stop()

    fpDat_1 = ni.np.asarray(plateForce1)
    fpDat_2 = ni.np.asarray(plateForce2)

    timeDat1 = ni.np.asarray(frc_time_a)
    timeDat2 = ni.np.asarray(frc_time_b)

    trialNum1 = ni.np.repeat(str(trID),len(timeDat1),axis=None)
    trialNum2 = ni.np.repeat(str(trID),len(timeDat2),axis=None)

    tmpResampled1 = list(zip(fpDat_1[:,0], fpDat_1[:,1], fpDat_1[:,2], fpDat_1[:,3], fpDat_1[:,4], fpDat_1[:,5],timeDat1))
    tmpRes1 = ni.pd.DataFrame(tmpResampled1,columns=['Fx','Fy','Fz','Tx','Ty','Tz','Time'])

    tmpRes1.insert(0, "Trial", trialNum1 , True) # Add trial number to the dataframe 

    tmpResampled2 = list(zip(fpDat_2[:,0], fpDat_2[:,1], fpDat_2[:,2], fpDat_2[:,3], fpDat_2[:,4], fpDat_2[:,5],timeDat2))
    tmpRes2 = ni.pd.DataFrame(tmpResampled2,columns=['Fx','Fy','Fz','Tx','Ty','Tz','Time'])

    tmpRes2.insert(0, "Trial", trialNum2 , True) # Add trial number to the dataframe 

    outfile1 = open(fileName1, "w")
    csvText1 = tmpRes1.to_csv(index=False)
    outfile1.writelines(csvText1)
    outfile1.close()

    outfile2 = open(fileName2, "w")
    csvText2 = tmpRes2.to_csv(index=False)
    outfile2.writelines(csvText2)
    outfile2.close()

    task1.clear()

    # collect response:
    resp = input('Enter response:  ')

        # check if answer is correct
    if target[trID] == int(resp):
        correctResp = 1
        print('\n' + 'Correct! ' + '\n')
        correct = correct + 1
    else:
        correctResp = 0
        print('\n' + 'Incorrect' + '\n')

    if correctResp == 1 and correct == nDown:  # if correct answer and reach N-Down decrease freq and change step size
        trig = 1
        count = count + 2
       # if stimulus > stdStim:
        #compStim = compStim[step]
        # elif stimulus <= stdStim:
        #     compStim = 400
        step += 1
        if step <= 0:
            step = 0
        elif step >= 10:
            step = 10

        if n_reversals >= 1 and correct == nDown:
            n_reversals = n_reversals + 1

    elif correctResp == 1 and correct % 2 == 0:  # if correct and more than N-Down decrease freq but not step size
        trig = 1
        count = count + 1

      #  if stimulus > stdStim:
        #compStim = compStim[step]
        # elif stimulus <= stdStim:
        #     compStim = 400
        step += 1
        # if step <= 0:
        #     step = 0
        # elif step >= 10:
        #     step = 10

    elif correctResp == 0 and trig == 1:
        correct = 0
        n_reversals = n_reversals + 1
        trig = 0
        count = 0

        #if stimulus > stdStim:
        #compStim = stimulus[step]
        # elif stimulus <= stdStim:
        #     compStim = stimulus + step[changeIdx]
        step -= 1
        
# `       if step <= 0:
#             step = 0
#         elif step >= 10:
#             step = 10

    elif correctResp == 0 and trig == 0:
        correct = 0
        trig = 0
        count = 0

        #if stimulus > stdStim:
       # compStim = stimulus[step]
        # elif stimulus <= stdStim:
        #     compStim = stimulus + step[changeIdx]
        step -= 1

        # if step <= 0:
        #     step = 0
        # elif step >= 10:
        #     step = 10
    # if n_reversals < 2: # change step size after 2 reversals
    #     changeIdx = 0
    # else:
    #     changeIdx = 1


    print('N. Reversals: '  + str(n_reversals) + '\n')
    print('-----------------------------------------'+ '\n')

    subjNum.append(subjID)
    trialNum.append(trID)
    targtPos.append(target[trID])
    standardFreq.append(stdStim)
    compFreq.append(stimulus)
    correctR.append(correctResp)
    reversals.append(n_reversals)

    temp = pd.DataFrame(trialNum,columns=['Trial'])
    temp.insert(0,"Subj", subjNum,True)
    temp.insert(2,"StandardStim", standardFreq,True)
    temp.insert(3,"ComparisonStim", compFreq,True)
    temp.insert(4,"TargetPos", targtPos,True)
    temp.insert(5,"CorrectResponse", correctR,True)
    temp.insert(6,"n_revers", reversals,True)

    temp.to_csv(fileName, index=False)


    #mess = input('PRESS ENTER TO CONTINUE')

    if trID == maxTrials or (n_reversals>= n_revers):
        print("END OF EXPERIMENT")
        exit()



