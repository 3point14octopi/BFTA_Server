using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using JAFSock;
using System.Collections.Generic;
using BFTA_Server;
using System.Runtime.InteropServices;

namespace SocketHelpers
{
    public static class SocketHelpFunctions
    {
        public static bool CanBeRead(Socket s)
        {
            return s.Poll(1, SelectMode.SelectRead);
        }

        public static bool CanWriteTo(Socket s)
        {
            return s.Poll(1, SelectMode.SelectWrite);
        }

        public static void KillClient(Socket s)
        {
            s.Shutdown(SocketShutdown.Both);
            s.Close();
        }

        public static string ReadSocket(Socket sock)
        {
            byte[] buffer = new byte[512];
            int receivedBytes = 0;
            string messg = null;

            try
            {
                receivedBytes = sock.Receive(buffer);

                messg = Encoding.ASCII.GetString(buffer, 0, receivedBytes);

                if (messg != "QUIT")
                {
                    return messg;
                }
                else
                {
                    return ("end_9669");
                }
            }
            catch (ArgumentNullException aNullExc)
            {
                Console.WriteLine("ArgumentNullException: {0}", aNullExc.ToString());
            }
            catch (SocketException sExc)
            {
                Console.WriteLine("Socket exception: {0}", sExc);
            }
            catch (Exception unexpectedExc)
            {
                Console.WriteLine("Unexpected Exception: {0}", unexpectedExc);
            }

            return ("error_9669");

        }

        public static short ParseInstructType(JClient cli)
        {
            byte[] shortBuff = new byte[2];
            TheWorldsMostUnnecessaryStructure shrt = new TheWorldsMostUnnecessaryStructure();
            try
            {
                cli.m_bufSize = cli.m_socket.Receive(cli.m_receiveBuffer);
                Buffer.BlockCopy(cli.m_receiveBuffer, 0, shortBuff, 0, 2);

                shrt = BytesToShortStuff(shortBuff);

                if(shrt.val < 1 || shrt.val > 2)
                {
                    Console.WriteLine("Could not properly parse command block from {0}. Desynching game states may now occur", cli.m_name);
                    cli.m_deathFlagged = true;
                    return 0;
                }
                else
                {
                    return shrt.val;
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Connection issue has occurred between the server and {0}. {0} will be removed from the update loop.", cli.m_name);
                cli.m_deathFlagged = true;
                return 0;
            }

        }



        public static void PrintEachClientEPInfo(List<JClient> clients)
        {
            foreach (JClient c in clients)
            {
                IPEndPoint clientEP = (IPEndPoint)c.m_socket.RemoteEndPoint;

                Console.WriteLine("Client: {0}    Port: {1}", clientEP.Address, clientEP.Port);
            }
        }

        public static int GetDeadCount(List<JClient> socks)
        {
            int deadCount = 0; 

            for(int i = 0; i < socks.Count; 
                deadCount = (socks[i].m_deathFlagged)?deadCount+1:deadCount,
                i++) { }

            return deadCount;
        }
        
        public static byte[] ServComToByteBlock(ServerCommand s)
        {
            int comSize = Marshal.SizeOf(s);
            byte[] byteBlock = new byte[comSize + 2];

            TheWorldsMostUnnecessaryStructure ind = new TheWorldsMostUnnecessaryStructure();
            ind.Set(s.ComIndex());
            byte[] iBytes = ind.ToBytes();
            Buffer.BlockCopy(iBytes, 0, byteBlock, 0, 2);

            IntPtr memPoint = Marshal.AllocHGlobal(comSize);
            Marshal.StructureToPtr(s, memPoint, false);
            Marshal.Copy(memPoint, byteBlock, 2, comSize);
            Marshal.FreeHGlobal(memPoint);

            return byteBlock;
        }

        public static NetCommand BytesToNetCom(byte[] bytes, NetCommand nCom)
        {
            int obSize = Marshal.SizeOf(nCom);
            IntPtr memPointer = Marshal.AllocHGlobal(obSize);
            Marshal.Copy(bytes, 0, memPointer, obSize);
            Marshal.PtrToStructure(memPointer, nCom);
            Marshal.FreeHGlobal(memPointer);

            return nCom;
        }

        public static T BytesToServCom<T>(byte[] bytes, T sCom)
        {
            int obSize = Marshal.SizeOf(sCom);
            IntPtr memPointer = Marshal.AllocHGlobal(obSize);
            Marshal.Copy(bytes, 0, memPointer, obSize);
            Marshal.PtrToStructure(memPointer, sCom);
            Marshal.FreeHGlobal(memPointer);

            return sCom;
        }


        public static TheWorldsMostUnnecessaryStructure BytesToShortStuff(byte[] bytes)
        {
            TheWorldsMostUnnecessaryStructure str = new TheWorldsMostUnnecessaryStructure();
            int obSize = Marshal.SizeOf(str);

            IntPtr memPointer = Marshal.AllocHGlobal(obSize);
            Marshal.Copy(bytes, 0, memPointer, obSize);
            Marshal.PtrToStructure(memPointer, str);
            Marshal.FreeHGlobal(memPointer);


            Console.WriteLine("Short val: {0}", str.val);

            return str;
        }





        public static bool AllReady(short[] status)
        {
            for(int i = 0; i < status.Length; i++)
            {
                if (status[i] == -1) return false;
            }

            return true;
        }

    }

}