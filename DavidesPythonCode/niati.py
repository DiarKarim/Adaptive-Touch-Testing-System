#**************************************************************************
#************************** PG Touch Plate Jan 2021 by Diar Karim *********
#**************************************************************************
# Diar's custom python module library for ATI and National Instruments code
#	Author: Diar Karim
#	Contact: diarkarim@gmail.com
# 	Version: 1.0
#	Date: 28/01/2021
#**************************************************************************
#**************************************************************************
#**************************************************************************

#*************************************************************************
#************************** Define user functions ************************
#*************************************************************************
import time
import daqmx
import matplotlib.pyplot as plt
import socket
import numpy as np
import sys
import winsound
import pandas as pd

def init_readForce(tsk1,S2,times,bias=np.zeros(6)):
	data  = tsk1.read()
	data_cal = np.array(CalibrateForce(data[0][0],S2)) - bias

	return data_cal

def readForce(tsk1,S2,times,bias=np.zeros(6)): #sock,addr
	data  = tsk1.read()
	data_cal = np.array(CalibrateForce(data[0][0],S2)) - bias

	data_out = None
	for d in data_cal[:3]:

		if data_out is None:
			data_out = str(d)
		else:
			data_out += ", " + str(d)
	# Send via UDP
	# print data_out + ', ' + str(times)

	data_out = data_out + ', %lf\n'%(times,)
	# sock.sendto(data_out, addr)

	return data_cal

def CalibrateForce(rawForce,S2):
	fx = np.divide(np.dot(rawForce,np.transpose(S2[0,[0,1,2,3,4,5]])),S2[0,6])
	fy = np.divide(np.dot(rawForce,np.transpose(S2[1,[0,1,2,3,4,5]])),S2[1,6])
	fz = np.divide(np.dot(rawForce,np.transpose(S2[2,[0,1,2,3,4,5]])),S2[2,6])
	tx = np.divide(np.dot(rawForce,np.transpose(S2[3,[0,1,2,3,4,5]])),S2[3,6])
	ty = np.divide(np.dot(rawForce,np.transpose(S2[4,[0,1,2,3,4,5]])),S2[4,6])
	tz = np.divide(np.dot(rawForce,np.transpose(S2[5,[0,1,2,3,4,5]])),S2[5,6])

	forces_cal = [fx, fy, fz, tx, ty, tz]
	return forces_cal

def beepSound(freq):
	frequency = freq  # Set Frequency in Hertz
	duration = 100  # Set Duration To 1000 ms == 1 second
	winsound.Beep(frequency, duration)

def ReadPosition(sock):
	pos_data, pos_addr_rec = sock.recvfrom(1024)
	# print pos_data
	return np.array((pos_data.split(', '))).astype(float)

def ReadExperiment(sock):
	expData, exp_addr_rec = sock.recvfrom(512)
	# print expData 
	# return np.array((expData.split(', '))).astype(int)
	return expData

def SaveData(txWriter1,data):
		#txWriter1.write(str(data.save) + "\n") # format to a better way
		# print type(data)
		# print data.shape
		np.savetxt(txWriter1, data, delimiter='\t')

def SaveText(txWriter1,data):
		#txWriter1.write(str(data.save) + "\n") # format to a better way
		# print type(data)
		# print data.shape
		# np.savetxt(txWriter1, data)
		txWriter1.write(str(data.save) + "\n") # format to a better way

# Create a channel object
def CreateNewTask(dev, sampleRate):
	try: 
		task = daqmx.tasks.Task()
		time.sleep(1)

		MAX_CHANNELS = 6
		for channel_idx in range(MAX_CHANNELS):
			channel = daqmx.channels.AnalogInputVoltageChannel()
			channel.physical_channel = dev + "/ai%d"%(channel_idx,) # Probe ATI F/T sensor
			channel.name = "analog input %d"%(channel_idx,)
			task.add_channel(channel)

		task.configure_sample_clock(sample_rate=sampleRate,samples_per_channel=1)

	except Exception as e:
		print (e)
		task.stop()
		task.clear()
		pass

	return task



def SaveJson(ptxID,trialNum,path,fileName,fx,fy,fz,tx,ty,tz,t):

	dataAll = None 
	tmpResampled = list(zip(fx,fy,fz,tx,ty,tz,t))
	tmpRes = pd.DataFrame(tmpResampled,columns=['Fx','Fy','Fz','Tx','Ty','Tz','Time'])

	tmpRes.insert(0, "Participant_ID", ptxID , True) # Add participant id to dataframe
	tmpRes.insert(0, "Trial", trialNum , True) # Add trial number to the dataframe 

	if dataAll is None:
		dataAll = tmpRes
	else:
		dataAll = pd.concat((dataAll, tmpRes))      

	# Save to file 
	outfile = open(path + fileName, "w")
	jsonText = dataAll.to_json(orient="columns")
	outfile.writelines(jsonText)
	outfile.close()