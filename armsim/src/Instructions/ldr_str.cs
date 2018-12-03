using Prototype.Model;
using System.Diagnostics;
using System;
/// <summary>
/// Classes to implement arm store and load
/// </summary>
namespace Prototype.Instructions
{
    /// <summary>
    /// arm store and load
    /// accesses Ram to load/store information
    /// </summary>
    class ldr_str :Instruction
    {
        public bool p; //pre or post index
        public bool u; //positve offset (1) or negative (0)
        bool b; //lrd/str a byte(1) or a word(0)
        public bool w; //update reg after completion(1) only when pre-indexing
        public bool l; //load(1) store(0)
        public int Rn; //base reg
        public int rn_val; //holds the value stored in rn
        int rd_val; //holds the value stored in rd;
        int Rd; // source/destination reg
        int op2_v; // holds the value calculated in op2
        /// <summary>
        /// initalizes class
        /// </summary>
        /// <param name="armcode">machine code instruction</param>
        /// <param name="c">CPU reference</param>
        public ldr_str(int armcode, CPU c) : base(armcode,c)
        {
            p = memory.testBit(armcode, 24);
            //u = memory.testBit(armcode, 23);
            b = memory.testBit(armcode, 22);
            w = memory.testBit(armcode, 21);
            l = memory.testBit(armcode, 20);
            Rn = memory.ExtractBits_shifted(armcode, 16, 19);
            Rd = memory.ExtractBits_shifted(armcode, 12, 15);
            rd_val = cpu.regs.ReadWord(Rd * 4);
            rn_val = cpu.regs.ReadWord(Rn * 4);
            
            u = memory.testBit(armcode, 23);


            if (memory.testBit(armcode, 25))
            {
                op2 = new shift(memory.ExtractBits_shifted(armcode, 0, 11), cpu);
            }
            else
                op2 = new Operand2(memory.ExtractBits_shifted(armcode, 0, 11));

            op2_v = (u ? op2.Compute() : 0-op2.Compute());
           //Console.WriteLine("LOAD/STORE: COMPLETED CREATION");
        }

        /// <summary>
        /// translates machine code to arm assembly
        /// </summary>
        /// <returns>arm instruction</returns>
        public override string Disassemble()
        {
            //check if load or store
            //check operand2 type
            //string d = "";

            
           // d = string.Format("0x{0:X8} {1}{2} r{3}, [r{4}{5}]",code,  l ? "ldr":"str",b ? "b":"", Rd, Rn, op2);
           //Console.WriteLine("LOAD/STORE: op2="+op2);
            string s = string.Format("0x{0:X8} {1}{2}{3} r{4}, [r{5}", code, l ? "ldr" : "str", b ? "b" : "", base.Disassemble(),Rd, Rn);
            if (u)
            {
                //string.Format("0x{0:X8} {1}{2} r{3}, [r{4}{5}]", code, l ? "ldr" : "str", b ? "b" : "", Rd, Rn, (op2_v == 0) ? "" : op2.ToString());
                return string.Format("{0}{1}]{2}", s, (op2_v == 0) ? "" : op2.ToString(), w ? "!" : "");
            }
            else
            {
                return string.Format("{0}, #-{1}]{2}", s, op2.Compute(),w?"!":"");
            }
        }

        /// <summary>
        /// stores values in memory 
        /// </summary>
        /// <returns>true</returns>
        /// 
        void store()
        {
            int addr = p ? rn_val + op2_v : rn_val;
            if(b)
                
                cpu.RAM.WriteByte(addr, (byte)(rd_val & 0xFF));
            else
                cpu.RAM.WriteWord(addr, rd_val);
        }

        /// <summary>
        /// Loads values from memory 
        /// </summary>
        /// <returns>true</returns>
        /// 
        void load()
        {
           //Console.WriteLine("LOAD: started LOADING, Rn=" + Rn * 4 + ", rd value="+rn_val+", op2="+op2_v+", u="+u);
            int addr = p ? rn_val + op2_v : rn_val;
            if (b)
                cpu.regs.WriteWord(Rd*4, (int)cpu.RAM.ReadByte(addr));
            else
                cpu.regs.WriteWord(Rd * 4, cpu.RAM.ReadWord(addr));
           //Console.WriteLine("LOAD: COMPLETED LOADING into RD="+Rd*4);
        }

        public override bool Execute()
        {
            if (!test_condition())
                return true;

            if (l)
                load();
            else
                store();
            if(w)
                cpu.regs.WriteWord(Rn*4, rn_val+op2_v); //write back to reg

            return true;
        }

    }

    /// <summary>
    /// Class to load/store multiple regs from/in memory
    /// Affective address must be in Rd
    /// </summary>
    class ldmfd_stmfd : ldr_str
    {
        public ldmfd_stmfd(int code, CPU c) : base(code, c) { }

        public override string Disassemble()
        {
            string de = string.Format("0x{0:X8} {1} r{2}{3}, ", code, l ? "ldmfd" : "stmfd",Rn, w ? "!" : "");

            de += "{";
            for (int i = 0; i < 16; i++)
            {
                if (memory.testBit(code, i))
                    de += "r" + i + " ,";
            }
            de =de.Remove(de.Length-2);
            
            return de + "}";
        }

        /// <summary>
        /// Loads into a list of regs values pointed to by Rd, post incrememting
        /// </summary>
        void LDMIA()
        {
            for (int i = 0; i < 16; i++)
            {
                if (memory.testBit(code, i))
                {
                   //Console.WriteLine("LDMFD_STMFD: LDMIA index=" + i +", rnvalue="+rn_val+ ", rn="+Rn);
                    cpu.regs.WriteWord(i * 4, cpu.RAM.ReadWord(rn_val));
                    rn_val += 4;
                }

            }
            
        }
        /// <summary>
        /// Stores values form a list of regs into memory starting at the location given by rd
        /// pre-decrement
        /// </summary>
        void STMDB()
        {
            for (int i = 15; i > -1; i--)
            {
                if (memory.testBit(code, i))
                {
                   //Console.WriteLine("LDMFD_STMFD: STMFD index="+i);
                    rn_val -= 4;
                    cpu.RAM.WriteWord(rn_val, cpu.regs.ReadWord(i * 4));
                }
            }
            
        }

        public override bool Execute()
        {
            if (!test_condition())
                return true;
            if (l)
                LDMIA();
            else
                STMDB();
            if (w)
                cpu.regs.WriteWord(Rn * 4, rn_val);

            return true;
        }
    }
}
