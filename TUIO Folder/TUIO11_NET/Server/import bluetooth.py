import bluetooth
import socket

# File to store recognized Bluetooth devices
file_path = "bluetooth_devices.txt"

# Function to load existing devices from the file
def load_devices(file_path):
    devices = set()
    try:
        with open(file_path, "r", encoding="utf-8") as file:
            for line in file:
                name, addr = line.strip().split(" - ")
                devices.add((name, addr))
    except FileNotFoundError:
        # If the file does not exist, initialize it
        open(file_path, "w", encoding="utf-8").close()
    return devices

# Function to save a new device to the file
def save_device(file_path, name, addr):
    with open(file_path, "a", encoding="utf-8") as file:
        file.write(f"{name} - {addr}\n")

# Discover nearby Bluetooth devices
nearby_devices = bluetooth.discover_devices(lookup_names=True)

# Load previously stored devices
known_devices = load_devices(file_path)

# Determine if there are any new devices
new_device_found = False
for addr, name in nearby_devices:
    if (name, addr) not in known_devices:
        new_device_found = True
        save_device(file_path, name, addr)  # Save the new device
        print(f"{name} - {addr}: New device found, will send 'signup'.")
        

# Set the message based on whether new devices were found
message = "signup" if new_device_found else "login"

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
        
        # Send the single message ("login" or "signup")
        conn.sendall(message.encode())
        print(f"Message '{message}' sent to client.")
