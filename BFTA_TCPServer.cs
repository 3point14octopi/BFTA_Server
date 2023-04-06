using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using SocketHelpers;
using JAFSock;
using System.Runtime.InteropServices;

namespace BFTA_Server
{

    
    class BFTA_TCPServer
    {
        public static bool turnEnded = false;
        public static bool isSinglePlayer = false;
        public static short[] characters;
        public static bool[] eliminated;
        
        public static void SendServMessTo(JClient player, ServerCommand sCom)
        {
            if(!player.m_deathFlagged && SocketHelpFunctions.CanWriteTo(player.m_socket)){
                try
                {
                    byte[] comByte = SocketHelpFunctions.ServComToByteBlock(sCom);
                    byte[] outCom = new byte[4 + comByte.Length];
                    TheWorldsMostUnnecessaryStructure ind = new TheWorldsMostUnnecessaryStructure();
                    ind.Set(3);
                    byte[] iBytes = ind.ToBytes();
                    Buffer.BlockCopy(iBytes, 0, outCom, 0, 2);
                    ind.Set(1);
                    iBytes = ind.ToBytes();
                    Buffer.BlockCopy(iBytes, 0, outCom, 2, 2);
                    Buffer.BlockCopy(comByte, 0, outCom, 4, comByte.Length);


                    player.m_socket.Send(outCom);
                    Console.WriteLine("Sent server command to {0}", player.m_name);
                }
                catch (Exception)
                {
                    player.m_deathFlagged = true;
                }
            }
        }

        public static void ServerUpdateAll(List<JClient> players, ServerCommand sCom)
        {
            byte[] toSend = SocketHelpFunctions.ServComToByteBlock(sCom);
            for (int i = 0; i < players.Count; SendServMessTo(players[i], sCom), i++) { }
        }

        public static void SendTurnUpdate(List<JClient> players, Queue<JPlayerCommandBlock> comQueue)
        {
            for(int i = 0; i < players.Count; i++)
            {
                if(!players[i].m_deathFlagged && SocketHelpFunctions.CanWriteTo(players[i].m_socket))
                {
                    try
                    {
                        if (isSinglePlayer || (comQueue.Peek().m_sender != players[i].m_name))
                            players[i].m_socket.Send(comQueue.Peek().m_commandQueue);
                    }
                    catch (Exception)
                    {
                        players[i].m_deathFlagged = true;
                    }
                }
            }

            comQueue.Dequeue();
        }



        public static void ParseExternalServerCommand(JClient sender)
        {
            byte[] shortBuff = new byte[2];
            byte[] parseBuff = new byte[sender.m_bufSize - 4];

            Buffer.BlockCopy(sender.m_receiveBuffer, 2, shortBuff, 0, 2);
            TheWorldsMostUnnecessaryStructure str = SocketHelpFunctions.BytesToShortStuff(shortBuff);
            Buffer.BlockCopy(sender.m_receiveBuffer, 4, parseBuff, 0, parseBuff.Length);

            if(str.val == 2)
            {
                if (!sender.m_isReady)
                {
                    ReadyCommand rCom = SocketHelpFunctions.BytesToServCom(parseBuff, new ReadyCommand());
                    characters[sender.m_indexInLobby] = rCom.selectedCharacter;
                    Console.WriteLine("{0} has selected a character", sender.m_name);
                }
            }
            else if(str.val == 6)
            {
                MarkPlayerAsEliminated ripCom = SocketHelpFunctions.BytesToServCom(parseBuff, new MarkPlayerAsEliminated());
                eliminated[ripCom.elimIndex] = true;
                Console.WriteLine("Player{0} has eliminated Player{1}!", sender.m_indexInLobby, ripCom.elimIndex);
            }
            else{
                Console.WriteLine("A ServerCommand was sent, but it could not be parsed properly or was invalid.");
            }
        }

        public static void HandleReceived(JClient j, Queue<JPlayerCommandBlock> outMessages, Queue<JServerCommand>ServerMessages)
        {
            
            short comType = SocketHelpFunctions.ParseInstructType(j);

            if(comType == 1)
            {
                outMessages.Enqueue(j.ConvertToComBlock());
                turnEnded = true;
            }else if (comType == 3)
            {
                ParseExternalServerCommand(j);
            }
            else if(comType == 0 && j.m_deathFlagged)
            {
                SocketHelpFunctions.KillClient(j.m_socket);
            }

        }



