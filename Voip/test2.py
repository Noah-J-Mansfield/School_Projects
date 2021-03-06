import alsaaudio # use sudo apt install python3-pyaudio to get this module
import wave
import socket
import time
from threading import Thread, Lock
from datetime import datetime
import re
import os
import sys, select, tty, termios


b_port = 5000 #broad cast port
c_port = 8000 #communication port


MAX_BYTES = 100
running = True


in_call_lock = Lock()



class B_Thread(Thread):
    handle = "nmans433"
    start_time = datetime.now()
    com_handles = {}
    command = ["",(0,0)]
    id_number = 0
    message = ""
    message_recieved = ""
    status = ""
    in_call = False
    lock = Lock()
    call_address = (0,0)
    incoming_cal  = False
    incoming_cal_address = (0,0)
    #recieved[3] = ["",(0,0),""]

    def __init__(self):
        super().__init__()
        self.start_time = datetime.now()
        B_Thread.com_handles.clear()
       
        
    def millis(self):
        dt = datetime.now() - self.start_time
        ms = (dt.days * 24 * 60 * 60 + dt.seconds) * 1000 + dt.microseconds / 1000.0
        return ms   

    def com_print(self):
        os.system('clear')
        with B_Thread.lock:
            l = list(B_Thread.com_handles.keys())
            l.sort()
            print("Active Devices:")
            for i in l:
                print(str(B_Thread.com_handles[i][2]) + ": " + i)
                
        with B_Thread.lock:
    
            print("\nstatus: " + B_Thread.status)
            print("message sent: " + B_Thread.message)
            print("message recieved: " + B_Thread.message_recieved)
        print("\nhelp:")
        print("--To call type 'c' 'id_number' followed by enter\n--To accept a call type 'a' followed by enter\n--To end a call type 'h' followed by enter\n--To terminate the program type 'e' followed by enter")
        with in_call_lock:
            print("IN CALL == "+str(B_Thread.in_call))
            print("Running == "+str(running))

    def handle_response(self, data, address):
        data = data.decode()
        op = re.search("[a-zA-Z]*",data)
        if op == None:
            return
        re_handle = str(data[data.find("@")+1:])
        
            
        if op.group(0) == "call":
                
            with B_Thread.lock:
                with in_call_lock:   
                    if B_Thread.in_call == False:
                        B_Thread.status = "incoming call from "+re_handle

                B_Thread.incoming_cal_address = address
                if B_Thread.command[0].find("hangup") != 0:
                    B_Thread.incoming_cal = True

        if op.group(0) == "accept":
            with B_Thread.lock:
                
                B_Thread.status  = "Call in progress"
                with in_call_lock:
                    if B_Thread.command[0].find("hangup") == 0:
                        B_Thread.in_call = False
                    else:
                        B_Thread.in_call = True
                B_Thread.call_address = address

        if op.group(0) == "hangup":
            with B_Thread.lock: 
                B_Thread.status = "Awaiting call"
                with in_call_lock:
                    B_Thread.in_call = False
                B_Thread.command[0] = ""

        if op.group(0) == "reg":
            flag = True
            if re_handle in B_Thread.com_handles:
                flag = False
            if flag and re_handle != B_Thread.handle:
                with B_Thread.lock:
                        
                    B_Thread.com_handles[re_handle] = (data, address, B_Thread.id_number)
                    B_Thread.id_number+=1

    def run(self):
        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        sock.bind(('',b_port))
        sock.setsockopt(socket.SOL_SOCKET,socket.SO_BROADCAST,1)
        sock.setblocking(False)
        start = self.millis()
        prev_elapsed_time = start
        handle = B_Thread.handle.encode()
        while running:

            try:
                data, add = sock.recvfrom(MAX_BYTES)
                if len(data.decode()) > 0:
                    
                    self.handle_response(data, add)
                  
                    with B_Thread.lock:
                        B_Thread.message_recieved = data.decode()
            except:
                pass
            
        
            elapsed_time = self.millis() - start
            
            if elapsed_time - prev_elapsed_time > 1000:
                with B_Thread.lock:
                    if len(B_Thread.command[0]) > 0 and B_Thread.command[1] != (0,0):
                        B_Thread.message = B_Thread.command[0]
                  
                        sock.sendto(B_Thread.command[0].encode(), B_Thread.command[1])
                        
                        if B_Thread.command[0].find("hangup") == 0:
                            B_Thread.command[0] = ""
                            B_Thread.status = "Awaiting call"
                      
                    else:   
                        
                        sock.sendto(b"reg@"+handle,('<broadcast>', b_port))
                        B_Thread.message = "reg@"+handle.decode()
                        
                self.com_print()
                prev_elapsed_time = elapsed_time

        sock.close()

        

