//// Classes to implement dataprocessing Arm instructions and (compare and multiply)

using System;
using Prototype.Model;
using System.Diagnostics;
namespace Prototype.Instructions
{
    /// <summary>
    /// Parent class for all instructions of type dataprocessing
    /// </summary>
    class DataProcessing : Instruction
    {
        public int Rn; //source reg
        public int Rd; //destination reg
        public bool s; //determines if instruction will update CPSR

        /// <summary>
        /// initalizes a data processing class
        /// </summary>
        /// <param name="armcode">the instruction macchine code</param>
        /// <param name="c">CPU reference</param>
        public DataProcessing(int armcode, CPU c) : base(armcode, c)
        {
            Rn = memory.ExtractBits_shifted(armcode, 16, 19);
            Rd = memory.ExtractBits_shifted(armcode, 12, 15);
           //Console.WriteLine("DATAPROCESSING: arm code = " + string.Format("0x{0:X8}",code));
            //determine what type of operand2 to create
            if (memory.testBit(armcode, 25))
            {
               //Console.WriteLine("DATAPROCESSING: OPERAND 2 is immediate shift");
                op2 = new immediate_shift(memory.ExtractBits_shifted(armcode, 0, 11));
            }
            else
            {
               //Console.WriteLine("DATAPROCESSING: OPERAND 2 is shift");
                op2 = new shift(memory.ExtractBits_shifted(armcode, 0, 11), cpu);
            }
            s = memory.testBit(code,20);
            op2.cflag = (s ? memory.ExtractBits_shifted(cpu.CPSR, flags.C, flags.C) : 2);
        }


        public void sbit()
        {
            if (Rd * 4 == 60)
            {

                cpu.CPSR = cpu.regs.ReadWord(Registers.SPSR);
                Registers.state = ((cpu.CPSR & 0x1F) == 0b11111 ? 0 : (cpu.CPSR & 0x1F) == 0b10011 ? 1 : 2);

            }
            else
            {
                int nzcf = cpu.CPSR;
                int rv = cpu.regs.ReadWord(Rd * 4);
                nzcf = memory.setbit(nzcf, flags.N, ((rv >> 31) & 1) == 1 );
                nzcf = memory.setbit(nzcf, flags.Z, rv == 0);
                nzcf = memory.setbit(nzcf, flags.C, op2.carryout == 1 );
                cpu.CPSR = nzcf;
            }
        
        }
        /// <summary>
        /// returns the basic string translation of the operand2 section of a instruction
        /// </summary>
        /// <returns> the operand2 as a string</returns>
        public override string Disassemble()
        {

            return base.Disassemble();
        }
    }
    /// <summary>
    /// class to move a value from a value into a register
    /// </summary>
    class Move:DataProcessing
    {
        /// <summary>
        /// initalize the class
        /// </summary>
        /// <param name="armcode">instruction machine code</param>
        /// <param name="c">CPU reference</param>
        public Move(int armcode, CPU c) : base(armcode, c)
        {
           //Console.WriteLine("MOVE: MADE A NEW MOVE INSTRUCTION"+(s ? " S is set":""));

        }

        /// <summary>
        /// Turns the machine code into assembly
        /// </summary>
        /// <returns>an arm assembly instruction</returns>
        public override string Disassemble()
        {
            return string.Format("{0:X8} MOV{1} r{2}", code,s?"S":"", Rd) + op2;
        }

        /// <summary>
        /// moves a value into register Rd
        /// </summary>
        /// <returns> true </returns>
        public override bool Execute()
        {
            if (test_condition())
            {
                //Console.WriteLine("MOVE: RD=" + Rd + " op2=" + string.Format("0x{0:X8}", op2.Compute()));
                cpu.regs.WriteWord(Rd * 4, op2.Compute());

                if (s)
                {
                    sbit();
                }
            }
            return true;
        }
    }
    /// <summary>
    /// Adds the value of rn to op2 and stores in rd
    /// </summary>
    class ADD : DataProcessing
    {
        public ADD(int armcode, CPU c) : base(armcode,c)
        {
           //Console.WriteLine("ADD: Created a new add instruction");
        }

        public override string Disassemble()
        {
            return string.Format("{0:X8} ADD{1} r{2}, r{3}", code,s?"S":"", Rd, Rn) + op2;
        }

