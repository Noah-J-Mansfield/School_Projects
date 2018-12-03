using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prototype.Model;
using System.Diagnostics;
namespace Prototype.Unittests
{
    class TestComputer
    {
        static Computer com;
        static memory ram;
        public static void runtests()
        {
            string[] f = { "", "" };
            Options opt = new Options(f);
            com = new Computer(opt);
            ram = com.Getram();
            ////com.Getram().
            //Test_add_breakpoint();
        }

        public static void Test_getlineofmem()
        {
            ram.WriteWord(16,0x0000BEEF);
            ram.WriteWord(20, 0x0000DEAD);
            ram.WriteWord(24, 0x0000BEEF);
            ram.WriteWord(28, 0x0000DEAD);
            Debug.Assert(com.get_Line_of_memory(0x10)[0] == "0x00000010" && com.get_Line_of_memory(0)[1] == " EF BE 00 00 AD DE 00 00 EF BE 00 00 AD DE 00 00");
        }
        public static void Tes_()
        {

        }
        public static void Test_d_breakpoint()
        {

        }
        public static void Test_ad_breakpoint()
        {

        }
    }
}
