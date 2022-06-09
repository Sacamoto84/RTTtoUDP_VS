// minimalistic telnet implementation
// conceived by Tom Janssens on 2007/06/06  for codeproject
//
// http://www.corebvba.be

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace MinimalisticTelnet
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("RTT UDP V3.2");
            ShowWindow(GetConsoleWindow(), 0);

            NotifyIcon icon = new NotifyIcon();
            icon.Icon = MinimalisticTelnet.Properties.Resources.altyo;//new System.Drawing.Icon("Resources/altyo.ico");
            icon.Visible = true;
            icon.Text = "Свернутое консольное приложение";
            icon.Click += Ni_Click;

            Thread t = new Thread(Adder);
            t.Name = "Thread1";
            t.Start();

            Thread tRecive = new Thread(ReciveUDP);
            tRecive.Name = "UDP Reciver";
            tRecive.Start();

            Application.Run();
        }

        private static bool ConsoleVisible = false;
        private const int HIDE = 0;
        private const int SHOW = 9;
        private const int MF_BYCOMMAND = 0;
        private const int SC_CLOSE = 0xF060;

        private static IntPtr ConsoleWindow = GetConsoleWindow();
        static Socket server;// = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [System.Runtime.InteropServices.DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        static void ToggleWindow(bool visible)
        {
            ConsoleVisible = visible;
            ShowWindow(ConsoleWindow, visible ? SHOW : HIDE);
        }

        static void Adder()
        {

            UTF8Encoding utf8 = new UTF8Encoding();
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("192.168.0.255"), 8888);
            byte[] data = new byte[2048 * 32];
            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            TelnetConnection tc = null;
            int count = 0;

            while (true)
            {
                try
                {
                    data = Encoding.UTF8.GetBytes("\u001b[01;38;05;107m*** Подключение к Telnet ***\u001b[0m");
                    server.SendTo(data, data.Length, SocketFlags.None, ip);
                    Console.WriteLine("\u001b[01;38;05;107m***Подключение к Telnet ***\u001b[0m");
                    tc = new TelnetConnection("localhost", 19021);
                }
                catch (Exception ex)
                {
                    if (tc != null)
                    {
                        tc.tcpSocket.Close();
                    }
                    Console.WriteLine("\u001b[38;05;232;48;05;196m***Ошибка подключения к Telnet***\u001b[0m");
                    data = Encoding.UTF8.GetBytes("\u001b[38;05;232;48;05;196m***Ошибка подключения к Telnet***\u001b[0m");
                    server.SendTo(data, data.Length, SocketFlags.None, ip);
                }

                if (tc != null)
                {
                    while (tc.IsConnected)
                    {
                        //Проверяем подключение
                        if (tc.checkConnect() == false)
                        {
                            data = Encoding.UTF8.GetBytes("\u001b[38;05;9m*** Error Telnet Server Отключен ***");
                            server.SendTo(data, data.Length, SocketFlags.None, ip);
                            break;
                        }
                        byte[] buf = tc.Read();
                        if (buf != null)
                        {
                            if (buf.Length > 0)
                            {
                                count++;
                                String decodedString = utf8.GetString(buf);
                                Console.Write($"{count} : {decodedString}");
                                server.SendTo(buf, buf.Length, SocketFlags.None, ip);
                            }
                        }
                    }
                }
            }
            server.Close();
        }

        static int Show = 0;
        private static void Ni_Click(object sender, EventArgs e)
        {
            if (Show == 0)
            {
                Show = 1;
                ShowWindow(GetConsoleWindow(), 1);
            }
            else
            {
                Show = 0;
                ShowWindow(GetConsoleWindow(), 0);
            }
        }

        private static void ReciveUDP()
        {
            UdpClient client = new UdpClient();
            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 8889);
            client.Client.Bind(localEp);
            while (true)
            {
                try
                {
                    byte[] data = client.Receive(ref localEp);
                    string text = Encoding.UTF8.GetString(data);
             
                    Console.WriteLine("Прием коанды:"+text);

                    if(text == "Reset")
                    {
                        Process.Start("c:\\Jlink\\reset.bat");
                    }
                    if (text == "Activate")
                    {
                        Kill_Jlink();
                        Thread.Sleep(1000);
                        Process pro = new Process();
                        pro.StartInfo.FileName = @"c:\\Jlink\\activate.bat";
                        pro.StartInfo.Arguments = "";
                        pro.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                        pro.StartInfo.CreateNoWindow = true;
                        pro.Start();
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.ToString());
                }
            }
        }

        static void Kill_Jlink()
        {
            System.Diagnostics.Process[] etc = System.Diagnostics.Process.GetProcesses();//получим процессы
            foreach (System.Diagnostics.Process anti in etc)//обойдем каждый процесс
            {
                    if (anti.ProcessName.ToLower().Contains("Jlink".ToLower())) //найдем нужный и убьем
                    {
                        anti.Kill();
                    }
            }

        }



    }
}
