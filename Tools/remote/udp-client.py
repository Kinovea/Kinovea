import os
import socket
import time

# Every 10 seconds send an empty udp broadcast to 192.168.1.255 port 8875
def broadcast():
    print(f'Broadcasting at {time.ctime()}')
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
    #s.sendto(b'', ('192.168.1.255', 8875))
    s.sendto(b'', ('127.0.0.1', 8870))
    s.close()


if __name__ == '__main__':
    while True:
        broadcast()
        time.sleep(5)