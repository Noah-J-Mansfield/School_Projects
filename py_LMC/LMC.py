# lmc.py
# An implementation of a variation of the Little Man Computer
# https://en.wikipedia.org/wiki/Little_man_computer

# Global variables for LMC components
memory = [0] * 100
pc = 0
accum = 0
inbox = []
outbox = []
running = True  # Set to False when HLT is executed

# Instruction numbers
HLT = 0
ADD = 1
SUB = 2
STA = 3
LDA = 4
BRA = 5
BRZ = 6
INP = 7
OUT = 8


# ---------------- LMC Component Interfaces ------------------

def readMem(addr):
    """Returns value at address `addr` in memory, or 0 if `addr` is out of range"""
    if 0 <= addr < len(memory):
        return memory[addr]
    else:
        return 0

def writeMem(addr, val):
    """Writes `val` to memory cell at address `addr`"""
    if 0 <= addr < len(memory) and 0 <= val <= 999:
        memory[addr] = val

def readAccum():
    """Returns value of accumulator"""
    return accum

def writeAccum(val: int):
    """Writes `val` to accumulator, if 0 <= `val` <= 999"""
    global accum
    if 0 <= val <= 999:
        accum = val

def readPC():
    """Returns current program counter value"""
    return pc

def writePC(val):
    """Writes `val` to program counter, if 0 <= `val` <= 999"""
    global pc
    if 0 <= val < len(memory):
        pc = val

def readInbox():
    """Removes and returns first number from inbox. If inbox is empty, returns 0."""
    global inbox
    if inbox == []:
        return 0
    else:
        return int(inbox.pop(0))    
        


def writeOutbox(val):
    """Places `val` at end of outbox"""
    outbox.append(val)

# ------------ Fetch / Decode / Execute Functions ------------

def fetch():
    """Fetches and returns next instruction indicated by PC. Increments PC."""
    pcval = readPC()
    instr = readMem(pcval)
    writePC(pcval + 1)
    return instr

