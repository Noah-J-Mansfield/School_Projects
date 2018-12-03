using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prototype.Model;
using System.Diagnostics;
/// <summary>
/// classes to implement arm operand2 behavior
/// </summary>
namespace Prototype.Instructions
{

    /// <summary>
    /// class to handle getting operand2 value
    /// </summary>
    public class Operand2
    {
        public int code; //the 12 bit operand
        public int carryout;
        public int cflag = 2;
        public Operand2(int c)
        {
            code = c;
        }

        
        /// <summary>
        /// performs all steps to fine the value of operand
        /// </summary>
        /// <returns>value of operand2</returns>
        public virtual int Compute()
        {
            //shift bits
            return code;
        }

        /// <summary>
        /// shifts word to the left
        /// </summary>
        /// <param name="word">word to shift</param>
        /// <param name="amount">amount to shift the bits</param>
        /// <returns>shifted word</returns>
        public int Lsl(int word, int amount) { return word << amount; }

        /// <summary>
        /// shifts word to the right. new significant bit will always be zero
        /// </summary>
        /// <param name="word">word to shift</param>
        /// <param name="amount">amount to shift the bits</param>
        /// <returns>shifted word</returns>
        public int Asr(int word, int amount)
        {
            return word >> amount;
        }

        /// <summary>
        /// shifts word to the right. new significant bit will be either 1 if word negitive and 0 if word positive
        /// </summary>
        /// <param name="word">word to shift</param>
        /// <param name="amount">amount to shift the bits</param>
        /// <returns>shifted word</returns>
        public int Lsr(int word, int amount)
        {
            uint value = (uint)word;
            value = value >> amount;
            word = unchecked((int)(value));
            return word;
        }

        /// <summary>
        /// shifts word to the right and wrap around the word
        /// </summary>
        /// <param name="word">word to shift</param>
        /// <param name="amount">amount to shift the bits. will be multiplied by 2</param>
        /// <returns>shifted word</returns>
        public int Ror(int word, int amount)
        {
            uint value = (uint)word;
            
           //Console.WriteLine("OPERAND2: ROR: amount="+amount);
            while (amount != 0)
            {
                if ((value & 0x1) == 1)
                    value = (value >> 1) | 0x80000000;
                else
                    value = value >> 1;
                amount--;
               
            }
           //Console.WriteLine(String.Format("OPERAND2: ROR: word = 0x{0:X8}, value = 0x{1:X8}",word, value));
            return unchecked((int)value);
        }
        public override string ToString()
        {
            return ", #" + code;
        }

    }


    /// <summary>
    /// handles regs shifted by immediate or reg 
    /// </summary>
    public class shift : Operand2
    {

        int shift_amount;
        int reg;
        int shift_type;
        bool shiftby; //false = reg, true = immd
        CPU cpu;
        /// <summary>
        /// initalizes shift class
        /// </summary>
        /// <param name="c">machine code</param>
        public shift(int c, CPU cp) : base(c)
        {
            cpu = cp;
            if (memory.testBit(code, 4))
            {
                //shifted by a reg
                shiftby = false;
                shift_amount = cpu.regs.ReadWord(memory.ExtractBits_shifted(code,8,11)*4);
               //Console.WriteLine("SHIFT: shift reg = " + memory.ExtractBits_shifted(code, 8, 11) + ", shift amount = " + shift_amount);
            }
            else
            {
                //shifted by a imm
                shiftby = true;
                shift_amount = memory.ExtractBits_shifted(code, 7,11);
               //Console.WriteLine("SHIFT: shift amount = " + shift_amount);
            }
            reg = cpu.regs.ReadWord(memory.ExtractBits_shifted(code, 0, 3)*4);
            shift_type = memory.ExtractBits_shifted(code, 5, 6);
           //Console.WriteLine("SHIFT: reg = " + reg);


        }

        public override string ToString()
        {
            string de = ", ";
            de += "r" + memory.ExtractBits_shifted(code, 0, 3);
            if (memory.ExtractBits_shifted(code, 7, 11) != 0)
            {
                switch (memory.ExtractBits_shifted(code, 5, 6))
                {
                    case 0b00:
                        de += ", lsl";
                        break;
                    case 0b01:
                        de += ", lsr";
                        break;
                    case 0b10:
                        de += ", asr";
                        break;
                    case 0b11:
                        de += ", ror";
                        break;
                    default:
                        break;
                }
                if (memory.testBit(code, 4))
                    return de + " r" + memory.ExtractBits_shifted(code, 8, 11);
                else
                    return de + " #" + memory.ExtractBits_shifted(code, 7, 11);
            }
            return de;
        }

