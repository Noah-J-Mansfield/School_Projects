using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// File holding the Ram class. used to simulate ram memory and provide helpful read/write functions
/// also holds banked reg swaping system
/// </summary>

namespace Prototype.Model
{
    /// <summary>
    /// Class to simulate Ram by providing read/write and adrressing
    /// Words in ram are stored in little endian. words = 4 bytes
    /// </summary>

    public class memory
    {
        public event EventHandler writetoConsole;
        public event EventHandler readfromkeyboard;
        public bool flag = false;
        public char key;
        public byte[] mem; //mem

        public uint memsize = 0; //size of mem

        /// <summary>
        /// initializes the Ram
        /// </summary>
        /// <param name="size"> the size of mem in bytes</param>
        public memory(int size)
        {

            mem = new byte[size];
            memsize = (uint)size;
        }

       

        /// <summary>
        /// reads a word from mem
        /// </summary>
        /// <param name="addr"> address in ram to read from. MUST be 0 or divisible by 4</param>
        /// <returns> The word stored in ram that starts at "addr"</returns>
        public int ReadWord(int addr)
        {
            
            if (flag)
            {
                if (addr == 52) { addr = Registers.r13; }
                if (addr == 56) { addr = Registers.r14; }
            }
            //uint addr = unchecked((uint)a);
           //Console.WriteLine("MEMORY: READ: ADDR="+addr+", "+(flag ? "regs":"ram"));
            if ((addr % 4 != 0 || addr > memsize - 4) && addr != 0)
            {

                return 0;
            }


            //converts from little endian to big endian
            
            return mem[addr] + (mem[addr + 1] << 8) + (mem[addr + 2] << 16) + (mem[addr + 3] << 24);


        }
        /// <summary>
        /// reads a halfword from mem
        /// </summary>
        /// <param name="addr"> address in ram to read from. MUST be 0 or divisible by 2</param>
        /// <returns> The halfword stored in ram that starts at "addr"</returns>
        public short ReadHalfWord(int addr)
        {
            if (addr % 2 != 0 || addr >= memsize - 2)
                return 0;


            return (short)(mem[addr] + (mem[addr + 1] << 8));

        }

        /// <summary>
        /// reads a byte from mem
        /// </summary>
        /// <param name="addr"> address in ram to read from </param>
        /// <returns> The byte stored in ram at "addr"</returns>
        public byte ReadByte(int addr)
        {
            if (addr == 0x100001 && readfromkeyboard != null)
            {
                readfromkeyboard(this, EventArgs.Empty);
                return (byte)key;
            }
            
            if (addr < memsize)
            {

                return mem[addr];
            }
            return 0;
        }

        /// <summary>
        /// writes a word to mem
        /// </summary>
        /// <param name="addr"> address in memory to write word. MUST be 0 or divisible by 4</param>
        /// <param name="word"> an int to write to memorry</param>
        public void WriteWord(int addr, int word)
        {
            if (flag)
            {
                if (addr == 52) { addr = Registers.r13; }
                if (addr == 56) { addr = Registers.r14; }
            }
            //uint addr = unchecked((uint)a);
           //Console.WriteLine("MEMORY: ADDRESS = " + addr);
           //Console.WriteLine("MEMORY: WORD = " + word);
           
            if (addr % 4 == 0 || addr >= memsize - 4 || addr == 0)
            {
                
                //write each byte form word to memory while converting word to little endian 
                mem[addr] = (byte)(word & 255);
                mem[addr + 1] = (byte)((word >> 8) & 255);
                mem[addr + 2] = (byte)((word >> (16)) & 255);
                mem[addr + 3] = (byte)((word >> (24)) & 255);
            }
            if (addr == 72 && word == 0xfc) {//Console.WriteLine("\n********************\nMEMORY: HERE\n********************\n");
            }

        }

        /// <summary>
        /// writes a halfword to mem
        /// </summary>
        /// <param name="addr"> address in memory to write halfword. MUST be 0 or divisible by 2</param>
        /// <param name="hword"> an short to write to memorry</param>
        public void WriteHalfWord(int addr, short hword)
        {
            if (addr % 2 == 0 || addr >= (memsize - 2) || addr == 0)
            {
                //write each byte form hword to memory while converting hword to little endian 
                mem[addr] = (byte)(hword & 255);
                mem[addr + 1] = (byte)((hword >> 8) & 255);

            }
        }

