using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prototype.Model;
using System.Diagnostics;

/// <summary>
/// class to implement creation of arm instructions and special arm instructions (swi, mrs, msr)
/// </summary>

namespace Prototype.Instructions
{
    /// <summary>
    /// Creates instances of instruction classes
    /// </summary>
    public static class MakeInstuction
    {
        /// <summary>
        /// creates an instruction class
        /// </summary>
        /// <param name="code">an instruction in numaric form</param>
        /// <param name="c">reference to cpu(needed for instructions to execute)</param>
        /// <returns>an instance of an instruction child class</returns>
        public static Instruction make(int code, CPU c,int addr)
        {
           //Console.WriteLine(string.Format("MAKE_INSTRUCTION: bits 25-27 = 0b{0} and bits 21-24 = 0b{1}",  Convert.ToString(memory.ExtractBits_shifted(code, 25, 27),2), Convert.ToString(memory.ExtractBits_shifted(code, 21, 24),2)));
            
            //determine what class of instruction
            switch (memory.ExtractBits_shifted(code, 25, 27))
            {
                //most likely a dataprocessing class
                case 1:
                case 0:
                    //determine the dataprocess arm opcode
                    switch (memory.ExtractBits_shifted(code, 21, 24))
                    {
                        case 0b0000:

                            if (memory.ExtractBits_shifted(code, 25, 27) == 0 && memory.ExtractBits_shifted(code, 4, 7) == 0b1001)
                            {
                               //Console.WriteLine("INSTRUCTION: making new MUL instruction");
                                return new MUL(code, c);
                            }

                           //Console.WriteLine("INSTRUCTION: making new AND instruction");
                            return new AND(code, c);

                        case 0b0001:
                           //Console.WriteLine("INSTRUCTION: making new EOR instruction");
                            return new EOR(code, c);

                        case 0b0010:
                           //Console.WriteLine("INSTRUCTION: making new SUB instruction");
                            return new SUB(code, c);

                        case 0b0011:
                           //Console.WriteLine("INSTRUCTION: making new RSB instruction");
                            return new RSB(code, c);

                        case 0b0100:
                           //Console.WriteLine("INSTRUCTION: making new ADD instruction");
                            return new ADD(code, c);

                        case 0b0101:
                            
                        case 0b0110:

                        case 0b0111:

                        case 0b1000:
                            
                        case 0b1010:
                            if (memory.ExtractBits_shifted(code, 20, 27) == 0b10000 || memory.ExtractBits_shifted(code, 20, 27) == 0b10100)
                                return new MRS(code,c);
                           //Console.WriteLine("INSTRUCTION: making new CMP instruction");
                            return new CMP(code, c);
                        case 0b1011:
                        case 0b1001:
                            if (memory.ExtractBits_shifted(code, 20, 27) == 0b00010010 && memory.ExtractBits_shifted(code, 4, 7) == 0b0001)
                            {
                               //Console.WriteLine("INSTRUCTION: making new BX instruction");
                                return new BX(code, c);
                            }
                            if ((memory.ExtractBits_shifted(code, 23, 27) == 0b00110 || memory.ExtractBits_shifted(code, 23, 27) == 0b00010) && (memory.ExtractBits_shifted(code, 20, 22) == 0b110 || memory.ExtractBits_shifted(code, 20, 22) == 0b10))
                            {
                               //Console.WriteLine("INSTRUCTION: making new MSR instruction");
                                return new MSR(code, c);
                            }
                                                   
                            break;

                        case 0b1100:
                           //Console.WriteLine("INSTRUCTION: making new ORR instruction");
                            return new ORR(code, c);

                        case 0b1101:
                           //Console.WriteLine("INSTRUCTION: making new MOVE instruction");
                            return new Move(code,c);

                        case 0b1110:
                           //Console.WriteLine("INSTRUCTION: making new BIC instruction");
                            return new BIC(code, c);

                        case 0xF:
                           //Console.WriteLine("INSTRUCTION: making new MVN instruction");
                            return new MVN(code, c);

                        default:
                            break;
                    } break;
                case 0b010:
                case 0b011:
                   //Console.WriteLine("INSTRUCTION: Making new ldr_str instruction");
                    return new ldr_str(code, c);
                case 0b111:
                    if (memory.testBit(code, 24))
                    {
                       //Console.WriteLine("INSTRUCTION: Making new SWI instruction");
                        return new SWI(code, c);
                    }
                    break;
                case 0b100:
                    if ((memory.ExtractBits_shifted(code, 23, 24) == 0b01 && memory.testBit(code, 20)) | (memory.ExtractBits_shifted(code, 23, 24) == 0b10 && !memory.testBit(code, 20)))
                    {
                       //Console.WriteLine("INSTRUCTION: Making new ldmfd_stmfd instruction");
                        return new ldmfd_stmfd(code, c);
                    }
                    break;
                case 0b101:
                   //Console.WriteLine("INSTRUCTION: making new BRANCH instruction");
                    return new Branch(code, c,addr);
                default:
                    break;
               
            }

            return new Instruction(code, c);
        }
    }

