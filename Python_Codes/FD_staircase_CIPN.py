# Author: Davide Deflorio
# Date: 24.02.2022
# Email: dxd815 @ student.bham.ac.uk

import syntacts as syn
from time import sleep
import numpy as np
import pandas as pd
import random


session = syn.Session() # start Syntacts session


# display and enter subject data
#np.disp(':::::::::::::::::::::::::::')
subjID = input('Enter subject number:  ')
subInitials = input('Enter subject initials:  ')
#gender = raw_input('Enter gender (f/m):  ')
#subAge = raw_input('Enter subject age:  ')
#np.disp(':::::::::::::::::::::::::::')
#aa = raw_input("PRESS ENTER TO CONTINUE")
#np.disp('---------------------------')

# Create filename and enter headers
dataFolder = "C:/Users/ObiPC/OneDrive/Projects/Adaptive-Touch-Testing-System/Staircase_Results/Davide_data/"
fileName = dataFolder + 'FD_staircase30Hz300Hz' + '_subj' + str(subjID) + '_' + str(subInitials)  + '.csv'

# initialise some variables
subjNum = []
trialNum = []
targtPos = []
standardFreq = []
compFreq = []
correct = []
reversals300 = []
reversals30 = []

stdFreq300 = 300
stdFreq30 = 30

compFreq300 = 400
compFreq30 = 80

step300 = [20, 10, 8, 5, 2] # stepsize
step30 = [10, 7, 5, 3, 1]

maxTrials = 120 # max number of trials

n_revers = 9 # max number of reversal

# create array with target positions for each trial
trgt_pos = [1, 2]
temp_pos = np.tile(trgt_pos,  int(maxTrials/2))
random.shuffle(temp_pos)
target = np.array(temp_pos)

changeIdx300 = 0 # index for step size
changeIdx30 = 0

correct300 = 0 # correct response variable
correct30 = 0

n_reversal300 = 0 # number of reversal
n_reversal30 = 0

nDown = 2 # N - Down

startId = int(input('Insert 1 to start with 300 Hz Staircase or 2 for 30 Hz Staircase: '))

# Experiment loop
session.open(3)

trig300 = 0
trig30 = 0

count300 = 0
count30 = 0

for trID in range(maxTrials):
    np.disp('Trial number ' + str(trID+1) + '..')
    sleep(2)

    if startId % 2 != 0: # if even trial number
        std = stdFreq300
        startId += 1

        frequency = compFreq300
        if frequency == stdFreq300:
            frequency = frequency+1

        if (target[trID] == 1 and frequency > stdFreq300) or (target[trID] == 2 and frequency < stdFreq300):

            if 420 > frequency >= 370:
                amp = 30
            elif 370 > frequency >= 350:
                amp = 15
            elif 350 > frequency > 345:
                amp = 6
            elif 345 >= frequency > 310:
                amp = 2
            elif 310 >= frequency > 300:
                amp = 1.1
            elif 300 >= frequency > 290:
                amp = 1
            elif 290 >= frequency > 280:
                amp = 0.9
            elif 280 >= frequency > 270:
                amp = 0.8     
            elif 270 >= frequency > 250:
                amp = 0.6
            elif 250 >= frequency >= 230:
                amp = 0.4
            elif 230 > frequency >= 220:
                amp = 0.25
            elif 220 > frequency >= 200:
                amp = 0.2

            sin = syn.Sine(frequency)
            bas = syn.Envelope(1, amp)
            sig = sin * syn.ASR(0.1,0.8,0.1) * bas
            session.play(0, sig) # plays sig 1 on channel 0
            sleep(sig.length)

            sleep(1.5)

            sin = syn.Sine(stdFreq300)  # 10 Hz Sine wave
            bas = syn.Envelope(1, 1)
            sig2 = sin * syn.ASR(0.1,0.8,0.1) * bas
            session.play(0, sig2)  # plays sig 1 on channel 0
            sleep(sig2.length)

        elif (target[trID] == 1 and frequency < stdFreq300) or (target[trID] == 2 and frequency > stdFreq300):

            sin = syn.Sine(stdFreq300)  # 10 Hz Sine wave
            bas = syn.Envelope(1, 1)
            sig2 = sin * syn.ASR(0.1,0.8,0.1) * bas
            session.play(0, sig2)  # plays sig 1 on channel 0
            sleep(sig2.length)

            sleep(1.5)

            if 420 > frequency >= 370:
                amp = 30
            elif 370 > frequency >= 350:
                amp = 15
            elif 350 > frequency > 345:
                amp = 6
            elif 345 >= frequency > 310:
                amp = 2
            elif 310 >= frequency > 300:
                amp = 1.1
            elif 300 >= frequency > 290:
                amp = 1
            elif 290 >= frequency > 280:
                amp = 0.9
            elif 280 >= frequency > 270:
                amp = 0.8     
            elif 270 >= frequency > 250:
                amp = 0.6
            elif 250 >= frequency >= 230:
                amp = 0.4
            elif 230 > frequency >= 220:
                amp = 0.25
            elif 220 > frequency >= 200:
                amp = 0.2

            sin = syn.Sine(frequency)  # 10 Hz Sine wave
            bas = syn.Envelope(1, amp)
            sig = sin * syn.ASR(0.1,0.8,0.1) * bas
            session.play(0, sig)  # plays sig 1 on channel 0
            sleep(sig.length)

        # collect response:
        resp = input('Enter response:  ')

        # check if answer is correct
        if target[trID] == int(resp):
            correctResp = 1
            print('Correct! ')
            correct300 = correct300 + 1
        else:
            correctResp = 0
            print('Incorrect')

        if correctResp == 1 and correct300 == nDown:  # if correct answer and reach N-Down decrease freq and change step size
            trig300 = 1
            count300 = count300 + 2
            if frequency > stdFreq300:
                compFreq300 = compFreq300 - step300[changeIdx300]
            elif frequency < stdFreq300:
                compFreq300 = compFreq300 + step300[changeIdx300]

            if n_reversal300 >= 1 and correct300 == nDown:
                n_reversal300 = n_reversal300 + 1

        elif correctResp == 1 and correct300 % 2 == 0:  # if correct and more than N-Down decrease freq but not step size
            trig300 = 1
            count300 = count300 + 1

            if frequency > stdFreq300:
                compFreq300 = compFreq300 - step300[changeIdx300]
            elif frequency < stdFreq300:
                compFreq300 = compFreq300 + step300[changeIdx300]

        elif correctResp == 0 and trig300 == 1 and frequency < 400:
            correct300 = 0
            n_reversal300 = n_reversal300 + 1
            trig300 = 0
            count300 = 0

            if frequency > stdFreq300:
                compFreq300 = compFreq300 + step300[changeIdx300]
            elif frequency < stdFreq300:
                compFreq300 = compFreq300 - step300[changeIdx300]

        elif correctResp == 0 and trig300 == 0:
            correct300 = 0
            trig300 = 0
            count300 = 0

            if frequency > stdFreq300:
                compFreq300 = compFreq300 + step300[changeIdx300]
            elif frequency < stdFreq300:
                compFreq300 = compFreq300 - step300[changeIdx300]

