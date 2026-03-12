import os
import socket
import time

# Send an empty udp broadcast every 10 seconds.
def broadcast():
    print(f'Broadcasting at {time.ctime()}')
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
    s.sendto(b'', ('192.168.0.228', 8875))
    s.close()


if __name__ == '__main__':
    while True:
        broadcast()
        time.sleep(10)