    /// <summary>
    /// class to execute an arm instruction
    /// </summary>
    public class Instruction
    {
        public int cond; //condition code for the instruction
        public int code; //the actual numaric instruction
        public CPU cpu; //reference to mem, regs, and CPSR;
        public Operand2 op2; //reference to a operand2 instance used to compute a value

        /// <summary>
        /// initalizes an instruction and sets the variables
        /// </summary>
        /// <param name="armcode"> the machine code instruction</param>
        /// <param name="cpu_r">a reference to the cpu</param>
        public Instruction(int armcode, CPU cpu_r)
        {
            cpu = cpu_r;
            code = armcode;
            cond = memory.ExtractBits_shifted((int)armcode, 28,31);
            
        }

        public bool test_condition()
        {
            
            if(cond < 2)
                return cond == 0 ? memory.testBit(cpu.CPSR, flags.Z): !memory.testBit(cpu.CPSR, flags.Z);
            if(cond < 4)
                return cond == 2 ? memory.testBit(cpu.CPSR, flags.C) : !memory.testBit(cpu.CPSR, flags.C);
            if(cond < 6)
                return cond == 4 ? memory.testBit(cpu.CPSR, flags.N) : !memory.testBit(cpu.CPSR, flags.N);
            if (cond < 8)
                return cond == 6 ? memory.testBit(cpu.CPSR, flags.V) : !memory.testBit(cpu.CPSR, flags.V);
            if(cond < 10)
                return cond == 8 ? memory.testBit(cpu.CPSR, flags.C) && !memory.testBit(cpu.CPSR, flags.Z) : !memory.testBit(cpu.CPSR, flags.C) || memory.testBit(cpu.CPSR, flags.Z);
            if (cond == 10)
                return memory.testBit(cpu.CPSR, flags.N) == memory.testBit(cpu.CPSR, flags.V);
            if(cond == 11)
                return memory.testBit(cpu.CPSR, flags.N) != memory.testBit(cpu.CPSR, flags.V);
            if(cond == 12)
                return (memory.testBit(cpu.CPSR, flags.N) == memory.testBit(cpu.CPSR, flags.V)) && !memory.testBit(cpu.CPSR, flags.Z);
            if(cond == 13)
                return (memory.testBit(cpu.CPSR, flags.N) != memory.testBit(cpu.CPSR, flags.V)) || memory.testBit(cpu.CPSR, flags.Z);
           

            return true;
        }

        public string cond_s()
        {
            return Disassemble();
        }

        //returns the instruction translated into a assembly string
        public virtual string Disassemble()
        {
            //translate the instrution into assembly
            switch (cond)
            {
                case 0:
                    return "EQ";
                case 1:
                    return "NE";
                case 2:
                    return "CS";
                case 3:
                    return "CC";
                case 4:
                    return "MI";
                case 5:
                    return "PL";
                case 6:
                    return "VS";
                case 7:
                    return "VC";
                case 8:
                    return "HI";
                case 9:
                    return "LS";
                case 10:
                    return "GE";
                case 11:
                    return "LT";
                case 12:
                    return "GT";
                case 13:
                    return "LE";
                case 14:
                    return "AL";
            }
            return "!Not implemetned";
        }
        //executes the function
        public virtual bool Execute()
        {
            //execute the instruction
           
            return true;
        }

    }

