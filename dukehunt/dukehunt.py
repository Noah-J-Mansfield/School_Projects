#!/usr/bin/env python3
import math
import bottle
import random



    


def click():
    """perform the correct action on object clicked"""
    if game.win == False:
        i_d = bottle.request.params["id"]
        game.turns += 5

        for i in game.objects:
            if i.id == int(i_d):
                #if i.type == 'decoy':
                
                if i.type == "duke":
                    if i.alive == True:
                        if i.hidden == True:
                            game.points +=30
                            game.hidden -= 1 #if duke has not aleready been discovered 
                
                game.sound = i.sound 
                i.reveal() #always do this

                if game.hidden <= 0: #condition to end game
                    game.win = True
                    game.points -= game.turns
                    game.end()
                    
                
            
                   
                        
        return screen()
    
    return screen()

def cheat():
    """Identify all decoys and dukes"""
    
    if game.hints == True:
        game.hints = False
    else:
        game.hints = True
    return screen()
def view():
    """update the screen, moving any able objects"""
    game.sound = "<audio src='static/sounds/35631__reinsamba__crystal_glass.wav' autoplay='true' />"
    if game.win == False:
        game.turns += 1
        game.update()
    return screen()

def screen():
    """format screen to include object created in program"""
    text = GAME_HTML
    
    
    if game.hints == True: # I attempted to use format, but it always conflicted with the css
        game.hint() #format the hints
        return text.replace("TURNSS",str(game.turns)).replace("DUKESS",str(game.hidden)).replace("%%%%",game.form_objects()).replace("$$$$",game.clues).replace("&endgame&",game.winstyle).replace("&gameover&",game.wintext).replace("^a^",str(game.sound))
    else:
        return text.replace("TURNSS",str(game.turns)).replace("DUKESS",str(game.hidden)).replace("%%%%",game.form_objects()).replace("$$$$","").replace("&endgame&",game.winstyle).replace("&gameover&",game.wintext).replace("^a^",str(game.sound))
    

def setup(d):
    """initalize and create objects for program"""
    d = int(d)
    game.dukes = d
    game.hidden = d
    num_veg = 5 * d
    i = 1
    j = 1
    while i <= num_veg:
    
        game.add(Vegitation(game.veg_img()))
        i+=1
    while j <= d:
        game.add(Duke(game.veg_img()))
        game.add(Decoy(game.veg_img()))
        j+=1

    
    return 0
        
@bottle.route('/static/<filename:path>')
def send_static(filename):
    """Helper handler to serve up static game assets.
    
    (Borrowed from BottlePy documentation examples.)"""
    return bottle.static_file(filename, root='./static')

@bottle.route('/')
def index():
    """Home page."""
    return TITLE_HTML

@bottle.route('/play')
def play():
    """branch according to user input"""
    
    state = bottle.request.params["action"]
    state= state.lower()
    if state == "start game":
        
        numD = bottle.request.params["numdukes"]
        game.reset()
        setup(numD)
        return screen()
    elif state == "click":
        return click()
    elif state == 'cheat':
        return cheat()
    elif state == 'view':
        return view()
    else:
        return GAME_HTML








class Vegitation:
    next_id = 1
    def __init__(self,img):
        self.id = Vegitation.next_id
        Vegitation.next_id+=1
        self.img = img
        self.x = random.randrange(50,950)
        self.y = random.randrange(10,450)
        self.alive = True
        self.dead = "fire.gif"
        self.type = "veg"
        self.sound = """<audio src='static/sounds/35631__reinsamba__crystal_glass.wav' autoplay='true' />"""

    def die(self):
        """change image to dead"""
        self.alive = False
        self.img = self.dead
       
      
    def reveal(self):
        """Entities class call this function"""
        pass
        
    def update(self):
        "here only for the Entities class to call"
        pass
    def clues(self):
        """returns hint image"""
        return ""

    def __str__(self):
        """formats object string for HTML use"""
        if self.alive == False:
            return """<a xlink:href="/play?action=click&amp;id={0}">
                <image x="{1}" y="{2}" xlink:href="static/img/{3}" width="34" height="34" />
            </a>""".format(self.id,self.x,self.y,self.dead)
        else:
            return """<a xlink:href="/play?action=click&amp;id={0}">
                    <image x="{1}" y="{2}" xlink:href="static/img/{3}" width="34" height="34" />
                </a>""".format(self.id,self.x,self.y,self.img)
            