def decode(instr: int) -> (int, int):
    """Decodes instruction `instr`, returning its (opcode, operand)"""
    return (instr // 100, instr % 100)

def execute(opcode: int, operand: int):
    """Executes instruction corresponding to `opcode`, using `operand` if needed"""
    global running
    if opcode == OUT:
        writeOutbox(readAccum())
    elif opcode == LDA:
        writeAccum(readMem(operand))
    elif opcode == HLT:
        running = False
    elif opcode == INP:
        writeAccum(readInbox()) 
    elif opcode == ADD:
        writeAccum(readAccum() + readMem(operand))
    elif opcode == STA:
        writeMem(operand, readAccum())
    elif opcode == SUB:
        writeAccum(readAccum() - readMem(operand))
    elif opcode == BRA:
        writePC(operand)
    elif opcode == BRZ:
        if readAccum() == 0:
            writePC(operand)
        
        

def step():
    """Performs one fetch-decode-execute step"""
    instr = fetch()
    (opcode, operand) = decode(instr)
    execute(opcode, operand)

def run():
    """Performs fetch-decode-execute steps until `running` is False"""
    while running:
        step()

# ----------------- Simulator setup ----------------

def reset():
    """Resets all computer components to their initial state"""
    global pc, memory, accum, inbox, outbox, running
    pc = 0
    memory = [0] * 100
    accum = 0
    inbox = []
    outbox = []
    running = True

def load(program: list, indata: list):
    """Resets computer, loads memory with `program`, and sets inbox to `indata`"""
    global inbox, running
    reset()
    if indata == []:
        running = False
        print("no input. please reset and enter valid input.")

    for i in range(len(program)):
        writeMem(i, program[i])
    inbox = indata

# ---------------- Simulator "display" ----------------------

def Format_Dump():
    global inbox, pc, accum
    i = 0
    j = 10
    mDump = ""
    while i < len(memory):
       
        if j%10 == 0:
         mDump  = mDump + "\n"
        if j<20:
            mDump = mDump + " "
        if memory[i] == 0:
            mDump = mDump + str(i) + "[0  ]"
        else:
            mDump = mDump + str(i) + "[" + str(memory[i]) + "]"
        j += 1
        i +=1

    mDump = mDump + "\n                     PC[" + str(pc) + "] Acc[" + str(accum) + "]\n"
    mDump = mDump + "In box: " + str(inbox) + "\n"
    mDump = mDump +  "Out box: " + str(outbox)    
    return(mDump)



def dump():
     
     print(Format_Dump())
        


def toAssembly(instr: int) -> str:
    operand = instr%100
   
    list = ['HLT','ADD','SUB','STA','LDA','BRA','BRZ','INP','OUT']
   
    return ( list[( instr//100 )] + " " + str(operand) ) 

def disassemble(start: int, end: int):
    """Displays assembly language listing of memory contents `start` to `end`"""
    for addr in range(start, end + 1):
        print(str(addr).rjust(2) + ": " + toAssembly(readMem(addr)))

#------------ encoders & decoders

def encoder(asm:str)->int:
    """ turn strings into assembly language instructions
    takes a string in the form of  ADD 5 or HLT or SUB 87.
    converts it to a 3 digit int
    returns the 3 digit int
     """
    list = ['HLT','ADD','SUB','STA','LDA','BRA','BRZ','INP','OUT','DAT']
    i = asm.find(" ")
    if i == -1:
     
        opcode = asm[0:3].upper()
    else: 
        opcode = asm[0:i].upper()
    if opcode in list:
        if opcode == "DAT":
            return int(asm[i:].strip(" "))
        
        opcode = 100 * int(list.index(opcode))
        
            
        if len(asm)>= i and opcode != 800 and opcode != 700 and opcode != 0:
            operand = int(asm[i:].strip(" "))
            h = 9
        else:
            operand = 0

        return int( opcode + operand )
    else:
        return (-1)
    
def assemble(program: str) -> (list, list):
    lines = program.split("\n")
    i = 0
    assembled = []
    failed_Assembled = []
    while i < len(lines):
        if lines[i].find("/") > 0:
           code = encoder(lines[i][:lines[i].find("/")])
        else:
            code = encoder(lines[i])
        if code == -1:
            failed_Assembled.append(lines[i])
        else:
            assembled.append(code)
        i += 1
    while '' in failed_Assembled:
        failed_Assembled.pop(failed_Assembled.index(""))
   
    return (assembled,failed_Assembled)

def loadAssembly(program: str, indata: str)->int:
    """ translates assembly into digits"""
    
    data = indata.split(",")
    data = strList_to_intList(data)


    decode = assemble(program)
    if decode[1] == []:
        load(decode[0],data)
    else:
        print("The following instructions failed to assemble:\n",decode[1])
        


#-------------list converter
def strList_to_intList(list:list) -> list:
    """ converts a list of strings to integers 
    requires a string of digits
     """
    i = 0
    while i < len(list):
        list[i] = int(list[i])
        i+=1
    return list 


# ----------- Define shortcut names for interactive use

def sd():
    step()
    dump()

s = step
d = dump
r = run       

# ----------------- Unit Tests ------------------------

def test_mem():
    reset()
    assert memory == [0] * 100
    writeMem(1, 5)
    assert readMem(1) == 5

    reset()
    writeMem(-1, 5)
    assert memory == [0] * 100

    writeMem(1, 1000)
    assert memory == [0] * 100

def test_LDA():
    reset()
    writeMem(3, 50)
    execute(LDA, 3)
    assert readAccum() == 50

def test_OUT():
    reset()
    writeAccum(3)
    execute(OUT, 0)
    assert outbox == [3]
    
def test_toAssembly():
  
    assert toAssembly(000)  == "HLT 0"
    assert toAssembly(101)  == "ADD 1"
    assert toAssembly(202)  == "SUB 2"
    assert toAssembly(335)  == "STA 35"
    assert toAssembly(404)  == "LDA 4"
    assert toAssembly(505)  == "BRA 5"
    assert toAssembly(606)  == "BRZ 6"
    assert toAssembly(707)  == "INP 7"
    assert toAssembly(808)  == "OUT 8"

    reset()

def test_readInbox():
        global inbox
        inbox = [1,2,3]
        assert readInbox() == 1
        assert readInbox() == 2
        assert readInbox() == 3
        assert readInbox() == 0
def test_encoder():
       assert encoder("HLT") == 000
       assert encoder("ADD 56") == 156
       assert encoder("sub 5") == 205
       assert encoder("StA 4") == 304
       assert encoder("LDA 54") == 454
       assert encoder("BRA 90") == 590
       assert encoder("BRZ 21") == 621
       assert encoder("INP") == 700
       assert encoder("OUT") == 800
       assert encoder("DAT 564") == 564
       assert encoder("day") == -1


if __name__ == "__main__":
    reset()
    test_encoder()