    /// <summary>
    /// triggers exceptions
    /// </summary>
    class SWI : Instruction
    {
        int value;
        public SWI(int code, CPU c) : base(code, c) { value = memory.ExtractBits_shifted(code, 0, 23); }
        public override string Disassemble()
        {
            return string.Format("0x{0:X8} SWI{1} #{2}",code,base.Disassemble(),value);
        }

        public void Putchar() { }
        public void Readline() { }


        public override bool Execute()
        {
            if (test_condition())
            {
                if (value == 0x11)
                    return false;
                int temp = cpu.CPSR;
                cpu.change_mode(1);
                cpu.regs.WriteWord(Registers.r14, cpu.pc - 4);
                cpu.CPSR = memory.setbit(cpu.CPSR, 7, true);
                cpu.pc = 8;
                cpu.regs.WriteWord(Registers.SPSR, temp);
                
            }

            return true;
           
          
        }
    }
    //loads a register into either cpsr or spsr
    public class MSR : Instruction
    {
        Operand2 operand;
        int Rm;
        int byte_mask;
        int UnallocMask = 0x0FFFFF20;
        int UserMask = unchecked((int)0xF0000000);
        int PrivMask = 0xF;
        int StateMask = 0;
        int mask;
        public MSR(int code, CPU c) : base(code, c)
        {
            //int fieldmask = memory.ExtractBits_shifted(code, 16, 19);
            if (memory.testBit(code, 25))
                Rm = (new immediate_shift(code)).Compute();
            else
                Rm = cpu.regs.ReadWord(memory.ExtractBits_shifted(code, 0, 3)*4);
            byte_mask =unchecked((int)( (memory.testBit(code, 16) ? 0xFF : 0) |
                                        (memory.testBit(code, 17) ? 0xFF00 : 0) |
                                        (memory.testBit(code, 18) ? 0xFF0000 : 0) |
                                        (memory.testBit(code, 19) ? 0xFF000000 : 0)));
            
        }
        public override string Disassemble()
        {
            return string.Format("{0:X8} MSR{1} {2}{3} {4}",code,base.Disassemble(), memory.testBit(code,20) ? "SPSR" : "CPSR",memory.testBit(code,25) ? "" : ",", memory.testBit(code, 25) ? new immediate_shift(code): op2);
        }
        public override bool Execute()
        {
            if (test_condition())
            {
                if (memory.testBit(code, 22))
                {
                    mask = unchecked((int)(byte_mask & (UserMask | PrivMask | StateMask)));
                    cpu.regs.WriteWord(Registers.SPSR, (cpu.regs.ReadWord(Registers.SPSR) & ~mask) | (Rm & mask));
                }
                else
                {
                    if (Registers.state > 0)
                    {
                        if ((Rm & StateMask) != 0)
                        {
                            //Console.WriteLine("MSR: UNPREDICTABLE");
                        }
                        else
                            mask = byte_mask & (UserMask | PrivMask);
                    }
                    else
                    {
                        mask = byte_mask & UserMask;
                    }
                    //cpu.CPSR = (cpu.CPSR & ~mask) | (Rm & mask);
                    cpu.CPSR = (cpu.CPSR & ~byte_mask) | (Rm & byte_mask);

                    switch (memory.ExtractBits_shifted(cpu.CPSR, 0, 4))
                    {
                        case 0b10011:
                            cpu.change_mode(1);
                            break;
                        case 0x1F:
                            cpu.change_mode(0);
                            break;
                        case 0b10010:
                            cpu.change_mode(2);
                            break;
                    
                    }
                }
            }
            return true;
        }
    }
    //loads cpsr or spsr into a register with a bit mask
    public class MRS : Instruction
    {
        int rd;
        
        public MRS(int code, CPU c) : base(code, c)
        {
            rd = memory.ExtractBits_shifted(code, 12, 15) * 4;
        }
        public override string Disassemble()
        {
            return string.Format("{0:X8} MRS{1} {2}, {3}", code, base.Disassemble(), memory.ExtractBits_shifted(code, 12, 15), memory.testBit(code, 20) ? "SPSR" : "CPSR" );

        }
        public override bool Execute()
        {
            if (test_condition())
            {
                if (memory.testBit(code, 22))
                    cpu.regs.WriteWord(rd, cpu.regs.ReadWord(Registers.SPSR));
                else
                    cpu.regs.WriteWord(rd, cpu.CPSR);
            }

            return true;
            
        }
    }
}

