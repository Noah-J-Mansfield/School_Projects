
import sys
'''CpS 110 Program 5: Text Adventures

Completed by Noah Mansfield (nmans433)
'''

#global vars
flags = {"last input":"","last target":"","return":[]}



class Phrase:
    """A pair of strings: .verb and .info
    
    .verb is the action to perform (always lowercase).
    .info is extra information the action may need.
    """
    def __init__(self, verb: str, info: str):
        self.verb = verb.lower()
        self.info = info
        
        
    
    def is_chapter(self, label: str) -> bool:
        """Is this Phrase of the form "chapter <label>"?"""
        
        return (self.verb == "chapter") and (self.info == label)

        #raise NotImplementedError()
    
    def is_end(self) -> bool:
        """Is this Phrase's verb "end"?"""
        return self.verb.lower() == "end"
        #raise NotImplementedError()
    


class Line:
    """A list of one or more Phrase objects.
    """
    def __init__(self):
        self.phrases = []
    
    def add(self, p: Phrase):
        """Add a Phrase to the end of our list."""
        self.phrases.append(p)
    
    def length(self) -> int:
        """Return the number of Phrases in our list."""
        return len(self.phrases)
    
    def get(self, i: int) -> Phrase:
        """Return the <i>'th Phrase from our list.
        
        Precondition: <i> is a valid, 0-based index
        """
        return self.phrases[i]
    
    def is_chapter(self, label: str) -> bool:
        """Does this Line begin with a Phrase of the form "chapter <label>"?"""
        return (self.phrases[0].verb == "chapter") and (self.phrases[0].info == label)


class Script:
    """A list of Line objects comprising a TAIL script.
    """
    def __init__(self):
        self.lines = []
    
    def add(self, line: Line):
        """Add <line> to the end of our list IF <line> is not empty."""
        if line != []:
            self.lines.append(line)
    
    def length(self) -> int:
        """Return the number of Lines in this script."""
        return len(self.lines)
    
    def get(self, i: int) -> Line:
        """Return the <i>'th Line of the script.
        
        Precondition: <i> is a valid, 0-based index
        """
        return self.lines[i]
    
    def find_chapter(self, label: str) -> int:
        """Return the index of the Line containing the Phrase "chapter <label>".
        
        If no such Line can be found, raises a ValueError exception.
        """
        g = 0
        for i in self.lines:
            
            if i.phrases[0].verb == "chapter" and i.phrases[0].info == label:
                return g
            g = g + 1
        raise ValueError()
    
    def next_phrase(self, iline: int, iphrase: int) -> (int, int):
        """Return the line/phrase indices of the next phrase after <iline>/<iphrase>.
        
        Precondition:
            * <iline> is a valid, 0-based index into our list of Lines
            * <iphrase> is a valid, 0-based index into that Line's list of Phrases
        """

        
        if (iphrase + 1) < len(self.lines[iline].phrases):
            return  iline,iphrase+1
        else:
            return self.next_line(iline,iphrase)

    
    def next_line(self, iline: int, iphrase: int) -> (int, int):
        """Return the line/phrase indices of the next line after <iline>/<iphrase>.

        Precondition:
            * <iline> is a valid, 0-based index into our list of Lines
            * <iphrase> is a valid, 0-based index into that Line's list of Phrases
        """
        if (iline + 1) < len(self.lines):
            return iline + 1, 0
        else:
            raise ValueError()
        



def load_script(stream) -> Script:
    """Return the Script created by parsing the contents of the file <stream>."""
    row = stream.readline()
    script = Script()
    j =0
    while row != "":
        row = row.strip()
    

        if row != "":
            
            if row[0] != "#":
                
                line = Line()
                phrases = []
                index = 0
                if row[0] == ">":
                    
                    line.add(Phrase("println",row[1:]))
                else:
                    phrases = row.split(" ")
                    i = len(phrases)

                    while index < i:
                        
                        
                        line.add(Phrase(phrases[index],phrases[index + 1].replace("+"," ")))
                        
                            
                        index = index + 2

                script.add(line)          
        row = stream.readline()
      
        j = j + 1
    return script


