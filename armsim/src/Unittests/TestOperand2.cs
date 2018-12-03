using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Prototype.Instructions;
using Prototype.Model;

namespace Prototype.Unittests
{
    class TestOperand2
    {
        public static void RunTests()
        {
            Test_shift_left();
            Test_shift_right();
            Test_a_shift_right();
            Test_ror();
            Test_imm();
            Test_shift();
            Test_imm_shift();
            
        }


        public static void Test_shift_left()
        {
           //Console.WriteLine("OPERAND2TESTS: TESTING LEFT SHIFT");
            Operand2 op2 = new Operand2(0xFFFF);
            
            Debug.Assert(op2.Lsl(0b11,2) == 0b1100);

            Debug.Assert(op2.Lsl(0x1,27) == 0x08000000);
        }
        public static void Test_shift_right()
        {
           //Console.WriteLine("OPERAND2TESTS: TESTING RIGHT SHIFT");
            Operand2 op2 = new Operand2(0x32);
            Debug.Assert(op2.Lsr(0b1001, 2) == 0b10);
            Debug.Assert(op2.Lsr(unchecked((int)0x80000000), 1) == 0x40000000);

        }

        public static void Test_a_shift_right()
        {
           //Console.WriteLine("OPERAND2TESTS: TESTING RIGHT ARITHMATIC SHIFT");
            Operand2 op2 = new Operand2(32);

            Debug.Assert(op2.Asr(unchecked((int)0b11111111111111111111111111111111), 1) == unchecked((int)0b11111111111111111111111111111111));
            Debug.Assert(op2.Asr(unchecked((int)0b01111111111111111111111111111111), 1) == unchecked((int)0b00111111111111111111111111111111));
        }
        
        public static void Test_ror()
        {
           //Console.WriteLine("OPERAND2TESTS: TESTING ROR SHIFT");
            Operand2 op2 = new Operand2(56);



            Debug.Assert(op2.Ror(0b1, 2) == unchecked((int)0x40000000));
            Debug.Assert(op2.Ror(0b101, 4) == unchecked((int)0x50000000));
        }

        public static void Test_imm()
        {
           //Console.WriteLine("OPERAND2TEST: TEST_IMM");
            Operand2 i = new Operand2(0xFFF);
            Debug.Assert(i.Compute() == 0xfff);
        }
        public static void Test_shift()
        {
           //Console.WriteLine("OPERAND2TEST: TEST_SHIFT");
            CPU cpu = new CPU(new memory(4), new memory(64));

            cpu.regs.WriteWord(Registers.r2, 0b11);
            shift s = new shift(0x202, cpu);
           //Console.WriteLine("OPERAND2TEST: TEST_SHIFT: s = "+s.Compute());
            Debug.Assert(s.Compute() == 0b110000);

            cpu.regs.WriteWord(Registers.r4, 0x4);
            cpu.regs.WriteWord(Registers.r2, 0b11);
            s = new shift(0x412,cpu);
           //Console.WriteLine("OPERAND2TEST: TEST_SHIFT: s = " + s.Compute());
            Debug.Assert(s.Compute() == 0b110000);
        }
        public static void Test_imm_shift()
        {
           //Console.WriteLine("OPERAND2TEST: TEST_IMM_SHIFT");
        }
    }
}