        public override bool Execute()
        {
            int r = cpu.regs.ReadWord(Rn * 4);
            if (test_condition()) {

            cpu.regs.WriteWord(Rd * 4, cpu.regs.ReadWord(Rn * 4) + op2.Compute());
            if (s)
            {
                    
                    if (Rd * 4 == 60)
                    {
                        
                        cpu.CPSR = cpu.regs.ReadWord(Registers.SPSR);
                        Registers.state = ((cpu.CPSR & 0x1F) == 0b11111 ? 0 : (cpu.CPSR & 0x1F) == 0b10011 ? 1 : 2);

                    }
                    else
                    {
                        int nzcf = cpu.CPSR;
                        int rv = cpu.regs.ReadWord(Rd * 4);
                        
                        bool rbit = (((r >> 31) & 1) == 1);
                        bool opbit = (((op2.Compute() >> 31) & 1) == 1);
                        bool abit = ((((rv) >> 31) & 1) == 1);

                        //Console.WriteLine("ADDS: rbit="+rbit+" opbit="+opbit+" abit="+abit);

                        bool overflow = (rbit && opbit && !abit) || (!rbit && !opbit && abit);
                        bool carry = ((uint) rv > 0xffff && (uint)op2.Compute() >= 0xffff) || ((uint)rv >= 0xffff && (uint)op2.Compute() > 0xffff);

                        nzcf = memory.setbit(nzcf, flags.N, ((rv >> 31)&1) == 1);
                        nzcf = memory.setbit(nzcf, flags.Z, rv == 0);
                        nzcf = memory.setbit(nzcf, flags.C, carry);
                        nzcf = memory.setbit(nzcf, flags.V, overflow);
                        cpu.CPSR = nzcf;
                        //Console.WriteLine(string.Format("*****\nADDS: nzcf = {0:X8} rv={1}, op2={2}",nzcf,rv,op2.Compute()));
                    }
                    
                }
        }
            return true;
        }
    }
    /// <summary>
    /// Moves the inverses of op2 into rd
    /// </summary>
    class MVN : DataProcessing
    {
        public MVN(int code, CPU c) : base(code, c) { }

        public override string Disassemble()
        {
            return string.Format("{0:X8} MVN{1} r{2}", code,s?"S":"", Rd) + op2;
        }

        public override bool Execute()
        {
            if (test_condition())
            {
                cpu.regs.WriteWord(Rd * 4, ~op2.Compute());
                if (s)
                {
                    sbit();
                }
            }
            return true;
        }

    }
    /// <summary>
    /// subtracts the value of op2 from rn and store it in rd 
    /// </summary>
    class SUB : DataProcessing
    {
        public SUB(int code, CPU c) : base(code, c) { }

        public override string Disassemble()
        {
            return string.Format("{0:X8} SUB{1} r{2}, r{3}, ", code, s?"S":"", Rd, Rn) + op2;
        }

