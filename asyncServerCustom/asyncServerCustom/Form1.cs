using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;



namespace asyncServerCustom
{
    public partial class Form1 : Form
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        Thread socketsupervisor;
        bool isclose = false;
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            socketsupervisor = new Thread(SocketSupervisor);
            socketsupervisor.IsBackground = true;
            socketsupervisor.Start();
        }
        public void SocketSupervisor()
        {
            while (true)
            {
                
                try
                {
                    if (!isclose)
                    {
                        if (list.Count > 0)
                        {

                            for (int i = 0; i < list.Count; i++)
                            {
                                if (!SocketConnected(list[i]))
                                {
                                    textBox2.AppendText("[" + comboBox1.Items[i].ToString() + "] 연결이 끊겼습니다.\r\n");
                                    list.RemoveAt(i);
                                    comboBox1.Items.RemoveAt(i);
                                }
                            }
                        }
                        Thread.Sleep(200);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source);
                }
            }

        }
        List<Socket> list = new List<Socket>();
        Socket listener=null;
        public void StartListening(IPAddress ipadress, int port)
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipadress, port);

            // Create a TCP/IP socket.
            listener = new Socket(AddressFamily.InterNetwork,
              SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                textBox2.AppendText("서버가 시작되었습니다.\r\n");
                listener.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    listener);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                if (!isclose)
                {
                    // Signal the main thread to continue.
                    allDone.Set();
                    // Get the socket that handles the client request.
                    Socket Templistener = (Socket)ar.AsyncState;
                    Socket server = Templistener.EndAccept(ar);
                    list.Add(server);
                    comboBox1.Items.Add(server.RemoteEndPoint);
                    textBox2.AppendText(server.RemoteEndPoint + "의 연결 요청 수락\r\n");
                    // Create the state object.
                    StateObject state = new StateObject();
                    state.workSocket = server;
                    server.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    Templistener.BeginAccept(
                                       new AsyncCallback(AcceptCallback),
                                       Templistener);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
        public void ReadCallback(IAsyncResult ar)
        {

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            try
            {
                if (!isclose)
                {
                    // Read data from the client socket. 
                    int bytesRead = handler.EndReceive(ar);

                    if (bytesRead > 0)
                    {
                        // There  might be more data, so store the data received so far.
                        //string tempread = (Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                        string tempread = BitConverter.ToString(state.buffer, 0, bytesRead);
                        tempread = tempread.Replace("-", "");
                        var buf = new byte[bytesRead];
                        Buffer.BlockCopy(state.buffer, 0, buf, 0, bytesRead);

                        MCProtocol_t protocol = MCSerializer.Deserialize<MCProtocol_t>(buf);

                        byte[] toSend = null;
                        if (protocol.DeviceWordWrite.Command == 0x0401) {
                            // write
                            toSend = new byte[] {
                                0xD0, 0x00, 0x00, 0xFF, 0xFF, 0x03, 0x00, 0x04, 0x00, 0x00, 0x00,
                                // data
                                0x12, 0x00
                            };
                        } else if (protocol.DeviceWordWrite.Command == 0x1401) {
                            // read
                            toSend = new byte[] {
                                0xD0, 0x00, 0x00, 0xFF, 0xFF, 0x03, 0x00, 0x02, 0x00, 0x00, 0x00
                            };
                        }

                        var socket = list.FirstOrDefault();
                        if (socket != null && toSend != null) {
                            Send(socket, toSend);
                        }

                        // Check for end-of-file tag. If it is not there, read 
                        // more data.
                        //if (tempread.Contains("\r\n"))
                        {
                            // All the data has been read from the 
                            // client. Display it on the console.
                            textBox2.AppendText("[" + handler.RemoteEndPoint.ToString() + "] " + tempread);
                            // Echo the data back to the client.
                            //Send(handler, content);
                        }
                        // Not all data received. Get more.
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
            catch (ObjectDisposedException ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private void Send(Socket handler, String data)
        {
            try
            {
                if (!isclose)
                {
                    // Convert the string data to byte data using ASCII encoding.
                    byte[] byteData = Encoding.ASCII.GetBytes(data);

                    // Begin sending the data to the remote device.
                    handler.BeginSend(byteData, 0, byteData.Length, 0,
                        new AsyncCallback(SendCallback), handler);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private void Send(Socket handler, byte[] byteData)
        {
            try
            {
                if (!isclose)
                {
                    // Begin sending the data to the remote device.
                    handler.BeginSend(byteData, 0, byteData.Length, 0,
                        new AsyncCallback(SendCallback), handler);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                if (!isclose)
                {
                    // Retrieve the socket from the state object.
                    Socket handler = (Socket)ar.AsyncState;

                    // Complete sending the data to the remote device.
                    int bytesSent = handler.EndSend(ar);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (listener == null)
            {
                IPAddress ipadress = IPAddress.Parse(textBoxIP.Text);
                int port = Convert.ToInt16(textBoxport.Text);
                StartListening(ipadress, port);
                isclose = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (listener != null)
                {
                    isclose = true;
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].Shutdown(SocketShutdown.Both);
                        list[i].Close();
                    }
                    list.Clear();
                    listener.Close();
                    listener = null;
                    comboBox1.Items.Clear();
                    textBox2.AppendText("서버가 종료되었습니다.\r\n");
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
            catch (ObjectDisposedException ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                int i = comboBox1.SelectedIndex;                
                if (i >= 0 )
                {
                    Send(list[i], textBoxsend.Text + "\r\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }
        bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 & part2)
            {//connection is closed
                return false;
            }
            return true;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (list.Count != 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        Send(list[i], textBoxsend.Text + "\r\n");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

}
