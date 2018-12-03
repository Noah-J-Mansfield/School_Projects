using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Prototype.Instructions;
using System.Diagnostics;

/// <summary>
/// file holding CPU class
/// used to simulate a CPU by providing fetch, decode, execute, and extra support methods
/// </summary>

namespace Prototype.Model
{
    /// <summary>
    /// class for creating and executing arm instructions and handling interrupts 
    /// </summary>
    public class CPU
    {
        public memory RAM; //main memory and program
        public memory regs; //registers for program use
      
        public int steps = 0; //how many fetch decode execute cycles
        public Queue<int> breakpoint  = new Queue<int>(); //holds user generated breakpoints
        int saved_pc;//used to determine branching
        public bool IRQ = false; //interrupt flag
        //{
        //    get {
        //        IRQ_mutex.WaitOne();
        //        bool i = IRQ;
        //        IRQ_mutex.ReleaseMutex();
        //        return i;
        //        
        //        }
        //    set
        //    {
        //        IRQ_mutex.WaitOne();
        //        IRQ = value;
        //        IRQ_mutex.ReleaseMutex();
        //    }
        //}
        public int CPSR { get { return regs.ReadWord(Registers.CPSR); } set { regs.WriteWord(Registers.CPSR, value); } }

        public bool inter { get { return memory.testBit(CPSR, 7); } }
        public int pc {
            get { return regs.ReadWord(Registers.r15); }
            set
            {
               
                regs.WriteWord(Registers.r15 ,value);
           
            }
        }
        
        //initalizes CPU
        //mem is the main memory and holds the program
        // r is the registers used
        public CPU(memory mem, memory r)
        {
            RAM = mem;
            regs = r;
            IRQ = false;
        }

        //adds a breakpoiont to the program
        public void add_breakpoint(int i)
        {
            breakpoint.Enqueue(i);
            
        }

        //tests if current pc matches a brake point
        public bool isbreakpoint()
        {
            if (breakpoint != null)
            return breakpoint.Contains(pc);
            return false;
        }

        //reads the memory in ram at pc
        public int fetch()
        {
            int word = RAM.ReadWord(pc);
            //Console.WriteLine(string.Format("CPU: pc = {0:X}", pc));
            pc += 8;
            saved_pc = pc;
            steps++;
			//need to put pc with the other regs
            return word;
        }
        //decodes an integer into a arm instruction
        public Instruction decode(int code) { // -> public Instruction decode(int code)
            
            //Console.WriteLine(string.Format("CPU: code = {0:X}",code));
            
		
			return MakeInstuction.make(code, this,69);
        }
        //execute an arm instruction, determine pc, and check for interrupts
        public bool execute(Instruction instr)//-> (instruction i)
        {
           //Console.WriteLine("****************************************\nCPU:EXECUTE-STARTING");
            // call the execute function on the decoded instruction
            // i.execute();
            bool a = instr.Execute();
            if (instr is Branch || instr is BX)
            {
                if (!a) { pc -= 4; }
                return true;
            }
            //pc -= 4;
      
            
            if (!inter && IRQ)
            {
                //Console.WriteLine("CPU: EXECUTE: IRQ TRIGGERED");
                int temp = CPSR;
                change_mode(2);
                regs.WriteWord(Registers.r14, pc);
                regs.WriteWord(Registers.SPSR, temp);
                pc = 0x18;
                
            }
            if (saved_pc == pc)
                pc -= 4;
            
            //Console.WriteLine("CPU:EXECUTE-DONE\n****************************************");
            return a;
        }


        //changes the mode bits in cpsr and swaps in banked regs
        public void change_mode(int m)
        {
            switch (m)
            {
                case 0: //sys
                    CPSR = regs.ReadWord(Registers.SPSR);
                    Registers.state = 0;
                    CPSR = unchecked((int)((CPSR & 0xFFFFFFE0) | 0x1F));
                    //memory.setbit(CPSR, 7, false);
                    break;
                case 1: //svc
                    Registers.state = 1;
                    CPSR = unchecked((int)((CPSR & 0xFFFFFFE0) | 0x13));
                    break;
                case 2: //irq
                    
                    IRQ = false;
                    CPSR = memory.setbit(unchecked((int)((CPSR & 0xFFFFFFE0) | 18)), 7, true);
                    Registers.state = 2;
                    break;
            }
        }
    }
}
