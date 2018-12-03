using Prototype.Model;
using System;
using System.Diagnostics;

/// <summary>
/// file to hold branch classes that implement arm branch instructions
/// </summary>


namespace Prototype.Instructions
{
    /// <summary>
    /// jumps the program counter to a new location in code
    /// with optional jump back
    /// </summary>
    class Branch:Instruction
    {
        bool linking; //flag determines if we pass the pc into register
        int offset;
        int loc;
        /// <summary>
        /// initalizes Branch setting the linking flag and op2
        /// </summary>
        /// <param name="armcode"> the actual instruction</param>
        /// <param name="c"> a reference to the cpu class and thus a reference to RAM and Regs</param>
        public Branch(int armcode, CPU c, int loca) : base(armcode,c)
        {
            offset = (memory.testBit(code, 23) ? memory.ExtractBits_shifted(code, 0, 23) + 0x3f000000 : memory.ExtractBits_shifted(code, 0, 23)) << 2;
            loc = loca;
            linking = memory.testBit(armcode, 24);
            //initalize class
        }

        /// <summary>
        /// translate machine code into an arm assembly instruction
        /// </summary>
        /// <returns>assembly instruction</returns>
        public override string Disassemble()
        {
            //check operand two bytes
            //check if linking
            return string.Format("{0:X8} B{1}{2} 0x{3:x8}",code, linking?"l":"" ,base.Disassemble(), (loc+8+offset) );
        }

        /// <summary>
        /// changes the value in pc to move program execution
        /// </summary>
        /// <returns>true</returns>
        public override bool Execute()
        {
            if (!test_condition())
                return false;
            if (linking)
                cpu.regs.WriteWord(Registers.r14, cpu.pc-4);
            cpu.pc += offset;
            return true;
        }
    }


    class BX : Instruction
    {
        int offset;
        public BX(int code, CPU c) : base(code, c)
        {
            offset = c.regs.ReadWord(memory.ExtractBits_shifted(code, 0, 3) * 4) & unchecked((int)0xFFFFFFFE);
           //Console.WriteLine("BX: rm = " + memory.ExtractBits_shifted(code,0,3) + ", rm_value=" + c.regs.ReadWord(memory.ExtractBits_shifted(code, 0, 3) * 4));
        }

        public override string Disassemble()
        {
            return string.Format("{0:X8} BX{1} 0x{2:x8}", code, base.Disassemble(), offset);
        }

        public override bool Execute()
        {
            if (!test_condition())
                return false;

            cpu.pc = offset;
            return true;
        }
    }
}