        /// <summary>
        /// shfits the reg 
        /// </summary>
        /// <returns>shifted reg</returns>
        public override int Compute()
        {
            if (cflag < 2 && shift_amount == 0)
                carryout = cflag;
            //check which shifter to use
            
            switch (shift_type)
            {
                case 0:
                   //Console.WriteLine("SHIFT: shift type is lsl");
                   
                    if (cflag < 2)
                    {
                        if (shift_amount != 0 && shiftby)
                        {
                            carryout = memory.testBit(reg, (32 - shift_amount)) ? 1 : 0;
                        }
                        if (!shiftby) {
                            int i = memory.ExtractBits_shifted(shift_amount, 0, 7);
                            carryout = i == 0 ? cflag :
                                i < 32 ? (memory.testBit(reg, (32 - i)) ? 1 : 0 ): 
                                i==32 ? reg&1 : 0;
                        }
                    }
                    return Lsl(reg, shift_amount); ;
                case 1:
                   //Console.WriteLine("SHIFT: shift type is lsr");
                    
                    if (cflag < 2)
                    {
                        if (shiftby && shift_amount == 0) { carryout = (memory.testBit(reg, (31)) ? 1 : 0); }
                        if (shift_amount > 0 && shiftby)
                        {
                            carryout = memory.testBit(reg, (shift_amount-1)) ? 1 : 0;
                        }
                        if (!shiftby)
                        {
                            int i = memory.ExtractBits_shifted(shift_amount, 0, 7);
                            carryout = i == 0 ? cflag :
                                i < 32 ? (memory.testBit(reg, (i-1)) ? 1 : 0) :
                                i == 32 ? (memory.testBit(reg, (31)) ? 1 : 0) : 0;
                        }
                    }
                    return Lsr(reg, shift_amount);

                case 2:
                   //Console.WriteLine("SHIFT: shift type is asr");
                    
                    if (cflag < 2)
                    {
                        if (shiftby && shift_amount == 0)
                        {
                            carryout = memory.testBit(reg, 31) ? 1 : 0;
                           
                        }
                        if (shiftby) { carryout = memory.testBit(reg, shift_amount-1) ? 1 : 0; }
                        if (!shiftby)
                        {
                            int i = memory.ExtractBits_shifted(shift_amount, 0, 7);
                            carryout = i == 0 ? cflag :
                                i < 32 ? (memory.testBit(reg, (i - 1)) ? 1 : 0) : (memory.testBit(reg, (31)) ? 1 : 0);
                             
                        }
                    }
                    return Asr(reg, shift_amount);
                case 3:
                    
                    if (cflag < 2)
                    {
                        if (shiftby && shift_amount > 0)
                        {
                            carryout = (memory.testBit(reg, (shift_amount - 1)) ? 1 : 0);
                        }
                        else if(!shiftby)
                        {
                            int i = memory.ExtractBits_shifted(shift_amount, 0, 7);
                            carryout = i == 0 ? cflag :
                                (i & 0x1F) == 0 ? (memory.testBit(reg, 31) ? 1 : 0) : (memory.testBit(reg, (i & 0x1F) - 1) ? 1 : 0);
                        }
                    }
                   //Console.WriteLine("SHIFT: shift type is ror");
                    return Ror(reg, shift_amount);
            }
           //Console.WriteLine("SHIFT: SHIFT TYPE NOT IMPLEMENTED");
            return code;
        }
    }
    public class immediate_shift : Operand2
    {
        int shiftAmount; //amount to shift
        int im; //8 bit number to shift

        /// <summary>
        /// initalize the class
        /// </summary>
        /// <param name="c">machine code</param>
        public immediate_shift(int c) : base(c)
        {
            shiftAmount = memory.ExtractBits_shifted(c, 8,11);
            im = memory.ExtractBits_shifted(c, 0,7);
        }

        /// <summary>
        /// rotates the immediate value
        /// </summary>
        /// <returns>the rotated value</returns>
        public override int Compute()
        {
            //ror(im, shiftamount)
            int ans = Ror(im, shiftAmount << 1);
            if (cflag < 2)
            {
              carryout = (ans == 0 ? cflag:(ans >> 31)&1);
            }
            return ans;
            
        }
        public override string ToString()
        {
            return ", #" + Compute();
        }
    }


}