        public override bool Execute()
        {
            int r = cpu.regs.ReadWord(Rn * 4);
            if (test_condition()) { 
            cpu.regs.WriteWord(Rd * 4, cpu.regs.ReadWord(Rn * 4) - op2.Compute());
                if (s)
                {

                    if (Rd * 4 == 60)
                    {

                        cpu.CPSR = cpu.regs.ReadWord(Registers.SPSR);
                        Registers.state = ((cpu.CPSR & 0x1F) == 0b11111 ? 0 : (cpu.CPSR & 0x1F) == 0b10011 ? 1 : 2);

                    }
                    else
                    {
                        int nzcf = cpu.CPSR;
                        int rv = cpu.regs.ReadWord(Rd * 4);

                        bool rbit = (((r >> 31) & 1) == 1);
                        bool opbit = (((op2.Compute() >> 31) & 1) == 1);
                        bool abit = ((((rv) >> 31) & 1) == 1);



                        bool overflow = (rbit && !opbit && !abit) || (!rbit && opbit && abit);
                        bool carry = (uint)op2.Compute() > (uint)r;

                        nzcf = memory.setbit(nzcf, flags.N, ((rv >> 31)&1) == 1 );
                        nzcf = memory.setbit(nzcf, flags.Z, rv == 0);
                        nzcf = memory.setbit(nzcf, flags.C, carry);
                        nzcf = memory.setbit(nzcf, flags.V, overflow);
                        cpu.CPSR = nzcf;
                        //Console.WriteLine(string.Format("*****\nADDS: nzcf = {0:X8} rv={1}, op2={2}", nzcf, rv, op2.Compute()));
                    }

                }
            }
            return true;
        }
    }
    /// <summary>
    /// subtracts the value of rn from op2 and stores it in rd
    /// </summary>
    class RSB : DataProcessing
    {
        public RSB(int code, CPU c) : base(code, c) { }
        public override string Disassemble()
        {
            return string.Format("{0:X8} RSB{1} r{2}, r{3}", code, s ? "S" : "", Rd, Rn) + op2;
        }
        public override bool Execute()
        {
            int r = cpu.regs.ReadWord(Rn * 4);
            if (test_condition())
            {
                cpu.regs.WriteWord(Rd * 4, op2.Compute() - cpu.regs.ReadWord(Rn * 4));
                if (s)
                {

                    if (Rd * 4 == 60)
                    {

                        cpu.CPSR = cpu.regs.ReadWord(Registers.SPSR);
                        Registers.state = ((cpu.CPSR & 0x1F) == 0b11111 ? 0 : (cpu.CPSR & 0x1F) == 0b10011 ? 1 : 2);

                    }
                    else
                    {
                        int nzcf = cpu.CPSR;
                        int rv = cpu.regs.ReadWord(Rd * 4);

                        bool rbit = (((r >> 31) & 1) == 1);
                        bool opbit = (((op2.Compute() >> 31) & 1) == 1);
                        bool abit = ((((rv) >> 31) & 1) == 1);



                        bool overflow = (!rbit && opbit && !abit) || (rbit && !opbit && abit);
                        bool carry = (uint)r > (uint)op2.Compute();

                        nzcf = memory.setbit(nzcf, flags.N, ((rv >> 31)&1) == 1);
                        nzcf = memory.setbit(nzcf, flags.Z, rv == 0);
                        nzcf = memory.setbit(nzcf, flags.C, carry);
                        nzcf = memory.setbit(nzcf, flags.V, overflow);
                        cpu.CPSR = nzcf;
                        //Console.WriteLine(string.Format("*****\nADDS: nzcf = {0:X8} rv={1}, op2={2}", nzcf, rv, op2.Compute()));
                    }

                }
            }
            return true;
        }
    }
    /// <summary>
    /// performs bitwise and on rn and op2 storing the result in rd
    /// </summary>
    class AND : DataProcessing
    {
        public AND(int code, CPU c) : base(code, c) { }
        public override string Disassemble()
        {
            return string.Format("{0:X8} AND{1} r{2}, r{3}", code, s ? "S" : "", Rd, Rn) + op2;
        }
        public override bool Execute()
        {
            if (test_condition())
            {
                cpu.regs.WriteWord(Rd * 4, cpu.regs.ReadWord(Rn * 4) & op2.Compute());
                if (s)
                {
                    sbit();
                }
            }
                return true;
        }
    }
    /// <summary>
    /// Performs bitwise or on rn and op2 storing the result in rd
    /// </summary>
    class ORR : DataProcessing
    {
        public ORR(int code, CPU c) : base(code, c) { }
        public override string Disassemble()
        {
            return string.Format("{0:X8} ORR{1} r{2}, r{3}", code, s ? "S" : "", Rd, Rn) + op2;
        }
        public override bool Execute()
        {
            if (test_condition())
            {
                cpu.regs.WriteWord(Rd * 4, cpu.regs.ReadWord(Rn * 4) | op2.Compute());
                if (s)
                {
                    sbit();
                }
            }
            return true;
        }
    }
    /// <summary>
    /// Performs bitwise EOR on rn and op2 storing result in rd
    /// </summary>
    class EOR : DataProcessing
    {
        public EOR(int code, CPU c) : base(code, c) { }
        public override string Disassemble()
        {
            return string.Format("{0:X8} EOR{1} r{2}, r{3}", code, s ? "S" : "", Rd, Rn) + op2;
        }
        public override bool Execute()
        {
            if (test_condition())
            {
                cpu.regs.WriteWord(Rd * 4, cpu.regs.ReadWord(Rn * 4) ^ op2.Compute());
                if (s)
                {
                    sbit();
                }
            }
            return true;
        }
    }
    /// <summary>
    /// performs bitwise and on rn and bitwise not op2 storing result in rd
    /// </summary>
    class BIC : DataProcessing
    {
        public BIC(int code, CPU c) : base(code, c) { }
        public override string Disassemble()
        {
            return string.Format("{0:X8} BIC{1} r{2}, r{3}", code, s ? "S" : "", Rd, Rn) + op2;
        }
        public override bool Execute()
        {
            if (test_condition())
            {
                cpu.regs.WriteWord(Rd * 4, cpu.regs.ReadWord(Rn * 4) & (~op2.Compute()));
                if (s)
                {
                    sbit();
                }
            }
            return true;
        }
    }
    /// <summary>
    /// multiplies RS and RM storing the result in rd 
    /// </summary>
    class MUL : DataProcessing
    {
        int Rs;
        int Rm;

