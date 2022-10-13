# from serial import Serial
import serial
import time
# import numpy as np 
import pandas as pd

path = "C:/Users/ObiPC/OneDrive/Projects/Adaptive-Touch-Testing-System/Python_Codes/"
arduino = serial.Serial(port='COM13', baudrate=115200, timeout=.1)

def write_read():
    # arduino.write(bytes(x, 'utf-8'))
    # time.sleep(0.05)
    data = arduino.readline()
    return data

xVal = []
yVal = []
zVal = []
timings = []

trialDuration = 120
startTime = time.time()
currTime = 0

while currTime<trialDuration:

    currTime = time.time() - startTime

    try:
    
    #     num = input("Enter a number: ") # Taking input from user
        value = write_read()
        val = value.decode("unicode_escape")
        v = val.split('\t')

        xVal.append(float(v[0]))
        yVal.append(float(v[1]))
        zVal.append(float(v[2]))    
        timings.append(currTime)

        print(trialDuration - currTime)

    except KeyboardInterrupt:
        break

    except: 
        print('Error')


dictVals = {'XVal': xVal,
    'YVal': yVal,
    'ZVal': zVal,
    'time' : timings,}

df_vals = pd.DataFrame(dictVals)
df_vals.to_csv(path + str(time.time()) + "_imu.csv")    

