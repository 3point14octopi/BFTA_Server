using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace JAFSock
{
    public class JClient
    {
        public string m_name;
        public Socket m_socket;
        public byte[] m_receiveBuffer;
        public int m_bufSize;
        public bool m_deathFlagged;


        //spaghetti for BFTA
        public bool m_isReady;
        public bool m_isDoneTurn;
        public int m_indexInLobby;

        public JClient(int ind, string name = "default", Socket s = null)
        {
            m_indexInLobby = ind;


            m_name = name;
            m_socket = s;

            m_deathFlagged = false;
            m_isReady = false;
            m_isDoneTurn = true;

            m_receiveBuffer = new byte[1024];
        }

        public JPlayerCommandBlock ConvertToComBlock()
        {
            return new JPlayerCommandBlock(m_name, m_receiveBuffer, m_bufSize);
        }
    }

    public class JClientMessage
    {
        public string m_contents;
        public string m_sender;

        public JClientMessage(string s, string c)
        {
            m_sender = s;
            m_contents = c;
        }

        public string FormatToSendable()
        {
            return ("[" + m_sender + "]: " + m_contents);
        }
    }

    public  interface JNetComm
    {

    }

    public class JPlayerCommandBlock : JNetComm
    {
        public string m_sender;
        public byte[] m_commandQueue;

        public JPlayerCommandBlock(string n, byte[] buff, int s)
        {
            m_sender = n;

            m_commandQueue = new byte[s];
            Buffer.BlockCopy(buff, 0, m_commandQueue, 0, s);
            Console.WriteLine("Created a new command block from {0} with a buffer size of {1}", n, s.ToString());
        }
    }

    public class JServerCommand : JNetComm
    {
        public byte[] m_command;
        
        public JServerCommand(string str)
        {
            m_command = new byte[Marshal.SizeOf(str) + 2];
            byte[] shrt = new byte[2];
            shrt = BitConverter.GetBytes((short)1);
           Array.Copy(shrt, 0, m_command, 0, 2);
            byte[] strBuff = new byte[Marshal.SizeOf(str)];
            strBuff = Encoding.ASCII.GetBytes(str);
            Array.Copy(strBuff, 0, m_command, 2, strBuff.Length);
        }
    }

}
