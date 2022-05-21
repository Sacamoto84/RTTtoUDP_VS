// minimalistic telnet implementation
// conceived by Tom Janssens on 2007/06/06  for codeproject
//
// http://www.corebvba.be



using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MinimalisticTelnet
{
    enum Verbs
    {
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,
        IAC = 255
    }

    enum Options
    {
        SGA = 3
    }

    class TelnetConnection
    {
        public TcpClient tcpSocket;

        int TimeOutMs = 10;

        public TelnetConnection(string Hostname, int Port)
        {
            tcpSocket = new TcpClient(Hostname, Port);

        }

        public void WriteLine(string cmd)
        {
            Write(cmd + "\n");
        }

        public void Write(string cmd)
        {
            if (!tcpSocket.Connected) return;
            byte[] buf = System.Text.ASCIIEncoding.ASCII.GetBytes(cmd.Replace("\0xFF", "\0xFF\0xFF"));
            tcpSocket.GetStream().Write(buf, 0, buf.Length);
        }

        //Проверка соединения
        public bool checkConnect()
        {

            //System.Net.Sockets.SocketException: "Удаленный хост принудительно разорвал существующее подключение"
            try
            {
                if (tcpSocket.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (tcpSocket.Client.Receive(buff, SocketFlags.Peek) == 0)
                    {
                        // Client disconnected
                        Console.WriteLine("Jlink Telnet отключен");
                        tcpSocket.Close();
                        return false;
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
                return false;
            }

            return true;
        }


        public Byte[] Read()
        {
            if (!tcpSocket.Connected) return null;

            List<byte> list = new List<byte>();
            do
            {
                if (!tcpSocket.Connected)
                    return list.ToArray();

                ParseTelnet(list);
                System.Threading.Thread.Sleep(TimeOutMs);

            } while (tcpSocket.Available > 0);

            return list.ToArray();//sb.ToString();
        }

        public bool IsConnected
        {
            get { return tcpSocket.Connected; }
        }

        //void ParseTelnet(StringBuilder sb)
        void ParseTelnet(List<byte> list)
        {


            while (tcpSocket.Available > 0)
            {
                int input = tcpSocket.GetStream().ReadByte();

                //if ((input == 0xd0) || (input == 0xd1)) {   
                //    input = (UInt16)(input << 8) | (UInt16)(tcpSocket.GetStream().ReadByte());         
                //}

                switch (input)
                {
                    case -1:
                        break;
                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = tcpSocket.GetStream().ReadByte();
                        if (inputverb == -1) break;
                        switch (inputverb)
                        {
                            case (int)Verbs.IAC:
                                //literal IAC = 255 escaped, so append char 255 to string

                                list.Add((Byte)input);

                                //sb.Append(inputverb);
                                break;
                            case (int)Verbs.DO:
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = tcpSocket.GetStream().ReadByte();
                                if (inputoption == -1) break;
                                tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);
                                if (inputoption == (int)Options.SGA)
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                                else
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);
                                tcpSocket.GetStream().WriteByte((byte)inputoption);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        list.Add((Byte)input); //sb.Append( input );
                        break;
                }
            }
        }
    }
}
