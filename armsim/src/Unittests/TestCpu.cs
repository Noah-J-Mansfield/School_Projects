using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prototype.Model;
using System.Diagnostics;
namespace Prototype.Unittests
{
    class TestCpu
    {
        static CPU cpu;
        static memory ram;
        public static void runtests()
        {
            ram = new memory(100);
            ram.WriteWord(4, 0xFFFF);
            
            cpu = new CPU(ram, new memory(15 * 16));
        }

        public static void Test_fetch()
        {
            cpu.pc = 4;
            Debug.Assert(cpu.fetch()  == 0xFFFF);
            Debug.Assert(cpu.pc == 8);
        }
        public static void Test_decode()
        {
            //NOT IMPLEMENTED: no tests yet
        }
        public static void Test_execute()
        {
            //NOT IMPLEMENTED: no tests yet
        }

        public static void Test_add_breakpoint()
        {
            cpu.add_breakpoint(0x0010);
            cpu.pc = 0x0010;
            Debug.Assert(cpu.isbreakpoint());
            cpu.pc = 0xC0;
            Debug.Assert(cpu.isbreakpoint());
        }
    }
}
