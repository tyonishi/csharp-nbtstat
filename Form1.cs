using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtIP.Text = "192.168.1.101";
        }

        private void btnNbtstat_Click(object sender, EventArgs e)
        {
            txtInfo.Text = "";　　//文本信息显示框
            string ipStr = txtIP.Text;
            if (string.IsNullOrWhiteSpace(ipStr))
            {
                MessageBox.Show("IP不能为空！");
            }
            if (!IsIPAddress(ipStr))
            {
                MessageBox.Show("IP格式不正确！");
                return;
            }

            string str = "", strHost = "", Group = "", User = "", strMac = "";
            int receive, macline = 0, usernum = 0;
            string[] domainuser = new string[2];
            domainuser[0] = "";
            domainuser[1] = "";

            try
            {
                var senderTest = new IPEndPoint(IPAddress.Any, 0);
                var Remote = (EndPoint)senderTest;

                var ipep = new IPEndPoint(IPAddress.Parse(ipStr), 137);

                var server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000);

                byte[] bs = new byte[50] { 0x0, 0x00, 0x0, 0x10, 0x0, 0x1, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x20, 0x43, 0x4b, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x0, 0x0, 0x21, 0x0, 0x1 };
                server.SendTo(bs, bs.Length, SocketFlags.None, ipep);

                byte[] Buf = new byte[500];
                receive = server.ReceiveFrom(Buf, ref Remote);
                server.Close();

                if (receive > 0)
                {

                    byte[,] recv = new byte[18, 28];
                    recv = new byte[18, (receive - 56) % 18];

                    for (int k = 0; k < (receive - 56) % 18; k++)
                    {
                        for (int j = 0; j < 18; j++)
                        {
                            recv[j, k] = Buf[57 + 18 * k + j];
                        }
                    }

                    for (int k = 0; k < (receive - 56) % 18; k++)
                    {
                        str = "";
                        if (Convert.ToString(recv[15, k], 16) == "0" && (Convert.ToString(recv[16, k], 16) == "4" || Convert.ToString(recv[16, k], 16) == "44"))
                        {
                            for (int j = 0; j < 15; j++)
                            {
                                str += Convert.ToChar(recv[j, k]).ToString();
                            }
                            strHost = str.Trim();
                        }

                        if (Convert.ToString(recv[15, k], 16) == "0" && (Convert.ToString(recv[16, k], 16) == "84" || Convert.ToString(recv[16, k], 16).ToUpper() == "C4"))
                        {
                            for (int j = 0; j < 15; j++)
                            {
                                str += Convert.ToChar(recv[j, k]).ToString();
                            }
                            Group = str.Trim();
                        }

                        if (Convert.ToString(recv[15, k], 16) == "3" && (Convert.ToString(recv[16, k], 16) == "4" || Convert.ToString(recv[16, k], 16) == "44"))
                        {
                            for (int j = 0; j < 15; j++)
                            {
                                str += Convert.ToChar(recv[j, k]).ToString();
                            }
                            domainuser[usernum] = str.Trim();
                            usernum++;
                        }

                        if (Convert.ToString(recv[15, k], 16) == "0" && Convert.ToString(recv[16, k], 16) == "0" && Convert.ToString(recv[17, k], 16) == "0")
                        {
                            macline = k;

                            for (int i = 0; i < 6; i++)
                            {
                                if (i < 5)
                                {
                                    strMac += Convert.ToString(recv[i, macline], 16).PadLeft(2, '0').ToUpper() + ":";
                                }
                                if (i == 5)
                                {
                                    strMac += Convert.ToString(recv[i, macline], 16).PadLeft(2, '0').ToUpper();
                                }
                            }
                            k = (receive - 56) % 18;
                        }
                    }
                    User = domainuser[1];
                    if (string.IsNullOrEmpty(domainuser[1])) { User = domainuser[0]; }

                    txtInfo.Text = "主机：" + strHost + Environment.NewLine + "用户组：" + Group + Environment.NewLine + "用户：" + User + Environment.NewLine + "MAC地址：" + strMac;

                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static bool IsIPAddress(string ip)
        {

            if (string.IsNullOrEmpty(ip) || ip.Length < 7 || ip.Length > 15) return false;

            string regformat = @"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$";

            Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);

            return regex.IsMatch(ip);

        }

    }
}
