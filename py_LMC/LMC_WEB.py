import LMC
import bottle




        

@bottle.route('/')
def action():
    
    message = "enter program here"
    if 'action' in bottle.request.params:
        action = bottle.request.params["action"]

        if action == "Step":
            LMC.step()
        elif action == "Run":
            LMC.run()
        elif action == "Load":
            if 'program' in bottle.request.params:
                if 'inbox' in bottle.request.params:                 
                    program = bottle.request.params["program"]
                    inbox = bottle.request.params["inbox"]
                   
                    
                    LMC.loadAssembly(program, inbox)
                    
    
    return HTML_TEMPLATE.format(LMC.Format_Dump(),message)    
            
        
        
      



HTML_TEMPLATE ="""<html>

<body>
    <h1>Little Man Computer</h1>
    <form>
        <pre> {0}
        </pre>
        <input type="submit" name="action" value="Step">
        <input type="submit" name="action" value="Run">
        <h2>Load Program</h2>
        <textarea name="program" rows="20" cols="30">{1}</textarea><br>
        Enter input here (comma-delimited numbers): <input type="text" name="inbox" value="2,3,4">
        <input type="submit" name="action" value="Load">
    </form>
</body>

</html>"""


if __name__ == "__main__":
    # Launch the BottlePy dev server 
    import wsgiref.simple_server
    wsgiref.simple_server.WSGIServer.allow_reuse_address = 0
    bottle.run(host="localhost", port=8080, debug=True)



