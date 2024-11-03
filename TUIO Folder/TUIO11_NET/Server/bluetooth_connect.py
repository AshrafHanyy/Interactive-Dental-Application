import bluetooth
import socket

# Discover nearby Bluetooth devices and write to a file
nearby_devices = bluetooth.discover_devices(lookup_names=True)
with open("bluetooth_devices.txt", "w") as file:
    for addr, name in nearby_devices:
        file.write(f"{name} - {addr}\n")

# Set up a socket server
HOST = '127.0.0.1'  # Localhost for testing; use your actual IP if needed
PORT = 65432        # Port to listen on (non-privileged ports > 1023)

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server_socket:
    server_socket.bind((HOST, PORT))
    server_socket.listen()
    print("Waiting for a connection...")

    conn, addr = server_socket.accept()
    with conn:
        print('Connected by', addr)
        
        # Read and send the Bluetooth data
        with open("bluetooth_devices.txt", "r") as file:
            data = file.read()
        
        # Send data in chunks to avoid issues with buffer limits
        conn.sendall(data.encode())
        print("Data sent to client.")