        public static void StartGame(List<JClient> players)
        {
            SetCharacterOrder charOr = new SetCharacterOrder();
            CharSelectionsAndPlayerIndex csapi = new CharSelectionsAndPlayerIndex();
            StartGameCommand sgCom = new StartGameCommand();
            short a = (!isSinglePlayer)?(short)1:(short)0;
            short x = 0;
            for (; x < players.Count;
                //this ONLY works because we have 2 players
               
                charOr.Setup(x, a),
                csapi.Setup(x, characters[x], characters[a]),
                SendServMessTo(players[x], csapi),
                System.Threading.Thread.Sleep(100),
                SendServMessTo(players[x], sgCom),
                System.Threading.Thread.Sleep(1000),
                SendServMessTo(players[x], charOr), 
                x++,
                a = (x == characters.Count() -1)?(short)0:(short)(x+1));

            

            System.Threading.Thread.Sleep(1000);

            ArenaSetupCommand arSet = new ArenaSetupCommand();
            arSet.Setup();
            for (int i = 0; i < players.Count;
                SendServMessTo(players[i], arSet), i++) ;


            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("Starting the game!");
        }



        public static void GameLoop(List<JClient> players)
        {
            Queue<JPlayerCommandBlock> outMessages = new Queue<JPlayerCommandBlock> { };
            Queue<JServerCommand> sMes = new Queue<JServerCommand> { };
            StartTurnCommand startTurn = new StartTurnCommand();
            SendServMessTo(players[0], startTurn);





            int deactive = 0;
            int turn = 0;
            

            while(deactive < players.Count)
            {
                for(int i = 0; i < players.Count; i++)
                {
                    if(!players[i].m_deathFlagged && SocketHelpFunctions.CanBeRead(players[i].m_socket))
                    {
                        HandleReceived(players[i], outMessages, sMes);
                    }
                }

                if (outMessages.Count > 0)
                {
                    SendTurnUpdate(players, outMessages);
                    Console.WriteLine("Sent");
                }


                deactive = SocketHelpFunctions.GetDeadCount(players);


                if (turnEnded)
                {
                    int oldTurn = turn;
                    turn = (turn == 0) ? 1 : 0;
                    turn = (isSinglePlayer) ? 0 : turn;
                    System.Threading.Thread.Sleep(100);
                    if (eliminated[turn])
                    {
                        FlashWinLoss wl = new FlashWinLoss();
                        wl.endCond = 1;
                        SendServMessTo(players[oldTurn], wl);
                        Console.WriteLine("{0} has won!", players[oldTurn].m_name);
                        System.Threading.Thread.Sleep(100);
                        wl.endCond = 0;
                        SendServMessTo(players[turn], wl);

                        turnEnded = false;
                    }
                    else
                    {
                        SendServMessTo(players[turn], startTurn);
                        turnEnded = false;
                    }
                    
                }
            }

            Console.WriteLine("All players have disconnected. Closing the server...");
            
        }

        public static void IdlingHell(List<JClient> clients)
        {
            Console.WriteLine("Waiting for players to select characters...");
            while (!SocketHelpFunctions.AllReady(characters))
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    if (!clients[i].m_deathFlagged && SocketHelpFunctions.CanBeRead(clients[i].m_socket))
                    {
                        HandleReceived(clients[i], new Queue<JPlayerCommandBlock>(), new Queue<JServerCommand>());
                    }
                }
            }
            Console.WriteLine("All players have selected a character!");
            
        }


        public static void GetClients(List<JClient> clients, Socket server, int maxConnections)
        {

            int currentClients = 0;

            try
            {
                server.Listen(maxConnections);

                Console.WriteLine("Open for connections...");

                while (currentClients < (maxConnections))
                {
                    clients.Add(new JClient(currentClients, "Player_0" + (currentClients + 1).ToString()));
                    clients[currentClients].m_socket = server.Accept();
                    currentClients++;

                    Console.WriteLine("{0} connected", clients[currentClients - 1].m_name);
                    for (int i = 0; i < currentClients - 1; i++)
                    {
                        //clients[i].m_socket.Send(Encoding.ASCII.GetBytes("[SERVER]: " + clients[currentClients - 1].m_name + " joined. Waiting for users..."));
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        public static void StartServer(int users, string ipAd)
        {

            //Setup server
            IPAddress ip = IPAddress.Parse(ipAd);
            IPEndPoint localEP = new IPEndPoint(ip, 9669);
            Socket server = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            List<JClient> clientSocks = new List<JClient> { };



            try
            {
                server.Bind(localEP);

                //Accept connections 
                GetClients(clientSocks, server, users);
                SocketHelpFunctions.PrintEachClientEPInfo(clientSocks);

                //talk!
                IdlingHell(clientSocks);
                StartGame(clientSocks);
                GameLoop(clientSocks);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


        }
        public static int Main(string[] args)
        {
            Console.WriteLine("this shitty server uses 127.0.0.1 as the default. isn't that nice?");
            Console.Write("Enter users: ");
            int s = int.Parse(Console.ReadLine());
            characters = new short[s];
            eliminated = new bool[s];
            for (int itr = 0; itr < s; characters[itr] = -1, eliminated[itr] = false, itr++) ;
            if (s == 1) isSinglePlayer = true;
            //Console.Write("Enter the IP address: ");
            string i = "127.0.00.1";
            StartServer(s, i);
            
            
            return 0;


        }
    }
}