        public MUL(int armcode, CPU c) : base(armcode, c)
        {
            Rd = memory.ExtractBits_shifted(armcode, 16, 19);
            Rm = memory.ExtractBits_shifted(armcode, 0,3);
            Rs = memory.ExtractBits_shifted(armcode, 8,11);
        }

        public override string Disassemble()
        {
            return string.Format("{0:X8} MUL{1} r{2}, r{3}, r{4}", code, s ? "S" : "", Rd, Rm, Rs);
        }

        public override bool Execute()
        {
            if (test_condition())
            {
                cpu.regs.WriteWord(Rd * 4, cpu.regs.ReadWord(Rs * 4) * cpu.regs.ReadWord(Rm * 4));
                if (s)
                {
                    int nzcf = cpu.CPSR;
                    int rv = cpu.regs.ReadWord(Rd * 4);
                    nzcf = memory.setbit(nzcf, flags.N, ((rv >> 31)&1) == 1);
                    nzcf = memory.setbit(nzcf, flags.Z, rv == 0);
                    
                    cpu.CPSR = nzcf;
                }
            }
            return true;
        }
    }

    class CMP : DataProcessing
    {
        public CMP(int code, CPU c) : base(code, c) { }
        public override string Disassemble()
        {
            return string.Format("{0:X8} CMP r{1}", code, Rn) + op2;
        }
        public override bool Execute()
        {
            if (test_condition())
            {
                bool c,n,v,z;
                int rn_v, op;
               
                op = op2.Compute();
                rn_v = cpu.regs.ReadWord(Rn*4);
           
                int result = rn_v - op;
               //Console.WriteLine("DATAPROCSESSING: CMP, reg = r" + Rn + ", op2 = " + op2);
               //Console.WriteLine("DATAPROCSESSING: CMP, RESUALT= "+rn_v+" - "+op+" = " + result );

                if ((uint)op <= (uint)rn_v)
                {
                    
                    c = true;
                }
                else
                    c = false;
               //Console.WriteLine("DATAPROCESSING: CMP C=" + c.ToString());


                if (rn_v >= 0 && op < 0 && result < 0 || rn_v < 0 && op >= 0 && result >= 0)
                    v = true;
                else
                    v = false;
               //Console.WriteLine("DATAPROCESSING: CMP V=" + v.ToString());
                if (result >= 0)
                    n = false;
                else
                    n = true;
               //Console.WriteLine("DATAPROCESSING: CMP N=" + n.ToString());
                if (result == 0)
                    z = true;
                else
                    z = false;
               //Console.WriteLine("DATAPROCESSING: CMP Z=" + z.ToString());
                int flag = cpu.CPSR;
                flag = memory.setbit(flag, flags.N, n);

               //Console.WriteLine("DATAPROCESSING: CMP CPSR=" + flag);
                flag = memory.setbit(flag, flags.Z, z);
                
               //Console.WriteLine("DATAPROCESSING: CMP CPSR=" + flag);
                flag = memory.setbit(flag, flags.C, c);
               
               //Console.WriteLine("DATAPROCESSING: CMP CPSR=" + flag);
                cpu.CPSR = memory.setbit(flag, flags.V, v);
               //Console.WriteLine("DATAPROCESSING: CMP CPSR=" + flag);


            }
            return true;
        }
    }
}