class Duke(Vegitation):
    def __init__(self,img):
        super().__init__(img)
        self.dead = "tombstone.gif"
        self.discovered = "duke.gif"
        self.hidden = True
        self.hint = "dukecue.gif"
        self.type = "duke"
        self.sound = """<audio src='static/sounds/laugh.wav' autoplay='true' />"""
        self.speed = random.randrange(15,40)
        self.heading = random.randrange(0,360)
    def update(self):
        """if object not dead then moves object according to speed"""
        if self.alive == True:
            self.move()
    
    


    def move(self):
        """move object according to speed and heading"""
        rads = math.radians(self.heading)
        self.x += math.cos(rads) * self.speed      
        self.y +=-(math.sin(rads)*self.speed)
    def reveal(self):
        """change image and pervent from moving again"""
        self.img = self.discovered
        self.hidden = False
        self.speed = 0
   


    def clues(self):
        """formats hint for use in HTML"""
        return """<image x="{0}" y="{1}" xlink:href="static/img/{2}" width="34" height="34" />""".format(str(self.x),str(self.y+34),self.hint)
        

class Decoy(Duke):
    
    def __init__(self,img):
        super().__init__(img)
        self.dead = "explosion.gif"
        self.hint = "decoycue.gif"
        self.discovered = "explosion.gif"
        self.type = 'decoy'
        self.sound ="""<audio src='static/sounds/explosion.wav' autoplay='true' />"""
    def move(self):
        """sets random heading then see parnent move"""
        self.heading = random.randrange(0,360)
        super().move()

    def reveal(self):
        """does same as parent and then calls explode"""
        super().reveal()
        game.explode(self)
      
    def die(self):
        """pervent object from being activated again"""
        super().die()
        game.explode(self)

class Entities:
    
    def __init__(self):
        self.objects = []
        self.turns = 0
        self.dukes = 0
        self.hidden = 0
        self.vimg = ["bush.gif","grass.gif","tree.gif","rock.gif","flower.gif"]
        self.win = False
        self.hints = False
        self.clues = ""
        self.winstyle = ""
        self.wintext = ""
        self.points = 0
        self.sound = """<audio src='static/sounds/35631__reinsamba__crystal_glass.wav' autoplay='true' />"""

    def veg_img(self):
        """returns random image"""
        i = random.randrange(0,5)
        return self.vimg[i]

    def add(self,ob):
        """adds object to list"""
        self.objects.append(ob)

    def form_objects(self):
        """formats objects in list into HTML images"""
        l = ""
        i = 0 
        while i < len(self.objects):
            
            l += str(self.objects[i])
            i+=1
        return str(l)

    def update(self):
        """update the position of every object on screen"""
        for i in self.objects:
            i.update()
    def hint(self):
        """creates the hint HTML images for cheat function """
        l = ""
        for i in self.objects:
            l+= str(i.clues())
        self.clues = str(l) 
    
    def explode(self,i):
        """kill any living objects within 150px radius and subtract points"""
        for k in self.objects:
            if k.alive == True:
                
                if self.distance(k,i) <= 150:
                    game.points -= 1
                    if k.type == "duke":
                        game.points -= 4
                        if k.hidden == True:
                            game.hidden-= 1
                    k.die()
                   
            

    def distance(self,k,i):
        """returns absolute value of the distance between to points on a 2d plane"""
        x1 = k.x
        y1 = k.y
        x2 = i.x
        y2 = i.y
        dis = math.sqrt((x2-x1)**2+(y2-y1)**2)
        if dis < 0:
            return dis * (-1)
        else:
            return dis 

    def reset(self):
        """rests all Entities vars for new game"""
        self.win = False
        self.hints = False
        self.clues = ""
        self.winstyle = ""
        self.wintext = ""
        self.points = 0
        self.objects = []
        self.turns = 0
        self.dukes = 0
        self.hidden = 0
        self.sound = """<audio src='static/sounds/35631__reinsamba__crystal_glass.wav' autoplay='true' />"""

    def end(self):
        """formats the end game message"""
        self.winstyle = """.win{
            position: absolute;
			top: 190px;
			left:500px;
            font-size: 450%;

			color:black;
			
        }"""
        self.wintext = """<p class = 'win'>GAME OVER<br> turns: {0} | points:{1}</p>""".format(self.turns,self.points)


