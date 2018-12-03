import os,sys,time



def getTime(i):
    date = os.path.getctime( i )
    date = time.localtime(date)
    

    return str(date[0]) + "-" + str(date[1]) + "-" + str(date[2])

if len(sys.argv) >= 3:
    foldername = sys.argv[1]
    exts = {}
    for j in sys.argv[2].split(","):
        exts[j] = 0
    
    try:
        os.chdir(foldername)
    except OSError:
        sys.stderr.write("Please enter a vailid file path")
        exit()
    
    
    files = os.listdir()

    for i in files:
        if os.path.isfile(i):
            name = i.split(".")
            ext = name[len(name)-1]
            if ext in exts.keys():
               
                cDate = getTime(i)
                try:
                    
                    os.replace(i, cDate +"-"+i )
                    exts[ext] +=1
                except OSError:
                    sys.stderr.write("error while renameing {0}\n".format(i))
                    
                    
    for i in exts:
        print("{0} - renamed {1}".format(i,exts[i]))










    

else:
    sys.stderr.write("you did not enter the paramaters right. USAGE:python rename.py folder-name ext1,ext2,ext3,…")