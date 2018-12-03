using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;
using Prototype.Instructions;
using System.Windows.Threading;

/// <summary>
/// file holding computer class to help direct adn control communications with view and cpu and memory
/// </summary>

namespace Prototype.Model
{
    /// <summary>
    /// controls logic flow for memory and cpu
    /// sets up cpu, ram, and regs
    /// handles all communication with view
    /// handles trace files and loading elf exes
    /// </summary>
    public class Computer
    {
        public event EventHandler writechar; //handles i/o write
        public char keyboard; //temp for keybored
        memory RAM; //main memory
        memory regs; //regs for cpu
        CPU cpu; //performs fetch decode execute cycles
        Options opt; //the settings for computer and cpu 
        public bool trace = true; //write trace or no
        public bool trace_all = false; //trace exceptions?
        bool suppress= true; //do not trace reset
        private static Mutex mutex = new Mutex(); //thread safe way to access is running
        private static Mutex key_mutex = new Mutex(); //thread safe way to access keyboard
        bool running = false; //enable run
        StreamWriter file; //file to write trace too

        //mem_observer observer;

        public Computer(Options op)
        {
            //observer = new mem_observer();
            
            opt = op;
            trace_all = op.traceall;
            RAM = new memory(opt.memsize);
            regs = new memory(148);
            regs.WriteWord(Registers.CPSR, 0b10011); //set mode to SVC
            regs.flag = true; //used for swaping out regss
            cpu = new CPU(RAM, regs);
            file = new StreamWriter(Directory.GetCurrentDirectory() + "\\trace.log");
            RAM.writetoConsole += this.writetoConsole;
            RAM.readfromkeyboard += this.readformkeyboard;
            Registers.state = 1;
        }
        public bool inter { get { return cpu.inter; } }
        public int NZCF { get { return cpu.CPSR; } }

        //returns a string containing the mode of the CPSR
        public string mode
        {
            get
            {
                int code = memory.ExtractBits_shifted(cpu.CPSR, 0, 4);
                return code == 18 ? "IRQ" : (code == 19 ? "SVC" : "SYS");
            }
        }

        //loads an elf file into ram
        public bool loadfile()
        {
            if(Loader.load(RAM, opt) == 0)
            {
                if (cpu.RAM.ReadWord(0) == 0)
                {
                    cpu.pc = Loader.pc;
                    Registers.state = 0;
                    //set to system disable interrupts
                    regs.WriteWord(Registers.CPSR, cpu.CPSR = memory.setbit(0x1f, 7, true));
                    cpu.regs.WriteWord(Registers.r13, 0x7000);
                   //Console.WriteLine("COMPUTER: no interrupt table");
                  
                    suppress = false;
                }
                else
                {
                    cpu.pc = 0;
                    regs.WriteWord(Registers.CPSR, 0b10011); //set to svc
                    suppress = true;
                   //Console.WriteLine("COMPUTER: interupt table found, SETTING REGS pc=" + cpu.pc);
                    Registers.state = 1;
                }
                return true;
            }
            return false;
        }

        //returns a reference to ram
        public memory Getram()
        {
            return RAM;
        }
       
        //performs one fetch->execute->decode cycle
        public void step()
        {
           //Console.Write("\n");
            int temp_pc = cpu.pc;
            string smode = mode;
            int temp = cpu.fetch();
            Instruction t = cpu.decode(temp);
            bool j = cpu.execute(t);
            if (  !j || cpu.isbreakpoint())
                setrunning(false);
           //Console.WriteLine("COMPUTER: TRACE = " + trace);
            if (trace)
            {
                writetrace(temp_pc,smode);
            }
           //Console.Write("\n");
            if (Loader.pc == cpu.pc && suppress)
            {
                suppress = false;
                cpu.steps = 0;
            }
        }
        //while running is true performs fetch->decode->execute
        public void run()
        {
            while (true)
            {
                step();
                mutex.WaitOne();
                if (!running)
                {
                   //Console.WriteLine("COMPUTER: EXITING THREAD");
                    mutex.ReleaseMutex();
                    return;
                }
               //Console.WriteLine("COMPUTER: RUNNING, step counter: "+cpu.steps);
                
                mutex.ReleaseMutex();
               
                
            }
         
        }

