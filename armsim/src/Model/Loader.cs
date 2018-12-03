using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prototype.Unittests;

using System.Runtime.InteropServices;
using System.IO;
namespace Prototype.Model
{


    /// <summary>
    /// Program to load a elf file into simulated ram memory
    /// USAGE: armsim [ --load elf-file ] [ --mem memory-size ] [ --test ]
    /// --load: required, the name of the location of the elf file to load
    /// --mem: optional, default = 32768, the size of the simulated ram
    /// --test: optional, if present disables gui and runs unit tests
    /// </summary>



    // A struct that mimics memory layout of ELF file header
    // See http://www.sco.com/developers/gabi/latest/contents.html for details
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ELF
    {
        public byte EI_MAG0, EI_MAG1, EI_MAG2, EI_MAG3, EI_CLASS, EI_DATA, EI_VERSION;
        byte unused1, unused2, unused3, unused4, unused5, unused6, unused7, unused8, unused9;
        public ushort e_type;
        public ushort e_machine;
        public uint e_version;
        public uint e_entry;
        public uint e_phoff;
        public uint e_shoff;
        public uint e_flags;
        public ushort e_ehsize;
        public ushort e_phentsize;
        public ushort e_phnum;
        public ushort e_shentsize;
        public ushort e_shnum;
        public ushort e_shstrndx;
    }

    //struct that mimics the memory layout of a program header
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct header
    {
        //size of struct 32 bytes
        public uint type;
        public uint offset;
        public uint addr;
        public uint vaddr;
        public uint size;
        uint unused1, unused2, unused3;

    }
    /// <summary>
    /// insures valid args and runs test or loads ram
    /// </summary>
    class Loader
    {

        static memory ram = null; //the simulated ram

        static Options opt = null;//options for the program

        public static int pc = 0;

        public static string errormsg = "";
        public static int load(memory RAM, Options op)
        {
            ram = RAM;
            opt = op;
            return (Load_program());
        }
        /// <summary>
        /// Loads the program into ram
        /// </summary>
        static int Load_program()
        {
            string elfFilename = opt.filename; //file to load
            try
            {
               Console.WriteLine("loading file");
                using (FileStream strm = new FileStream(elfFilename, FileMode.Open))
                {
                    ELF elfHeader = new ELF();
                    byte[] data = new byte[Marshal.SizeOf(elfHeader)];

                    // Read ELF header data
                    strm.Read(data, 0, data.Length);
                    // Convert to struct
                    elfHeader = ByteArrayToStructure<ELF>(data);

                    //check and make sure its an elf file
                    if (elfHeader.EI_MAG1 == 'E' && elfHeader.EI_MAG2 == 'L' && elfHeader.EI_MAG3 == 'F')
                    {

                       Console.WriteLine("Loader: Entry point: " + elfHeader.e_entry.ToString("X4"));
                       Console.WriteLine("Loader: Number of program header entries: " + elfHeader.e_phnum);
                       Console.WriteLine("Loader: size of header: " + elfHeader.e_phentsize);

                        // Read first program header entry
                        strm.Seek(elfHeader.e_phoff, SeekOrigin.Begin);
                        data = new byte[elfHeader.e_phentsize];
                        strm.Read(data, 0, (int)elfHeader.e_phentsize);


                        strm.Seek(elfHeader.e_phoff, 0); //start of first header

                        for (int i = 1; i <= elfHeader.e_phnum; i++)
                        {
                           Console.Write("Loader: segment " + i + " - address = " + strm.Position + ", ");
                            int er = LoadRam(strm);
                            if (er == 1)
                            {
                                errormsg = "ERROR: File too large. Please restart the program with more mem \n\tarmsim --mem {amount of mem}";
                                return 1;
                            }
                        }
                        pc = (int)elfHeader.e_entry;
                        errormsg = "";
                        return 0;
                    }
                    else
                    {
                        
                       Console.WriteLine("Loader: EORROR: File is not in elf format");
                        errormsg = "ERROR: File is not in elf format";
                        return 1;
                    }
                }
            }
            catch (Exception)
            {
               Console.WriteLine("ERROR COULD NOT FIND FILE: " + opt.filename);
               Console.WriteLine("Loader: ERROR COULD NOT FIND FILE: " + opt.filename);
                return 1;
                //System.Environment.Exit(1);
            }
        }

        /// <summary>
        /// loads code pointed to be program headers
        /// </summary>
        /// <param name="strm">A file stream to read bytes from</param>
        static int LoadRam(FileStream strm)
        {
            uint memsize = ram.memsize;

            header pheader = new header();
            byte[] phead = new byte[Marshal.SizeOf(pheader)];
            strm.Read(phead, 0, phead.Length);
            pheader = ByteArrayToStructure<header>(phead);
            if (pheader.size > memsize)
            {
               Console.WriteLine("ERROR: file is too large");
               Console.WriteLine("Loader: file is too large");
                return 1;
            }
            memsize -= pheader.size;
            long position = strm.Position;//end of header/start of next header

            strm.Seek(pheader.offset, 0); //location of code
           Console.WriteLine("Size = " + pheader.size + ", ram address = " + pheader.addr);
            for (int j = 0; j < pheader.size; j++)
            {
                ram.mem[pheader.addr + j] = (byte)strm.ReadByte();
            }
            strm.Seek(position, 0);
            return 0;
        }


        // Converts a byte array to a struct
        static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                typeof(T));
            handle.Free();
            return stuff;
        }

        /// <summary>
        /// runs all unit tests and then exits
        /// </summary>
        public static void RunTests()
        {
           Console.WriteLine("Loader: Test_ReadWord started");
            TestRam.Test_ReadWord();
           Console.WriteLine("Loader: Test_ReadWord completed");

           Console.WriteLine("Loader: Test_ExtractBits started");
            TestRam.Test_ExtractBits();
           Console.WriteLine("Loader: Test_ExtractBits completed");

           Console.WriteLine("Loader: Test_ReadByte started");
            TestRam.Test_ReadByte();
           Console.WriteLine("Loader: Test_ReadByte completed");

           Console.WriteLine("Loader: Test_ReadHalfWord started");
            TestRam.Test_ReadHalfWord();
           Console.WriteLine("Loader: Test_ReadHalfWord completed");

           Console.WriteLine("Loader: Test_SetFlag started");
            TestRam.Test_SetFlag();
           Console.WriteLine("Loader: Test_SetFlag completed");

           Console.WriteLine("Loader: Test_TestFlag started");
            TestRam.Test_TestFlag();
           Console.WriteLine("Loader: Test_TestFlag completed");

           Console.WriteLine("Loader: Test_WriteByte started");
            TestRam.Test_WriteByte();
           Console.WriteLine("Loader: Test_WriteByte completed");

           Console.WriteLine("Loader: Test_WriteHalfWord started");
            TestRam.Test_WriteHalfWord();
           Console.WriteLine("Loader: Test_WriteHalfWord completed");

           Console.WriteLine("Loader: Test_WriteWord started");
            TestRam.Test_WriteWord();
           Console.WriteLine("Loader: Test_WriteWord completed");

           Console.WriteLine("Loader: Tests completed");

        }

    }

}
