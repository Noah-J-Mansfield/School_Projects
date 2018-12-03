import alsaaudio # use sudo apt install python3-pyaudio to get this module
import wave
import socket
import sys
import time
from threading import Thread, Lock
from datetime import datetime
import re

com_handles_lock = Lock()

b_port = 5000 #broad cast port
c_port = 8000 #communication port


MAX_BYTES = 100
running = True




class MyThread(Thread):
    handle = "TESTING"
    start_time = datetime.now()
    r_call = False
    r_accept = False
    r_hangup = False
    com_handles = [()]
    s_call = False
    s_accept = False
    s_hangup = False

    def __init__(self):
        super().__init__()
        self.start_time = datetime.now()
        MyThread.com_handles.clear()
        print(len(MyThread.com_handles))
        
    def millis(self):
        dt = datetime.now() - self.start_time
        ms = (dt.days * 24 * 60 * 60 + dt.seconds) * 1000 + dt.microseconds / 1000.0
        return ms   

    def com_print(self):
       
        with com_handles_lock:
            for i in MyThread.com_handles:
                print('\f'+i[0])

    def handle_response(self, data, address):
            data = data.decode()
            op = re.search("[a-zA-Z]*",data)
            if op == None:
                return
            
            
            if op.group(0) == "call":
                MyThread.r_call = True
            if op.group(0) == "accept":
                MyThread.r_accept = True
            if op.group(0) == "hangup":
                MyThread.r_hangup = True
            if op.group(0) == "reg":
                flag = True
                if len(MyThread.com_handles) > 0:
                    for i in MyThread.com_handles:
                        if i[0] == data:
                            flag = False
                if flag:
                    with com_handles_lock:
                       
                        MyThread.com_handles.append((data, address))


    def run(self):
        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        sock.bind(('',b_port))
        sock.setsockopt(socket.SOL_SOCKET,socket.SO_BROADCAST,1)
        sock.setblocking(False)
        start = self.millis()
        prev_elapsed_time = start

        while running:
            handle = MyThread.handle.encode()
            if MyThread.s_call:
                sock.sendto(b"call@"+handle,('<broadcast>', b_port))
               
            elif MyThread.s_accept:
                sock.sendto(b"accept@"+handle,('<broadcast>', b_port))
                MyThread.s_accept = False
            elif MyThread.s_hangup:
                sock.sendto(b"hangup@"+handle,('<broadcast>', b_port))
                MyThread.s_hangup = False
            else:
                sock.sendto(b"reg@"+handle,('<broadcast>', b_port))
            data, add = sock.recvfrom(MAX_BYTES)
            if len(data.decode()) > 0:
                
                self.handle_response(data, add)
            
            elapsed_time = self.millis() - start
            
            if elapsed_time - prev_elapsed_time > 1000:
                self.com_print()
                prev_elapsed_time = elapsed_time
        sock.close()

broadcast = MyThread()
broadcast.start() 

try:
    while running:
        pass
except:
    running = False

            