        //writes to the trace file: pc = location of instruction that just executed
        // saved mode = mode of cpsr when instruction began execution
        public void writetrace(int pc, string saved_mode)
        {
          
            string str_trace = "";
            for (int i =0; i <60; i+=4)
            {
                str_trace += string.Format(" {0}={1:X8}",i/4,getreg(i));
            }
            int nzcf = cpu.CPSR;
           //Console.WriteLine(String.Format("CPSR: {0}{1}{2}{3}",  (nzcf >> 31) &1, (nzcf >> 30) & 1, (nzcf >> 29) & 1, (nzcf >> 28) & 1));
            string NZCF_reg = String.Format("{0}{1}{2}{3}", (nzcf >> 31) & 1, (nzcf >> 30) & 1, (nzcf >> 29) & 1, (nzcf >> 28) & 1);
           //Console.WriteLine(string.Format("{0:d6} {1:X8} {2:X8} {3} {4}{5}", cpu.steps, pc, RAM.Checksum(), NZCF_reg, saved_mode, str_trace));
            if (trace_all && !suppress)
            {

               
                file.WriteLine(string.Format("{0:d6} {1:X8} {2:X8} {3} {4}{5}", cpu.steps, pc, RAM.Checksum(), NZCF_reg, saved_mode, str_trace));
            }
            else if (!trace_all && saved_mode == "SYS" && !suppress)
            {
                file.WriteLine(string.Format("{0:d6} {1:X8} {2:X8} {3} {4}{5}", cpu.steps, pc, RAM.Checksum(), NZCF_reg, "SYS", str_trace));
            }
            file.Flush();
        }

        //returns a line of ram in ram starting a address
        public string[] get_Line_of_memory(int address)
        {
            string mem_line = "";
            string ascii = "";
            string addr = string.Format("0x{0,-3:X8}:", address);
            for (int i = address; i < (address + 16); i++)
            {
                byte b = RAM.ReadByte(i);
                mem_line += " " + string.Format("{0,-4:X2}",b);
                if (b > 32)
                {
                    ascii += (char)b + " ";
                }
                else
                {
                    ascii += ". ";
                }
              

            }

            string[] str = { addr, mem_line, ascii };

            return str;;
        }

        //returns a register from cpu
        public int getreg(int r)
        {
            return cpu.regs.ReadWord(r);
        }

        //returns the string of the instruction located at addr
        public string dissassmble(int addr)
        {
            
            return MakeInstuction.make(RAM.ReadWord(addr),cpu,addr).Disassemble();
        }
    
        //toggles trace on and off. closes and opens files when needed
        public void toggle_trace()
        {
            if (trace)
            {
                trace = false;
                if (file != null)
                {
                   //Console.WriteLine("COMPUTER: Closing trace.log");
                    file.Close();
                   //Console.WriteLine("COMPUTER: trace.log Closed");
                }
             
            }
            else
            {
               //Console.WriteLine("COMPUTER: " + Directory.GetCurrentDirectory() + "\\trace.log");
                trace = true;
                file = new StreamWriter(Directory.GetCurrentDirectory() + "\\trace.log");
            }
        }
        //thread safe method to turn off running
        public void setrunning(bool flag)
        {
            mutex.WaitOne();
            running = flag;
            mutex.ReleaseMutex();
        }

        //resets the cpu and computer
        public void reset()
        {
            if (RAM.ReadWord(0) == 0)
            {
                cpu.pc = Loader.pc;
                cpu.regs.WriteWord(Registers.r13, 0x7000);
                cpu.CPSR = memory.setbit(0x1f,7,true); //set to system disable interrupts
                Registers.state = 0;
            }
            else
            {
                cpu.pc = 0;
                cpu.CPSR = 0b10011; //set mode to SVC
                suppress = true;
                Registers.state = 1;
            }
            RAM.reset();
            regs.reset();
            cpu.steps = 0;
            if (file != null)
            {
                file.Close();
                if(trace)
                    file = new StreamWriter(Directory.GetCurrentDirectory()+"\\trace.log");
            }
        }

        //adds a breakpoint to the cpu
        // bp = pc to break at
        public void add_breakpoint(int bp)
        {
            cpu.add_breakpoint(bp);
        }

        //returns cpu current pc
        public int getpc()
        {
            return cpu.pc;
        }

        //observer method is alerted when i/o mapped mem is read from 
        public void readformkeyboard(object sender, EventArgs args)
        {
            memory s = sender as memory;
            key_mutex.WaitOne();
            s.key = keyboard;
            key_mutex.ReleaseMutex();
        }
        //observer method is alerted when i/o mapped mem is written too 
        public void writetoConsole(object sender, EventArgs args)
        {
           writechar(sender, args);
        }

        //sets the irq for cpu
        public void set_IRQ() { cpu.IRQ = true; }

        //places keyboard input into temp var. threadsafe
        public void keypress(char key)
        {
            key_mutex.WaitOne();
            keyboard= key;
            key_mutex.ReleaseMutex();
        }
    }


  


}
