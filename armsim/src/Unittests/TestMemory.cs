using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prototype.Model;

/// <summary>
/// Contains the unit tests for ram
/// </summary>
namespace Prototype.Unittests
{
    /// <summary>
    /// unit tests for ram
    /// </summary>
    class TestRam
    {
        //ram to be shared by all unit tests
        public static memory ram = new memory(16);


        public static void Test_ReadWord()
        {
            ram.mem[0] = 0x12;
            ram.mem[1] = 0x34;
            ram.mem[2] = 0x56;
            ram.mem[3] = 0x78;

            ram.mem[4] = 0x90;
            ram.mem[5] = 0x31;
            ram.mem[6] = 0x78;
            ram.mem[7] = 0x43;

            ram.mem[8] = 0x19;
            ram.mem[9] = 0x24;
            ram.mem[10] = 0x66;
            ram.mem[11] = 0x75;

            ram.mem[12] = 0x75;
            ram.mem[13] = 0x32;
            ram.mem[14] = 0x13;
            ram.mem[15] = 0x86;


            Debug.Assert(ram.ReadWord(4) == 0x43783190);
            Debug.Assert(ram.ReadWord(3) == 0);
            Debug.Assert(ram.ReadWord(16) == 0);
        }

        public static void Test_ReadHalfWord()
        {


            Debug.Assert(ram.ReadHalfWord(4) == 0x3190);
            Debug.Assert(ram.ReadHalfWord(6) == 0x4378);
            Debug.Assert(ram.ReadHalfWord(5) == 0);
            Debug.Assert(ram.ReadHalfWord(16) == 0);
        }

        public static void Test_ReadByte()
        {

            Debug.Assert(ram.ReadByte(4) == 0x90);
            Debug.Assert(ram.ReadByte(16) == 0);
            Debug.Assert(ram.ReadByte(6) == 0x78);
        }

        public static void Test_WriteWord()
        {
            ram.WriteWord(4, 0x08765432);
            Debug.Assert(ram.mem[4] == 0x32 && ram.mem[5] == 0x54 && ram.mem[6] == 0x76 && ram.mem[7] == 0x08);
            ram.WriteWord(5, 0x12345678);
            Debug.Assert(ram.mem[5] == 0x54 && ram.mem[6] == 0x76 && ram.mem[7] == 0x08);
        }



        public static void Test_WriteHalfWord()
        {
            ram.WriteWord(4, 0x5796);
            Debug.Assert(ram.mem[4] == 0x96 && ram.mem[5] == 0x57);
            ram.WriteWord(5, 0x8888);
            Debug.Assert(ram.mem[5] == 0x57);
        }

        public static void Test_WriteByte()
        {
            ram.WriteByte(7, 0x77);
            Debug.Assert(ram.mem[7] == 0x77);
        }

        public static void Test_SetFlag()
        {
            memory flageram = new memory(4);
            flageram.mem[0] = 0x01;
            flageram.mem[1] = 0x00;
            flageram.mem[2] = 0x01;
            flageram.mem[3] = 0x00;


            Debug.Assert(flageram.TestFlag(0, 0) == true);
        }

        public static void Test_ExtractBits()
        {
            Debug.Assert(ram.ExtractBits(0xb5, 1, 3) == 0x04);
            Debug.Assert(ram.ExtractBits(0x01, 0, 0) == 0x01);
        }

        public static void Test_TestFlag()
        {
            Debug.Assert(ram.TestFlag(0, 5) == false);
            Debug.Assert(ram.TestFlag(0, 4) == true);
        }

        public static void Test_Setflag()
        {
            memory flageram = new memory(4);
            flageram.mem[0] = 0x01;
            flageram.mem[1] = 0x00;
            flageram.mem[2] = 0x01;
            flageram.mem[3] = 0x00;

            flageram.SetFlag(0,7,true);
            Debug.Assert(flageram.mem[0] == 0x81);
        }
    }
}
