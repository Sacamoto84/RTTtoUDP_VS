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

namespace MinimalisticTelnet
{
    class Program
    {
        static void Main(string[] args)
        {
            UTF8Encoding utf8 = new UTF8Encoding();

            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("192.168.0.255"), 8888);
            byte[] data = new byte[2048*16];
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            TelnetConnection tc = new TelnetConnection("127.0.0.1", 19021);
            string s;// = tc.Login("","",1000);

            // while connected
            while (tc.IsConnected)
            {
                String decodedString = utf8.GetString(tc.Read());
                Console.Write(decodedString);
                if (decodedString != "")
                {
                    data = Encoding.UTF8.GetBytes(decodedString);
                    server.SendTo(data, data.Length, SocketFlags.None, ip);
                }  
            }
            Console.WriteLine("***DISCONNECTED");
            server.Close();
        }
    }
}
