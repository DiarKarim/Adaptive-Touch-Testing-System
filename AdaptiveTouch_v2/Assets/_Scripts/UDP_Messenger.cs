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
 * Version: 1.0
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

//class UDP_Receiver
//{
//    // udpclient object
//    UdpClient client;
//    public int port;
//    Thread receiveThread;

//    public string lastReceivedUDPPacket = "";
//    public string allReceivedUDPPackets = "";

//    // start from shell
//    private static void Main()
//    {
//        //UDPReceive receiveObj = new UDPReceive();
//        //receiveObj.init();

//        //string text = "";
//        //do
//        //{
//        //    text = Console.ReadLine();
//        //} while (!text.Equals("exit"));
//    }

//    public UDP_Receiver()
//    {

//    }

//    public void Init_UDP()
//    {
//        init();
//    }

//    public void RecUDP_Message(string msg)
//    {
//        client = new UdpClient(port);
//        while (true)
//        {

//            try
//            {
//                // Bytes empfangen.
//                IPEndPoint anyIPR = new IPEndPoint(IPAddress.Any, 0);
//                byte[] dataR = client.Receive(ref anyIPR);
//                string textr = Encoding.UTF8.GetString(dataR);
//            }
//            catch (Exception err)
//            {
//                Debug.Log(err.ToString ());
//            }
//        }
//    }

//    private void init()
//    {
//        // Endpunkt definieren, von dem die Nachrichten gesendet werden.
//        Debug.Log("UDPSend.init()");

//        // define port
//        port = 8899;

//        // status
//        Debug.Log("Sending to 127.0.0.1 : " + port);
//        Debug.Log("Test-Sending to this Port: nc -u 127.0.0.1  " + port + "");


//        // ----------------------------
//        // Abhören
//        // ----------------------------
//        // Lokalen Endpunkt definieren (wo Nachrichten empfangen werden).
//        // Einen neuen Thread für den Empfang eingehender Nachrichten erstellen.
//        receiveThread = new Thread(new ThreadStart(RecUDP_Message));
//        receiveThread.IsBackground = true;
//        receiveThread.Start();

//    }



//}

class UDP
{

    public string DestinationIP;
    public int PortNumber;

    private string msg3;
    private int mesg;

    private byte[] send_buffer_1;
    private byte[] send_buffer_0;
    private byte[] end_buffer_2;

    private Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private IPEndPoint endPoint;

    public UDP()
    {
        this.DestinationIP = "127.0.0.1";
        this.PortNumber = 5032; 
        Init_UDP();
    }

    public void Init_UDP()
    {
        IPAddress serverAddr = IPAddress.Parse(DestinationIP);
        endPoint = new IPEndPoint(serverAddr, PortNumber);
        send_buffer_1 = Encoding.ASCII.GetBytes("1");
    }

    public void SendUDP_Message(string msg)
    {
        send_buffer_1 = Encoding.ASCII.GetBytes(msg);

        try
        {
            sock.SendTo(send_buffer_1, endPoint);
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }

        Debug.Log("*** UDP Class sends message: " + msg + " \n");
    }

}

    public class UDP_Messenger : MonoBehaviour
{

    private UDP _udp = new UDP();
    public static string msg;
    public static bool sendMessage; 

    void Start()
    {
        _udp.DestinationIP = "192.168.1.70"; 
        _udp.PortNumber = 8000;
        _udp.Init_UDP(); 
    }

    private void Update()
    {
        if (sendMessage)
        {
            SendUDPMessage(msg);
            sendMessage = false; 
        }
    }

    public void SendUDPMessage(string msg)
    {
        _udp.SendUDP_Message(msg);
    }
}