        /// <summary>
        /// writes a byte to mem
        /// </summary>
        /// <param name="addr"> address in memory to write byte</param>
        /// <param name="hword"> a byte to write to memorry</param>
        public void WriteByte(int addr, byte b)
        {
            if (addr == 0x100000 && writetoConsole != null)
            {
                key = (char)b;
                writetoConsole(this, EventArgs.Empty);
                return;
            }

            if (addr >= memsize)
                return;
            mem[addr] = b;
        }

        /// <summary>
        /// Displays the contents of mem
        /// </summary>
        public void dump_mem()
        {
            int k = 0;
            foreach (byte b in mem)
            {
                k++;
                //Console.Write(b + " ");
                if (k % 40 == 0) { }
                   //Console.Write("\n");
            }
            //Console.Write("\n");
        }

        /// <summary>
        /// performs a calculation on all bytes in mem. is used to determine if a program was correctly loaded into mem
        /// </summary>
        /// <returns>the result of the calculation</returns>
        public uint Checksum()
        {
            uint cksum = 0;

            for (int i = 0; i < memsize; ++i)
            {
                cksum += (uint)(mem[i] ^ i);
            }
            return cksum;
        }

        /// <summary>
        /// reads a word from memory and determines if the selected bit is 1 or 0
        /// </summary>
        /// <param name="addr"> the address of the word in memory to read</param>
        /// <param name="bit"> the position of the bit to check [0-31]</param>
        /// <returns></returns>
        public bool TestFlag(int addr, int bit)
        {



            return (((ReadWord(addr) >> bit) & 1) == 1);



        }

        public static bool testBit(int word, int bit)
        {
            return (((word >> bit) & 1) == 1);
        }

        public static int setbit(int word, int bit, bool flag)
        {

            if (flag)
                word = word | (1 << bit);
            else
            {
                uint w = (uint)word;
                uint b = (uint)(0b1 << bit);
                w = w & (0xffffffff - b);
                word = unchecked((int)w);
            }
                //word = word & (unchecked((uint)0xFFFFFFFF) - (uint)(1 << bit));
            return word;
        }
            public void reset()
        {
            
            for(int i =0; i< memsize;i++)
            {
                mem[i] = 0;
            }
        }

        /// <summary>
        /// reads a word from memory and sets the selected bit to 1 or 0
        /// </summary>
        /// <param name="addr">the address of the word in memory to read</param>
        /// <param name="bit"> the position of the bit to set [0-31]</param>
        /// <param name="flag">what to set the bit to (0 or 1)</param>
        public void SetFlag(int addr, int bit, bool flag)
        {
            int word = ReadWord(addr);
            if (flag)
                WriteWord(addr, word | (1 << bit));
            else
                WriteWord(addr, word & (256 - (1 << bit)));

        }

        /// <summary>
        /// takes a word and selects a range of bits and sets all bits outside that range to zero
        /// </summary>
        /// <param name="word"> the word to holding the bits to extract</param>
        /// <param name="startbit"> the position of the first bit to extract</param>
        /// <param name="endbit"> the position of the last bit to extract</param>
        /// <returns> a bit mask consitiong of the bits from range [startbit-endbit]</returns>
        public int ExtractBits(int word, int startbit, int endbit)
        {
            return (    (word >> startbit) & (1 << (endbit + 1 - startbit))  -1  ) << startbit;
        }

        public static int ExtractBits_shifted(int word, int startbit, int endbit)
        {
            return ((word >> startbit) & (1 << (endbit + 1 - startbit)) - 1);
        }


    }

    static class flags
    {
        public const int N = 31;
        public const int Z = 30;
        public const int C = 29;
        public const int V = 28;
    }

    static class Registers
    {
        public static int state = 0; // 0=sys, 1=svc, 2=irq
        public static int r0 = 0;
        public static int r1 = 4;
        public static int r2 = 8;
        public static int r3 = 12;
        public static int r4 = 16;
        public static int r5 = 20;
        public static int r6 = 24;
        public static int r7 = 28;
        public static int r8 = 32;
        public static int r9 = 36;
        public static int r10 = 40;
        public static int r11 = 44;
        public static int r12 = 48;
        public static int r13 { get { return state == 0 ? 52 : (state == 1 ? 68 : 80); } }
        public static int r14 { get{ return state == 0 ? 56 : (state == 1 ? 72 : 84); } }
        public static int r15 = 60;
        public static int CPSR = 64;
        public static int SPSR { get {return state == 1 ? 76: 88; } }
    }
}
