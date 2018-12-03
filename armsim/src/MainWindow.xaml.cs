///MainWindow.xaml.cs
///Control class for armsim
///sets up gui and computer class, and it deals with user input

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualBasic;
using System.IO;
using Prototype.Unittests;
using Prototype.Model;
using Prototype.Instructions;
namespace Prototype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static Computer com;
        public static Options opt;

        // 1: error loading file
        bool file_loaded = false;
        public MainWindow()
        {

            
           

            opt = new Options(App.args);
            
          
           Console.WriteLine("GUI: flag "+opt.flag);
            if (opt.flag)
            {
               Console.WriteLine("GUI: Starting Loader tests");
                Loader.RunTests();
               Console.WriteLine("GUI: Starting Computer tests");
                TestComputer.runtests();
               Console.WriteLine("GUI: Starting Cpu tests");
                TestCpu.runtests();
               Console.WriteLine("GUI: Finished tests, exiting");

               Console.WriteLine("GUI: Starting Operand2 tests");
                TestOperand2.RunTests();
               Console.WriteLine("GUI: Finished Operand2 tests");

               Console.WriteLine("GUI: Starting Arm Instruction tests");
                TestInstructions.run();
               Console.WriteLine("GUI: Finished Arm Instruction tests");
               Console.WriteLine("All unit test have passed");
                System.Environment.Exit(1);
            }
            com = new Computer(opt);
            com.writechar += this.writechar;
            InitializeComponent();

            set_shortcuts();

            if (opt.filename != "")
            {
               Console.WriteLine("GUI: file selected is " + opt.filename);
                file_loaded = com.loadfile();
                
                if (opt.exec)
                {
                    run();
                    System.Environment.Exit(0);
                }
                update_panels();
            }
           
          
        }



        public void set_shortcuts()
        {
            RoutedCommand newCmd = new RoutedCommand();
            newCmd.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(newCmd, load_b_Click));
            newCmd = new RoutedCommand();
            newCmd.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(newCmd, stop_b_Click));
            newCmd = new RoutedCommand();
            newCmd.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(newCmd, reset_b_Click));
            newCmd = new RoutedCommand();
            newCmd.InputGestures.Add(new KeyGesture(Key.B, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(newCmd, breakpoint_bt_Click));
            newCmd = new RoutedCommand();
            newCmd.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(newCmd, Trace_bt_Click));
            newCmd = new RoutedCommand();
            newCmd.InputGestures.Add(new KeyGesture(Key.F5));
            CommandBindings.Add(new CommandBinding(newCmd, run_b_Click));
            newCmd = new RoutedCommand();
            newCmd.InputGestures.Add(new KeyGesture(Key.F10));
            CommandBindings.Add(new CommandBinding(newCmd, step_b_Click));
        }


        private void run_b_Click(object sender, RoutedEventArgs e)
        {
            run_b.IsEnabled = false;
            stop_b.IsEnabled = true;
            step_b.IsEnabled = false;
            reset_b.IsEnabled = false;
            breakpoint_bt.IsEnabled = false;

         

         

            ThreadStart runcpu = new ThreadStart(run);

            Thread run_t = new Thread(runcpu);

            
            run_t.Start();

        }

        public void run()
        {
            
            com.setrunning(true);
            try
            {
                com.run();
            }
            catch { Dispatcher.Invoke(crash); }
            Dispatcher.Invoke(stop);


        }
        
        public void writechar(object sender, EventArgs args)
        {
            memory mem = sender as memory;
           //Console.WriteLine("GUI RUN_THREAD: UPDATEING THE OUTPUT key="+mem.key);
            Dispatcher.Invoke(new Action(() => WriteCharToOutput(mem.key)));
        }

        private void WriteCharToOutput(char c) { output_tb.Text+= c; }


        private void load_b_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog filedialog = new OpenFileDialog();

            filedialog.InitialDirectory = Directory.GetCurrentDirectory();
            filedialog.Filter = "Exe files (*.exe)|*.exe";
            filedialog.RestoreDirectory = false;
            if (filedialog.ShowDialog() == true)
                opt.filename = filedialog.FileName;


            if (opt.filename != null)
            {
               Console.WriteLine("GUI: file selected is " + opt.filename);
                com.reset();
                output_tb.Text = "";
                file_loaded = com.loadfile();
               //Console.WriteLine("GUI: pc=" + com.getpc() + ", r13="+com.getreg(Registers.r13));
                //file_loaded = (Loader.load(com.Getram(), opt) == 0);
               
            }
            else
                opt.filename = "NONE";

            update_panels();
        }
        private void update_panels()
        {
            if (file_loaded)
            {
               Console.WriteLine("\n--------------------\nGUI: UPDATEING PANELS");
                Filename.Content = "Current file: " + opt.filename;
                set_memory_panel(com.getpc());
                set_dissasembler_panel(com.getpc());
                set_regester_panel();
                set_stack();
                set_CPSR();
                Checksum_lb.Content = String.Format("Checksum: {0}", com.Getram().Checksum());
               Console.WriteLine("GUI: PANELS UPDATED\n--------------------");
            }
            else
            {
               Console.WriteLine("GUI: UPDATING PANELS TO BLANK");
                Mem_view.Items.Clear();
                dissasmbly_view.Items.Clear();
                Reg_view.Items.Clear();
                stack_view.Items.Clear();
                NZCF_lb.Content = "NZCF:";
                Checksum_lb.Content = "Checksum:";
                com.reset();
                Filename.Content = "Current File: NONE";
                if(opt.filename != "")
                output_tb.Text += "ERROR: could not load File: "+opt.filename+"\n"+Loader.errormsg;
            }
        }

        public void set_CPSR()
        {
            int nzcf = com.NZCF;
            NZCF_lb.Content = String.Format("MODE={0} : I={1}\nN={2} : Z={3} : C={4} : F={5}", (com.mode == "SVC" ? "SUPERVISOR" : (com.mode == "SYS" ? "SYSTEM    ":"IRQ       ")), (nzcf >> 7 & 1), (nzcf >> 31) & 1, (nzcf >> 30) & 1, (nzcf >> 29) & 1, (nzcf >> 28) & 1);
        }

        private void set_memory_panel(int address)
        {
           Console.WriteLine("GUI: updating memory panel to address: 0");
            Mem_view.Items.Clear();
            address -= address % 16;
            for (int i = address; i < address+(16*7); i += 16)
            {
                string[] row = com.get_Line_of_memory(i);

                Mem_view.Items.Add(new memline() {Address = row[0], Memory = row[1], Ascii = row[2] });//.Items.add(com.get_Line_of_memory(i));
            }
            Mem_view.SelectedIndex = 0;
        }

        private void set_stack()
        {
            stack_view.Items.Clear();
            int i = com.getreg(Registers.r13);
            for (int count =0; count < 5; i += 4, count++)
            {
                stack_view.Items.Add(new stack() { Address = string.Format("0x{0:X8}",i), Value = string.Format("0x{0:X8}",com.Getram().ReadWord(i))});
               
            }
        }
        private void set_regester_panel()
        {
            Reg_view.Items.Clear();
            
            for (int i = 0; i < 60; i += 4)
            {
                Reg_view.Items.Add(string.Format("reg{0:d2} = 0x{1:X8}",i/4,com.getreg(i)));
            }
            Reg_view.Items.Add(string.Format("pc    = 0x{0:X8}", com.getpc()));
        }
        private void set_dissasembler_panel(int addr)
        {
           Console.WriteLine("GUI: setting dissasembly panel to "+addr);
            dissasmbly_view.Items.Clear();
            addr -= addr % 4;
            //string[] asm = com.dissassmble();
            if (addr > 11)
            {
                addr -= 12;
            }
            int j = addr;
            for (int i = 0; addr < j + (7*4); addr+= 4, i++)
            {
                //string.Format("0x{0:X8}", com.Getram().ReadWord(addr)), Assembly = asm[i] 
                dissasmbly_view.Items.Add(new asmline() {Address = string.Format("0x{0:X8}",addr), Instruction = com.dissassmble(addr)});
           
            }
            dissasmbly_view.SelectedIndex = 3;
        }

       

        public void stop()
        {
            run_b.IsEnabled = true;
            step_b.IsEnabled = true;
            stop_b.IsEnabled = false;
            reset_b.IsEnabled = true;
            breakpoint_bt.IsEnabled = true;
            update_panels();
        }

        private void crash() { output_tb.Text += "Program crashed!"; }

        private void step_b_Click(object sender, RoutedEventArgs e)
        {
            run_b.IsEnabled = false;
            stop_b.IsEnabled = false;
            step_b.IsEnabled = false;
            try
            {
                com.step();
            }
            catch
            {
                crash();
            }
            stop();
           
        }

        private void stop_b_Click(object sender, RoutedEventArgs e)
        {
            com.setrunning(false);
        }

        private void Memsel_tb_KeyDown(object sender,KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    int i = int.Parse(Memsel_tb.Text, System.Globalization.NumberStyles.HexNumber);
                   //Console.WriteLine("GUI: i = " + i + " i % 16 =" + i % 16);
                   
                    
                    set_memory_panel(i);
                    Mem_view.SelectedIndex = 0;
                    //ListViewItem it = Mem_view.Items.IndexOf(i/16);
                   Console.WriteLine("GUI:Finding mem address");

                }
                catch (Exception) {Console.WriteLine("GUI: could not convert address");
                }
            }
        }
        
        private void reset_b_Click(object sender, RoutedEventArgs e)
        {
            output_tb.Text = "";
            Mem_view.Items.Clear();
            dissasmbly_view.Items.Clear();
            Reg_view.Items.Clear();
            stack_view.Items.Clear();
            NZCF_lb.Content = "NZCF:";
            Checksum_lb.Content = "Checksum:";
            com.reset();
            if (opt.filename != "")
            {
               Console.WriteLine("GUI: file selected is " + opt.filename);

                file_loaded = com.loadfile();
                
            }
            update_panels();


        }

        private void breakpoint_bt_Click(object sender, RoutedEventArgs e)
        {
            string bp = Microsoft.VisualBasic.Interaction.InputBox("Please enter the hex value memory address for the break point", "Breakpoint", "");
            try
            {
                int intValue = int.Parse(bp, System.Globalization.NumberStyles.HexNumber);
                com.add_breakpoint(intValue);
            }
            catch (Exception) { }
        }

        private void dis_tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    int i = int.Parse(dis_tb.Text, System.Globalization.NumberStyles.HexNumber);
                   
                    set_dissasembler_panel(i);
                    
                   Console.WriteLine("GUI:Finding mem address");

                }
                catch (Exception) {
                        Console.WriteLine("GUI: could not convert address");
                    }
            }
        }

        private void Trace_bt_Click(object sender, RoutedEventArgs e)
        {
           
            if (com.trace)
            {
                Trace_bt.Content = "Trace: OFF";
            }
            else
            {
                Trace_bt.Content = "Trace: ON";
            }
            com.toggle_trace();
        }

  





        private void output_tb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            output_tb.Focus();
        }

        private void output_tb_TextInput(object sender, TextCompositionEventArgs e)
        {
            Char keyChar = (Char)System.Text.Encoding.ASCII.GetBytes(e.Text)[0];
            //output_tb.Text += keyChar;
            com.keypress(keyChar);
           //Console.WriteLine("GUI: KEY =" + keyChar);
            com.set_IRQ();
        }
    }

    public class memline
    {
        public string Address { get; set; }
        public string Memory { get; set; }
        public string Ascii { get; set; }
    }
    public class stack
    {
        public string Address { get; set; }
        public string Value { get; set; }
    }
    public class asmline
    {
        public string Address { get; set; }
        public string Instruction { get; set; }
        public string Assembly { get; set; }
    }

}