class ListenThread(Thread):

    def __init__(self):
        
        super().__init__()
  
        self.main_sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.main_sock.bind(('', c_port))
      
        self.address = (0,0)

        self.device_r = alsaaudio.PCM(alsaaudio.PCM_PLAYBACK,alsaaudio.PCM_NONBLOCK, device='default') # alsaaudio.PCM_NONBLOCK,
        self.device_r.setchannels(1)
        self.device_r.setrate(sample_rate)
        self.device_r.setformat(alsaaudio.PCM_FORMAT_S16_LE) # 16 bit little endian frames
        self.device_r.setperiodsize(period_size)
 
        self.device_s = alsaaudio.PCM(alsaaudio.PCM_CAPTURE,device='default')
        self.device_s.setchannels(1)
        self.device_s.setrate(sample_rate)
        self.device_s.setformat(alsaaudio.PCM_FORMAT_S16_LE) # 16 bit little endian frames
        self.device_s.setperiodsize(period_size)
        
       
        self.size_to_rw = period_size * 2
      

    def run(self):
        flag = True
        while running:
                in_call_lock.acquire()
                
                while B_Thread.in_call:
                    in_call_lock.release()
                    
                    if flag:
                        flag = False
                        self.address = B_Thread.call_address
                 
                    numframes, data_s = self.device_s.read()
                    #print("recorded "+str(len(data_s))+" Bytes")
                    
                    self.main_sock.sendto(data_s, (self.address[0], c_port))
                   
                    data = self.main_sock.recv(self.size_to_rw)
                    #print("recieved "+str(len(data))+" Bytes")
                    if data:
                        self.device_r.write(data)
                    
                    in_call_lock.acquire()
                in_call_lock.release()    
                flag = True
                
        print("----------------------DEAD----------------------")
        in_call_lock.release()
        self.main_sock.close()
        self.device_s.close()
        self.device_r.close()

#class KeyboardInput(object):
#    def __enter__(self):
#        self.old_settings = termios.tcgetattr(sys.stdin)


try:
    sample_rate = int(sys.argv[1])
    period_size = int(sys.argv[2])
    B_Thread.handle =  sys.argv[3]
    B_Thread.status = "Awaiting call"
    broadcast = B_Thread()
    listen = ListenThread()
 
    broadcast.start()
    listen.start()
   
    while running:
        cmd = input()
        if len(cmd) < 1:
            pass
            
        else:
       
            if cmd[0] == "c":
                
                with B_Thread.lock:

                    B_Thread.command[0] = "call@"+B_Thread.handle
                
                    for han in B_Thread.com_handles:
                       
                        if B_Thread.com_handles[han][2] == int(cmd[2]): #if id equals number given by user
                            
                            B_Thread.command[1] = B_Thread.com_handles[han][1]
                            
                            
                            B_Thread.status = "Calling " + han
                           
                            B_Thread.call_address = B_Thread.com_handles[han][2]
            
            if cmd[0] == 'a':
               
                with B_Thread.lock:
                    
                   
                    B_Thread.command[0] = "accept@"+B_Thread.handle
                    B_Thread.command[1] = B_Thread.incoming_cal_address
                    B_Thread.status = "Call in progress"
                  

                    B_Thread.call_address = B_Thread.incoming_cal_address
                    B_Thread.in_call = True
                    B_Thread.incoming_cal = False
                    

            if cmd[0] == 'h':
                with B_Thread.lock:
                    
                    B_Thread.command[0] = "hangup@"+B_Thread.handle
                     
                 
                    B_Thread.status = "Awaiting call"
                    if B_Thread.incoming_cal:
                        
                        B_Thread.command[1] = B_Thread.incoming_cal_address
                        B_Thread.incoming_cal = False
                        B_Thread.in_call = False
                    elif B_Thread.in_call:
                       
                        B_Thread.command[1] = B_Thread.call_address
                        B_Thread.in_call = False
                    
            if cmd[0] == "e":
                running = False
                with in_call_lock:
                    B_Thread.in_call = False
                
except:
    running = False

            