game = Entities() #control object for entire game

"""Gameplay page request handler."""
    

TITLE_HTML = """\
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN"    "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>

<head>
    <title>Duke Hunt</title>
    <style type='text/css'>
        body {
            background: black;
            border: 100;
            color: white;
        }
    </style>
</head>

<body>
    <center>

        <img src='static/img/dukehunt.gif' />
        <br />
        <h3>How to Play</h3>
        <p>
            Click <b>Update World</b> to advance one turn. Vegetation does not move. Dukes (and decoys!) do move.
            Click on an on-screen entity to lose 5 turns <i>and</i> reveal it (if it is a duke), detonate it (if it is a
            decoy), or simply lose 5 turns (vegetation).
        </p>
        <hr />
        <h3>Credits:</h3>
        <ul>
            <li>2dogSound_tadaa1_3s_2013jan31_CC-BY-30-US.wav by rdholder shared under the Creative Commons Attribution license.</li>
            <li>freesound.org &mdash; other sound effects</li>
        </ul>
        <br />

        <form action='/play'>
            Number of Dukes <img src='static/img/duke.gif' />: <input type='text' name='numdukes' value='3' />
            <input type='submit' name='action' value='Start Game' />
        </form>

    </center>
    <audio src='static/sounds/EASTERN_SITAR_BRIGHT_1.ogg' autoplay='true' />
</body>

</html>
"""

GAME_HTML = """\
<!DOCTYPE html>
<html xmlns='http://www.w3.org/1999/xhtml'>

<head>
    <title>Duke Hunt</title>
    <style>
        a {
            color: yellow;
        }
        
        &endgame&

        body {
            background-color: black;
            margin: 0;
            color: yellow;
        }
        
    </style>

</head>

<body>
 
    <center>
        <img src='static/img/dukehuntsmall.png' /> <br/>
        <table width='100%'>
            <tr>
                <td valign='bottom' align='right'><a href='?action=view'>Update World</a> </td>
                <td align='center' style='color:yellow'>
                    Turns: TURNSS| Undiscovered Dukes: DUKESS
                </td>
                
                <td valign='bottom' align='left'><a href='/'>New Game</a></td>
            </tr>
        </table>
       
        <svg xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' version='1.1' width='1000px' height='500px'>
          
            
            <rect x='0' y='0' width='1000' height='500' fill='lightgreen' />    
            
                
            <!-- TO DO: Replace the following "static" set of entities with 
                 those generated by the program --> 
            $$$$

            %%%%

            &gameover&
        </svg>
        <br />
        <a href="/play?action=cheat" style='color: #222222'>Cheat Mode</a>
        <br />
    </center>
    ^a^
</body>

</html>
"""





# Boilerplate bootstrapping logic for running as a standalone program
if __name__ == "__main__":
    import threading
    import time
    import webbrowser
    
    # Launch a background thread that will spawn a webbrowser in 1 second from thread start
    def wait_n_browse():
        time.sleep(1.0) # Race condition/hack, but likely to work in most cases...
        webbrowser.open("http://localhost:8080/")

    threading.Thread(target=wait_n_browse, daemon=True).start()
    
    # Launch the BottlePy dev server 
    import wsgiref.simple_server
    wsgiref.simple_server.WSGIServer.allow_reuse_address = 0

    # Launch the BottlePy dev server on the main thread (runs until Ctrl-Brk)
    bottle.run(host="localhost", port=8080, debug=True)
    


    