# Diar: Number of reversals determine step size 
        if n_reversal300 < 1:
            changeIdx300 = 0
        elif 1 <= n_reversal300 <= 3:
            changeIdx300 = 1
        elif 4 <= n_reversal300 <= 5:
            changeIdx300 = 2
        elif 5 < n_reversal300 <= 6:
            changeIdx300 = 3
        elif n_reversal300 > 7:
            changeIdx300 = 4

    else:
        std = stdFreq30
        startId += 1
        frequency = compFreq30

        if frequency == stdFreq30:
            frequency = frequency+1

        if (target[trID] == 1 and frequency > stdFreq30) or (target[trID] == 2 and frequency < stdFreq30):
            
            if 100 >= frequency >= 90:
                amp = 1.1
            elif 80 <= frequency < 90:
                amp = 1.3
            elif 70 <= frequency < 80:
                amp = 1.4
            elif 60 < frequency < 70:
                amp = 1.5
            elif 50 <= frequency <= 60:
                amp = 1.6
            elif 42 < frequency < 50:
                amp = 1.8
            elif 38 < frequency <= 42:
                amp = 2.4
            elif 35 <= frequency <= 38:
                amp = 2.5
            elif 30 <= frequency < 35:
                amp = 3.2
            elif 25 < frequency < 30:
                amp = 3.3
            elif 22 <= frequency <= 25:
                amp = 3.9
            elif 20 <= frequency < 22:
                amp = 4.7
            elif 10 <= frequency < 20:
                amp = 5.7

            sin = syn.Sine(frequency)  # 10 Hz Sine wave
            bas = syn.Envelope(1, amp)
            sig = sin * syn.ASR(0.1,0.8,0.1) * bas
            session.play(0, sig)  # plays sig 1 on channel 0
            sleep(sig.length)

            sleep(1.5)

            sin = syn.Sine(stdFreq30)  # 10 Hz Sine wave
            bas = syn.Envelope(1, 3.5)
            sig2 = sin * syn.ASR(0.1,0.8,0.1) * bas
            session.play(0, sig2)  # plays sig 1 on channel 0
            sleep(sig2.length)

        elif (target[trID] == 1 and frequency < stdFreq30) or (target[trID] == 2 and frequency > stdFreq30):

            sin = syn.Sine(stdFreq30)  # 10 Hz Sine wave
            bas = syn.Envelope(1, 3.5)
            sig2 = sin * syn.ASR(0.1,0.8,0.1) * bas
            session.play(0, sig2)  # plays sig 1 on channel 0
            sleep(sig2.length)

            sleep(1.5)

            if 100 >= frequency >= 90:
                amp = 1.1
            elif 80 <= frequency < 90:
                amp = 1.3
            elif 70 <= frequency < 80:
                amp = 1.4
            elif 60 < frequency < 70:
                amp = 1.5
            elif 50 <= frequency <= 60:
                amp = 1.6
            elif 42 < frequency < 50:
                amp = 1.8
            elif 38 < frequency <= 42:
                amp = 2.4
            elif 35 <= frequency <= 38:
                amp = 2.5
            elif 30 <= frequency < 35:
                amp = 3.2
            elif 25 < frequency < 30:
                amp = 3.3
            elif 22 <= frequency <= 25:
                amp = 3.9
            elif 20 <= frequency < 22:
                amp = 4.7
            elif 10 <= frequency < 20:
                amp = 5.7

            sin = syn.Sine(frequency)  # 10 Hz Sine wave
            bas = syn.Envelope(1, amp)
            sig = sin * syn.ASR(0.1,0.8,0.1) * bas
            session.play(0, sig)  # plays sig 1 on channel 0
            sleep(sig.length)

        # collect response:
        resp = input('Enter response:  ')

        # check if answer is correct
        if target[trID] == int(resp):
            correctResp = 1
            print('Correct! ')
            correct30 = correct30 + 1
        else:
            correctResp = 0
            print('Incorrect')

        if correctResp == 1 and correct30 == nDown:  # if correct answer and reach N-Down decrease freq and change step size
            trig30 = 1
            count30 = count30 + 2
            if frequency > stdFreq30:
                compFreq30 = compFreq30 - step30[changeIdx30]
            elif frequency < stdFreq30:
                compFreq30 = compFreq30 + step30[changeIdx30]

            if n_reversal30 >= 1 and correct30 == 2:
                n_reversal30 = n_reversal30 + 1

        elif correctResp == 1 and correct30 % 2 == 0:  # if correct and more than N-Down decrease freq but not step size
            trig30 = 1
            count30 = count30 + 1

            if frequency > stdFreq30:
                compFreq30 = compFreq30 - step30[changeIdx30]
            elif frequency < stdFreq30:
                compFreq30 = compFreq30 + step30[changeIdx30]

        elif correctResp == 0 and trig30 == 1 and frequency < 100:
            correct30 = 0
            n_reversal30 = n_reversal30 + 1
            trig30 = 0
            count30 = 0

            if frequency > stdFreq30:
                compFreq30 = compFreq30 + step30[changeIdx30]
            elif frequency < stdFreq30:
                compFreq30 = compFreq30 - step30[changeIdx30]

        elif correctResp == 0 and trig30 == 0:
            correct30 = 0
            trig30 = 0
            count30 = 0

            if frequency > stdFreq30:
                compFreq30 = compFreq30 + step30[changeIdx30]
            elif frequency < stdFreq30:
                compFreq30 = compFreq30 - step30[changeIdx30]

        if n_reversal30 < 1:
            changeIdx30 = 0
        elif 1 <= n_reversal30 <= 3:
            changeIdx30 = 1
        elif 4 <= n_reversal30 <= 5:
            changeIdx30 = 2
        elif 5 < n_reversal30 <= 6:
            changeIdx30 = 3
        elif n_reversal30 > 7:
            changeIdx30 = 4

    print(frequency)
    print(n_reversal300)
    print(n_reversal30)
    print(count300)
    print(count30)

    subjNum.append(subjID)
    trialNum.append(trID)
    targtPos.append(target[trID])
    standardFreq.append(std)
    compFreq.append(frequency)
    correct.append(correctResp)
    reversals300.append(n_reversal300)
    reversals30.append(n_reversal30)

    temp = pd.DataFrame(trialNum,columns=['Trial'])
    temp.insert(0,"Subj", subjNum,True)
    temp.insert(2,"StandardFreq", standardFreq,True)
    temp.insert(3,"ComparisonFreq", compFreq,True)
    temp.insert(4,"TargetPos", targtPos,True)
    temp.insert(5,"CorrectResponse", correct,True)
    temp.insert(6,"n_revers_300Hz", reversals300,True)
    temp.insert(7,"n_revers_30Hz", reversals30,True)

    temp.to_csv(fileName, index=False)


    mess = input('PRESS ENTER TO CONTINUE')

    if trID == maxTrials or (n_reversal300 >= n_revers and n_reversal30 >= n_revers):
        print("END OF EXPERIMENT")
        exit()


session.close()



#for tr = 1:120

#plot(tr, ff(1, tr), 'ko');
#hold on
#ylim([compFreqDown - 20 compFreqUp + 20])
#end
