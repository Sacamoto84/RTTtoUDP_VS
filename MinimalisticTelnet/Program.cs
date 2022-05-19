// minimalistic telnet implementation
// conceived by Tom Janssens on 2007/06/06  for codeproject
//
// http://www.corebvba.be

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;

namespace MinimalisticTelnet
{
    class Program
    {




        private static bool ConsoleVisible = false;
        private const int HIDE = 0;
        private const int SHOW = 9;
        private const int MF_BYCOMMAND = 0;
        private const int SC_CLOSE = 0xF060;


        private static IntPtr ConsoleWindow = GetConsoleWindow();
  

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

        

        private static void Adder()
        {

            UTF8Encoding utf8 = new UTF8Encoding();

            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("192.168.0.255"), 8888);
            byte[] data = new byte[2048 * 32];
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

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
                            data = Encoding.UTF8.GetBytes("\x1B[38;5;125m*** Error Telnet Server Отключет ***");
                            server.SendTo(data, data.Length, SocketFlags.None, ip);
                            break;
                        }

                        byte[] buf = tc.Read();

                        //Console.Write("\x1B[38;5;125mfffff");

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

                //data = Encoding.UTF8.GetBytes("***DISCONNECTED");
                //server.SendTo(data, data.Length, SocketFlags.None, ip);
                //Console.WriteLine("***DISCONNECTED");
            }

            server.Close();




        }

        static void Main(string[] args)
        {
            Console.WriteLine("RTT UDP v1.0");

            ShowWindow(GetConsoleWindow(), 0);

            NotifyIcon icon = new NotifyIcon();
            icon.Icon = new System.Drawing.Icon("altyo.ico");
            icon.Visible = true;
            icon.Text = "Свернутое консольное приложение";
            icon.Click += Ni_Click;

            Thread t = new Thread(Adder);
            t.Name = "Thread1";
            t.Start();


            Application.Run();



            //

           




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


    }
}
