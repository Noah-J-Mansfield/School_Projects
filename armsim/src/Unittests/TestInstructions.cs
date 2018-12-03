using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prototype.Instructions;
using Prototype.Model;
using System.Diagnostics;
namespace Prototype.Unittests
{
    public class TestInstructions
    {
        static memory regs;
        static CPU c = new CPU(new memory(1000), new memory(64));
        public static void run()
        {
            test_Make_Instruction();
            Test_Move();
            Test_ADD();
            Test_BIC();
            Test_EOR();
            Test_MUL();
            Test_MVN();
            Test_ORR();
            Test_RSB();
            Test_SUB();
            Test_AND();
            
            Test_STR();
            Test_LDR();

            
       
            
            

        }

        public static void test_Make_Instruction()
        {
           //Console.WriteLine("TESTINSTRUCTION: testing make instruction");
            CPU c = new CPU(new memory(4), new memory(4));
            Debug.Assert(MakeInstuction.make(0x0, c, 69) is Instruction);
            Debug.Assert(MakeInstuction.make(unchecked((int)0xE2821082), c,69) is ADD);
            Debug.Assert(MakeInstuction.make(unchecked((int)0xE3A0300A), c,69) is Move);
            Debug.Assert(MakeInstuction.make(unchecked((int)0xE5A41100), c,69) is ldr_str);
            Debug.Assert(MakeInstuction.make(unchecked((int)0xE4D49010), c,69) is ldr_str);
            Debug.Assert(MakeInstuction.make(unchecked((int)0xE2000000), c,69) is AND);
            Debug.Assert(MakeInstuction.make(unchecked((int)0xE2200000), c,69) is EOR);
            Debug.Assert(MakeInstuction.make(unchecked((int)0xE0421082), c,69) is SUB);
            Debug.Assert(MakeInstuction.make(unchecked((int)0xE0621082), c,69) is RSB);
            Debug.Assert(MakeInstuction.make(unchecked((int)0xE1821082), c,69) is ORR);
            Debug.Assert(MakeInstuction.make(unchecked((int)0xE1C21082), c,69) is BIC);
            Debug.Assert(MakeInstuction.make(unchecked((int)0xE1E21082), c,69) is MVN);
            Debug.Assert(MakeInstuction.make(unchecked((int)0xE9221082), c,69) is ldmfd_stmfd);
            Debug.Assert(MakeInstuction.make(unchecked((int)0xE9201082), c,69) is ldmfd_stmfd);

        }
        public static void Test_AND()
        {
            AND a = new AND(unchecked((int)0xE2021082), c);
            c.regs.WriteWord(Registers.r2, 0x102);
            a.Execute();
            Debug.Assert(c.regs.ReadWord(Registers.r1) == (0x102 & 0x82));
        }
        public static void Test_SUB()
        {
            SUB s = new SUB(unchecked((int)0xE2421082), c);
            c.regs.WriteWord(Registers.r2, 0x102);
            s.Execute();
            Debug.Assert(c.regs.ReadWord(Registers.r1) == (0x102 - 0x82));
        }
        public static void Test_ADD()
        {
            ADD a = new ADD(unchecked((int)0xE2821082), c);
            c.regs.WriteWord(Registers.r2, 0x102);
            a.Execute();
            Debug.Assert(c.regs.ReadWord(Registers.r1) == (0x102 + 0x82));
        }
        public static void Test_BIC()
        {
            BIC s = new BIC(unchecked((int)0xE3c21082), c);
            c.regs.WriteWord(Registers.r2, 0x102);
            s.Execute();
            Debug.Assert(c.regs.ReadWord(Registers.r1) == (0x102 & ~0x82));
        }
        public static void Test_RSB()
        {
            RSB s = new RSB(unchecked((int)0xE2621082), c);
            c.regs.WriteWord(Registers.r2, 0x102);
            s.Execute();
            Debug.Assert(c.regs.ReadWord(Registers.r1) == (0x82 - 0x102));
        }
        public static void Test_MVN()
        {
            MVN s = new MVN(unchecked((int)0xE3E21082), c);
            c.regs.WriteWord(Registers.r2, 0x102);
            s.Execute();
            Debug.Assert(c.regs.ReadWord(Registers.r1) == (~0x82));
        }
        public static void Test_ORR()
        {
            ORR s = new ORR(unchecked((int)0xE3821082), c);
            c.regs.WriteWord(Registers.r2, 0x102);
            s.Execute();
            Debug.Assert(c.regs.ReadWord(Registers.r1) == (0x102 | 0x82));
        }
        public static void Test_EOR()
        {
            EOR s = new EOR(unchecked((int)0xE2221082), c);
            c.regs.WriteWord(Registers.r2, 0x102);
            s.Execute();
            Debug.Assert(c.regs.ReadWord(Registers.r1) == (0x102 ^ 0x82));
        }
        public static void Test_MUL()
        {
            MUL s = new MUL(unchecked((int)0xE0020894), c);
            c.regs.WriteWord(Registers.r4, 0x102);
            c.regs.WriteWord(Registers.r8, 0x82);
            s.Execute();
            Debug.Assert(c.regs.ReadWord(Registers.r2) == (0x102 * 0x82));
        }
        public static void Test_Move()
        {
            regs = new memory(64);
            CPU c = new CPU(new memory(32), regs);


            int code = unchecked((int)3818922032);
           //Console.WriteLine(string.Format("TEST_INSTRUCTIONS-TESTMOVE: CODE = 0x{0:X8}", code));
            Instruction i = MakeInstuction.make(code, c,69);
            i.Execute();

            Debug.Assert(regs.ReadWord(8) == 48);
          //Console.WriteLine("TEST_INSTRUCTION_TESTMOVE: DECODE = " + i.Disassemble());
           

        }
        public static void Test_STR()
        {
           //Console.WriteLine("TESTINSTRUCTION:Testing STR");
           
            c.regs.WriteWord(Registers.r1, 0xdebeef);
            c.regs.WriteWord(Registers.r4, 100);
            ldr_str s = MakeInstuction.make(unchecked((int)0xE5A41100), c,69) as ldr_str;
           //Console.WriteLine(s.Disassemble());
            s.Execute();
            Debug.Assert(c.RAM.ReadWord(356) == 0xdebeef && c.regs.ReadWord(Registers.r4) == 356);
        }
        public static void Test_LDR()
        {
           //Console.WriteLine("TESTINSTRUCTION:Testing LDR");
            c.regs.WriteWord(Registers.r4, 200);
            c.RAM.WriteWord(456, 0xbeef);
            ldr_str s = new ldr_str(unchecked((int)0xE5B41100), c);
           
            s.Execute();
            Debug.Assert(c.regs.ReadWord(Registers.r1) == 0xbeef && c.regs.ReadWord(Registers.r4) == 456);
        }
        public static void Test_LDRB()
        {
           //Console.WriteLine("TESTINSTRUCTION:Testing LDR");
            c.regs.WriteWord(Registers.r4, 200);
            c.RAM.WriteWord(456, 0xef);
            ldr_str s = new ldr_str(unchecked((int)0xE5f41100), c);

            s.Execute();
            Debug.Assert(c.regs.ReadByte(Registers.r1) == 0xef && c.regs.ReadWord(Registers.r4) == 456);
        }
        public static void Test_STRB()
        {
           //Console.WriteLine("TESTINSTRUCTION:Testing STRB");

            c.regs.WriteByte(Registers.r1, 0xef);
            c.regs.WriteByte(Registers.r4, 101);
            ldr_str s = MakeInstuction.make(unchecked((int)0xE5E41100), c,69) as ldr_str;
           //Console.WriteLine(s.Disassemble());
            s.Execute();
            Debug.Assert(c.RAM.ReadByte(357) == 0xef && c.regs.ReadWord(Registers.r4) == 357);
        }

      
    }
}