﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace BFTA_Server
{
    public interface NetCommand
    {
        void Execute();
        void Inverse();
        short ComIndex();
    }

    [StructLayout(LayoutKind.Explicit, Size = 26)]
    public class MoveChar : NetCommand
    {
        [FieldOffset(0)] short indexMover;

        [FieldOffset(2)] float posX;
        [FieldOffset(6)] float posY;
        [FieldOffset(10)] float posZ;

        [FieldOffset(14)] float eulerX;
        [FieldOffset(18)] float eulerY;
        [FieldOffset(22)] float eulerZ;

        public void Setup()
        {
            
        }

        public void Execute()
        {
            //Debug.Log("WOAH");
            Console.WriteLine("Added ({0}, {1}, {2}) to Player_0{3}'s position", posX.ToString(), posY.ToString(), posZ.ToString(), indexMover.ToString());
            Console.WriteLine("Set Player_0{0}'s rotation to euler angles ({1}, {2}, {3})", indexMover.ToString(), eulerX.ToString(), eulerY.ToString(), eulerZ.ToString());
        }

        public void Inverse()
        {
            //Debug.Log("SICK");
            Console.WriteLine("Subtracted ({0}, {1}, {2}) from Player_0{3}'s position", posX.ToString(), posY.ToString(), posZ.ToString(), indexMover.ToString());
            Console.WriteLine("Set Player_0{0}'s rotation to euler angles ({1}, {2}, {3})", (eulerX).ToString(), (eulerY).ToString(), (eulerZ).ToString());
        }

        public short ComIndex() { return 0; }
    }


    public interface ServerCommand
    {
        void Execute();
        short ComIndex();
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public class ArenaSetupCommand : ServerCommand
    {
        [FieldOffset(0)] public int rngSeed;
        [FieldOffset(4)] public short arenaType;
        [FieldOffset(6)] public short playerIndex;

        public void Setup(short pI = 0)
        {
            rngSeed = 1234;
            arenaType = 1;
            playerIndex = pI;
        }
        public void Execute()
        {
            Console.WriteLine("Seed set to {0}", rngSeed);
            switch (arenaType)
            {
                case (1):
                    Console.WriteLine("Arena type: Western");
                    break;
                default:
                    Console.WriteLine("Arena type: Default");
                    break;
            }

            //NetworkParser.aGenRef.GetComponent<MakeArenaArray>().GenerateNewTerrain();

            //if we have multiple players we will do a thing trademark where we determine where 
            //all of the players actually get positioned on the map
        }

        public short ComIndex()
        {
            return 0;
        }

        public byte[] ToBytes()
        {
            int obSize = Marshal.SizeOf(this);

            byte[] bytes = new byte[obSize];
            IntPtr memPoint = Marshal.AllocHGlobal(obSize);
            Marshal.StructureToPtr(this, memPoint, false);
            Marshal.Copy(memPoint, bytes, 0, obSize);
            Marshal.FreeHGlobal(memPoint);

            return bytes;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 6)]
    public class CharSelectionsAndPlayerIndex : ServerCommand
    {
        [FieldOffset(0)] public short playerIndex;
        [FieldOffset(2)] public short localPlayerSelection;
        [FieldOffset(4)] public short remotePlayerSelection;

        public void Setup(short a = 0, short b = 0, short c = 0)
        {
            playerIndex = a;
            localPlayerSelection = b;
            remotePlayerSelection = c;
        }
        
        public void Execute()
        {
            //:boppin:
        }

        public short ComIndex()
        {
            return 1;
        }
    }


    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public class CharacterOrderCommand : ServerCommand
    {
        [FieldOffset(0)] public short ind1;
        [FieldOffset(2)] public short char1;
        [FieldOffset(4)] public short ind2;
        [FieldOffset(6)] public short char2;

        public void Setup(short a = 0, short b = 0, short c = 1, short d = 1)
        {
            ind1 = a;
            ind2 = b;
            char1 = c;
            char2 = d;
        }

        public void Execute()
        {
            //piss and cry
        }

        public short ComIndex()
        {
            return 1;
        }

    }

    [StructLayout(LayoutKind.Explicit, Size = 2)]
    public class ReadyCommand : ServerCommand
    {
        [FieldOffset(0)] public short selectedCharacter;

        public void Execute()
        {
            //cope
        }

        public short ComIndex()
        {
            return 2;
        }
    }

    

    [StructLayout(LayoutKind.Explicit, Size = 2)]
    public class StartTurnCommand : ServerCommand
    {
        public void Execute()
        {
            //seethe?
        }

        public short ComIndex()
        {
            return 3;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 2)]
    public class StartGameCommand : ServerCommand
    {
        public void Execute()
        {
            //cry
        }

        public short ComIndex()
        {
            return 4;
        }
    }


    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public class SetCharacterOrder : ServerCommand
    {
        [FieldOffset(0)] public short p1Index;
        [FieldOffset(2)] public short p2Index;

        public void Setup(short a = 0, short b = 0)
        {
            p1Index = a;
            p2Index = b;
        }

        public void Execute()
        {
            //i wish i was the one being executed rn :(
        }

        public short ComIndex()
        {
            return 5;
        }
    }









    [StructLayout(LayoutKind.Explicit, Size = 2)]
    public class TheWorldsMostUnnecessaryStructure
    {
        [FieldOffset(0)] public short val;

        public void Set(short x)
        {
            val = x;
        }

        public byte[] ToBytes()
        {
            int obSize = Marshal.SizeOf(this);

            byte[] bytes = new byte[obSize];
            IntPtr memPoint = Marshal.AllocHGlobal(obSize);
            Marshal.StructureToPtr(this, memPoint, false);
            Marshal.Copy(memPoint, bytes, 0, obSize);
            Marshal.FreeHGlobal(memPoint);

            return bytes;
        }
    }
}
