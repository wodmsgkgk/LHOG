using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using InTheHand.Windows.Forms;
using InTheHand.Net.Bluetooth.AttributeIds;
using System.IO;

namespace Bluetooth_ServerSide
{
    public partial class Form1 : Form
    {
        // Threads
        Thread AcceptAndListeningThread;

        // helper variable
        Boolean isConnected = false;
        
        //bluetooth stuff
        BluetoothClient btClient;  //represent the bluetooth client connection
        BluetoothListener btListener; //represent this server bluetooth device

        public Form1()
        {

            InitializeComponent();


            //when the bluetooth is supported by this computer

            if (BluetoothRadio.IsSupported)
            {

                UpdateLogText("Bluetooth Supported! by jaeeun");
                UpdateLogText("—————————–");
                
                //getting device information
                UpdateLogText("Primary Bluetooth Radio Name : " + BluetoothRadio.PrimaryRadio.Name);
                UpdateLogText("Primary Bluetooth Radio Address : " + BluetoothRadio.PrimaryRadio.LocalAddress);
                UpdateLogText("Primary Bluetooth Radio Manufacturer : " + BluetoothRadio.PrimaryRadio.Manufacturer);
                UpdateLogText("Primary Bluetooth Radio Mode : " + BluetoothRadio.PrimaryRadio.Mode);
                UpdateLogText("Primary Bluetooth Radio Software Manufacturer : " + BluetoothRadio.PrimaryRadio.SoftwareManufacturer);
                UpdateLogText("—————————–");

                //creating and starting the thread
                AcceptAndListeningThread = new Thread(AcceptAndListen);

                AcceptAndListeningThread.Start();

            }
            else
            {
                UpdateLogText("Bluetooth not Supported!");
            }
        }
        StreamReader srReceiver;
        private delegate void UpdateLogCallback(string strMessage);

        private void ReceiveMessages()
        {
            // Receive the response from the server
            srReceiver = new StreamReader(btClient.GetStream());

            // If the first character of the response is 1, connection was successful
            string ConResponse = srReceiver.ReadLine();

            // If the first character is a 1, connection was successful
            if (ConResponse[0] == '1')
            {
                // Update the form to tell it we are now connected
                this.Invoke(new UpdateLogCallback(this.UpdateLogText), new object[] { "Connected Successfully!" });
            }
            else // If the first character is not a 1 (probably a 0), the connection was unsuccessful
            {
                string Reason = "Not Connected: ";

                // Extract the reason out of the response message. The reason starts at the 3rd character
                Reason += ConResponse.Substring(2, ConResponse.Length - 2);

                // Exit the method
                return;
            }
            // While we are successfully connected, read incoming lines from the server
            while (isConnected)
            {
                // Show the messages in the log TextBox
                this.Invoke(new UpdateLogCallback(this.UpdateLogText), new object[] { srReceiver.ReadLine() });
            }
        }

        //the function of the thread
        public void AcceptAndListen()
        {
            while (true)
            {
                if (isConnected)
                {
                    //TODO: if there is a device connected
                    //listening
                    try
                    {
                        UpdateLogTextFromThread("Listening…1.");
                        NetworkStream stream = btClient.GetStream();

                        Byte[] bytes = new Byte[512];

                        String retrievedMsg = "";

                        stream.Read(bytes, 0, 512);

                        stream.Flush();

                        for (int i = 0; i < bytes.Length; i++)
                        {
                            retrievedMsg += Convert.ToChar(bytes[i]);

                        }

                        UpdateLogTextFromThread(btClient.RemoteMachineName + " : " + retrievedMsg);
                        UpdateLogTextFromThread("");

                        if (!retrievedMsg.Contains("servercheck"))
                        {

                            sendMessage("Message Received!");
                        }
                        //ReceiveMessages();
                    }
                    catch (Exception ex)
                    {
                        UpdateLogTextFromThread("There is an error while listening connection");
                        UpdateLogTextFromThread(ex.Message);
                        isConnected = btClient.Connected;
                    }
                }
                else
                {
                    //TODO: if there is no connection
                    // accepting
                    try
                    {
                        btListener = new BluetoothListener(BluetoothService.RFCommProtocol);
                        
                        UpdateLogTextFromThread("Listener created with TCP Protocol service " + BluetoothService.RFCommProtocol);
                        UpdateLogTextFromThread("Starting Listener….");
                        btListener.Start();
                        UpdateLogTextFromThread("Listener Started!");
                        UpdateLogTextFromThread("Accepting incoming connection….");
                        btClient = btListener.AcceptBluetoothClient();
                        isConnected = btClient.Connected;
                        UpdateLogTextFromThread("A Bluetooth Device Connected!");
                    }
                    catch (Exception e)
                    {
                        UpdateLogTextFromThread("There is an error while accepting connection");
                        UpdateLogTextFromThread(e.Message);
                        UpdateLogTextFromThread("Retrying….");
                    }
                }
            }
        }
        //this section is to create a method that allow thread accessing form’s component
        //we can’t update the text of the textbox directly from thread, so, we use this delegate function
        
        delegate void UpdateLogTextFromThreadDelegate(String msg);
        public void UpdateLogTextFromThread(String msg)
        {
            if (!this.IsDisposed && logsText.InvokeRequired) 
            {
                logsText.Invoke(new UpdateLogTextFromThreadDelegate(UpdateLogText), new Object[]{msg});
            }
        }
        //just ordinary function to update the log text.
        //after updating, we move the cursor to the end of text and scroll it to the cursor.
        public void UpdateLogText(String msg)
        {
            logsText.Text += msg + Environment.NewLine;
            logsText.SelectionStart = logsText.Text.Length;
            logsText.ScrollToCaret();
        }
        //function to send message to the client
        public Boolean sendMessage(String msg) 
        {
            try
            {
                if (!msg.Equals(""))
                {
                    UTF8Encoding encoder = new UTF8Encoding();
                    NetworkStream stream = btClient.GetStream();
                    stream.Write(encoder.GetBytes(msg + "\n"), 0, encoder.GetBytes(msg).Length);
                    stream.Flush();
                    
                }
            }
            catch (Exception ex)
            {
                UpdateLogTextFromThread("There is an error while sending message");
                UpdateLogTextFromThread(ex.Message);
                try
                {
                    isConnected = btClient.Connected;
                    btClient.GetStream().Close();
                    btClient.Dispose();
                    btListener.Server.Dispose();
                    btListener.Stop();
                }
                catch (Exception)
                {
                }
            
                return false;
            }
            
            return true;
        }
        //when closing or exiting application, we have to close connection and aborting the thread.
        //Otherwise, the process of the thread still running in the background.
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                AcceptAndListeningThread.Abort();
                btClient.GetStream().Close();
                btClient.Dispose();
                btListener.Stop();
                FormClosed += new FormClosedEventHandler(Form1_FormClosed);
            }
            catch (Exception)
            {
            }
        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }
        private void sendBtn_Click(object sender, EventArgs e)
        {
            sendMessage(messageText.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sendMessage(messageText.Text);
            sendMessage(textBox1.Text);

            messageText.Clear();
            textBox1.Clear();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}


