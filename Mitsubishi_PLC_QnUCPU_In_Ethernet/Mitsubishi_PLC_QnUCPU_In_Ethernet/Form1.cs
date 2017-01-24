using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Mitsubishi_PLC_QnUCPU_In_Ethernet
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public Socket client;
        int SendLen = 0;
        byte[] send_buff = null;
        byte[] receive_buff = null;

        private void btn_connect_Click(object sender, EventArgs e)
        {
            try
            {
                if (client == null)
                {
                    IPAddress ipAddr = IPAddress.Parse(tb_ipaddr.Text);
                    int port = Convert.ToInt16(tb_port.Text);

                    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    IPEndPoint EndPoint = new IPEndPoint(ipAddr, port);

                    client.Connect(EndPoint);

                    btn_connect.Enabled = false;
                    btn_disconnect.Enabled = true;

                    MessageBox.Show("접속되었습니다.");

                    timer1.Enabled = true;
                    timer1.Start();
                }

            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
            catch (ObjectDisposedException ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ulong DeviceNo;         //선두디바이스
            byte DeviceCode = 0xA8;     //디바이스코드
            ushort DeviceNum = 1;       //디바이스점수
            string read_result = null;  //결과값

            //Recoiler Tension
            DeviceNo = 3034;
            read_result = PLC_Read(DeviceNo, DeviceCode, DeviceNum);
            tb_tension.Text = read_result;

            ////Counter
            //DeviceNo = 3128;
            //read_result = PLC_Read(DeviceNo, DeviceCode, DeviceNum);
            //tb_counter.Text = read_result;

            ////Line Speed
            //DeviceNo = 3110;
            //read_result = PLC_Read(DeviceNo, DeviceCode, DeviceNum);
            //tb_speed.Text = read_result;
        }

        private string PLC_Read(ulong DeviceNo, byte DeviceCode, ushort DeviceNum)
        {
            MCProtocol_t send_protocol = new MCProtocol_t();
            string read_result = null;
            receive_buff = null;
            
            //Protocol에 따른 Send 데이터 생성
            send_protocol.SubHeader = 0x0050;
            send_protocol.NetworkNo = 0x00;
            send_protocol.PcNo = 0xFF;
            send_protocol.IoNo = 0x03FF;
            send_protocol.UnitNo = 0x00;
            send_protocol.MonitoringTimer = 0x0010;
            send_protocol.Command = 0x0401;
            send_protocol.SubCommand = 0x0000;
            send_protocol.DeviceNoCode = DeviceNo;
            send_protocol.DeviceNoCode |= (ulong)DeviceCode << 24;
            send_protocol.DevieceNum = DeviceNum;

            send_protocol.DataLength = (ushort)Marshal.SizeOf(send_protocol.MonitoringTimer);
            send_protocol.DataLength += (ushort)Marshal.SizeOf(send_protocol.Command);
            send_protocol.DataLength += (ushort)Marshal.SizeOf(send_protocol.SubCommand);
            send_protocol.DataLength += (ushort)Marshal.SizeOf(send_protocol.DeviceNoCode);
            //send_protocol.DataLength += (ushort)Marshal.SizeOf(send_protocol.DeviceCode);
            send_protocol.DataLength += (ushort)Marshal.SizeOf(send_protocol.DevieceNum);

            SendLen = (int)Marshal.SizeOf(send_protocol.SubHeader);
            SendLen += (int)Marshal.SizeOf(send_protocol.NetworkNo);
            SendLen += (int)Marshal.SizeOf(send_protocol.PcNo);
            SendLen += (int)Marshal.SizeOf(send_protocol.IoNo);
            SendLen += (int)Marshal.SizeOf(send_protocol.UnitNo);
            SendLen += (int)Marshal.SizeOf(send_protocol.DataLength);
            SendLen += (int)send_protocol.DataLength;

            //C#엔 포인터가 없기때문에 아래처럼 해야됨
            send_buff = new byte[Marshal.SizeOf(send_protocol)];
            unsafe
            {
                fixed (byte* fixed_buffer = send_buff)
                {
                    Marshal.StructureToPtr(send_protocol, (IntPtr)fixed_buffer, false);
                }
            }

            //프로토콜 Send
            try
            {
                client.Send(send_buff, SendLen, SocketFlags.None);
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
            catch (ObjectDisposedException ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }

            return read_result;

            //Send에대 대한 응답처리
        }


        private void btn_disconnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (client != null)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    client = null;

                    MessageBox.Show("접속이 해제되었습니다.");

                    btn_connect.Enabled = true;
                    btn_disconnect.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        private void lb_title_Click(object sender, EventArgs e)
        {

        }
    }
}
