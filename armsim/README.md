# Noah Mansfield
# CPS 310, 11/7/2018
## Hours spent on sim2: 29

### **Features**
All A-level features have been completed: memory mapped i/0, correct dissasembly, interrupt handling, reset handling, trace with and without tracing interrupts, trace suppresion durring reset, input/output through gui console, and all gui windows. 

### **Preqiuisites**
Windows 10
visual studio 2017
### __Build and Test__
BUILD:
click on the solution file and let visual studio load the file then run from the IDE

TEST:
To run unit tests, run the program with the --test flag; the GUI will not launch
### **Configuration**
STILL HAVE NOT FIXED: Logging currently cannot be toggled. default logging disabled for all but loader

### **User Guide**
Program to load a elf file into simulated ram memory
USAGE: armsim [ --load elf-file ] [ --mem memory-size ] [ --test ]
    
--load: optional, the name of the location of the elf file to load
    
--mem: optional, default = 32768, the size of the simulated ram in bytes
    
--test: optional, if present disables gui and runs unit tests; GUI will not launch

--exec: optional, launches the program without gui and starts execution on the loaded file. Is ignored if no valid --load command is given

--traceall: if included, trace will include all cycles completed in sys, svc, and irq. else, only sys is included in trace

GUI controls:

Load(ctrl-O): opens a dialog that allows you to pick a file to load into memory.

Trace(ctrl-T): toggles trace on and off (default ON). trace rights a log to file in current directory

Run(F5): executes a a fetch, decode, and execute cycle until stop is pressed, a break point is reached, or a zero is fetched

Step(F10): completes one fetch, decode, and execute cycle

Stop(ctrl-Q): stops the run function

Break point(ctrl-B): open a dialog and prompts for a memory location in hex 

Reset(ctrl-R): sets all memory to zero and reload last file

Memory address bar: enter a hex number and press enter. The panel will show that memory location.

Disassembly address bar: enter a hex number and press enter. The panel will show that memory location.

### **Instruction Implementation**
Implemented instructions, since sim1: msr, mrs, movs, b, bl, bx, cmp, and swi



### **Bug Report**
Sometimes after a failed file load, an error message will display when you load a good file. 
For progs with interrupt vector:
Inputing characters too fast will crash the program. Inputing characters before reset has finished can crash the program. Entering characters before running the program will crash the program. 
the output window cannot scroll. 
### **Project Journal**
https://docs.google.com/document/d/1yn-Zqehsxkjj4HW-a2YqplFgdBICJMwYbHREFIF5Snk/edit?usp=sharing
### **Academic Integrity Statement**
"By affixing my signature below, I certify that the accompanying work represents my own intellectual effort. Furthermore, I have received no outside help other than what is documented below."

Noah Mansfield