class Interpreter:
    """The logic and context required to interpret a TAIL script.
    
    Keeps a copy of the script being interpreted, along
    with all "state" required to keep track of where we
    are in the script, etc.
    
    Interpreters keep track of the current location in
    the TAIL script with a "bookmark": a pair of integers,
    one telling us the current LINE of the script,
    the other telling us the current PHRASE within that LINE.
    
    Each .step() call will
        * get the Phrase indicated by the "bookmark"
        * interpret that Phrase
        * and, depending on the Phrase, advance the "bookmark" to
            - the next phrase in that line, or
            - the next line, or
            - another line altogether (i.e., a "chapter" line)
    """
  




    def __init__(self, s: Script):
        self.script = s
        self.bookmark = [0,0]
        
       
    
    def _advance_phrase(self):
        """Move the "bookmark" forward to the next phrase.
        
        If there IS no next phrase (i.e., we were at the last
        phrase in the line), advance to the beginning of the
        next line instead.
        """
        phrases = self.script.get(self.bookmark[0]).phrases
        
        if (self.bookmark[1] + 1) < len(phrases):
            self.bookmark[1] += 1
        else:
            self._advance_line()
           
    
    def _advance_line(self):
        """Move the "bookmark" forward to the beginning of the next line."""
        self.bookmark[0] += 1
        self.bookmark[1]  = 0
    
    def _skip_to_line(self, iline: int):
        """Move the "bookmark" to the beginning of line <iline>."""
        self.bookmark[0] = iline
        self.bookmark[1] = 0
    
    def next_phrase(self) -> Phrase:
        """Get whatever Phrase the "bookmark" is pointing at."""
        return self.script.get(self.bookmark[0]).get(self.bookmark[1])
        
    def step(self):
        """Get the current Phrase, "interpret" it, and advance the "bookmark" appropriately."""
        # TODO: add logic to
        #   - get current phrase
        #   - "execute" that phrase (i.e., carry out the described action)
        #   - advance the bookmark to indicate the next phrase to interpret
        global l
        global flags
        phrase = self.next_phrase()
        info = phrase.info.replace("$",flags["last target"])
       
        
        if phrase.verb == "println":
            print(info)

        

        elif phrase.verb == "print":
            print(info,end='')

        elif phrase.verb == "goto":
            flags["last target"] = info
            i = 0
            while i < len(self.script.lines):
                if self.script.get(i).get(0).is_chapter(info):
                    self._skip_to_line(i)
                i += 1

        elif phrase.verb == "end":
            return "end"

        elif phrase.verb == "prompt":
            
            
            flags["last input"] = input(info)
           
         

        elif phrase.verb == "on":
            if not(info.lower() in flags["last input"].lower()):
                self._advance_line()
                return 0

        elif phrase.verb == "set":
        
            flags[info] = True
            
        elif phrase.verb == "clear":
            flags[info] = False

        elif phrase.verb == "if":
            if info in flags:
                if flags[info] != True:
                    self._advance_line()
                    return False
            else:
                flags[info] = False
                self._advance_line()
                return False

        elif phrase.verb == "unless":
            
            if info in flags:
                if flags[info] == True:
                    self._advance_line()
                    return False# might need to delete this
            
                

        elif phrase.verb == "match":
            l = len(info)
            if info.lower() == flags["last input"][:l].lower():
                flags["last input"] = flags["last input"][l:]
            else:
                self._advance_line()
                return False

        elif phrase.verb == "visit":
            flags["return"].append(self.bookmark[0])
            
            i = self.script.find_chapter(info)
            self._skip_to_line(i)
            

        elif phrase.verb == "return":
            self._skip_to_line(flags["return"].pop())
            self._advance_line()
            return False
            
        self._advance_phrase()



        
    def run(self):
        """Repeatedly step until an "end" Phrase is encountered."""
        while not self.next_phrase().is_end():
            self.step()


if __name__ == "__main__":
    # TODO: add logic to
    #  - get filename from command line argument 1 (or print an error/usage message if that argument is missing)
    #  - load script from that file
    #  - create interpreter to run the loaded script
    #  - run the interpreter to completion
    filename = sys.argv[1]

    if filename == "":
        sys.stderr.write("Error: no file given")
   
    else:
        with open(filename,"r") as file:

            script = load_script(file)
            terp = Interpreter(script)
            terp.run()