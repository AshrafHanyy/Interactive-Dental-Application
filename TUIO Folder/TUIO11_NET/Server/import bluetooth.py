import bluetooth
import socket
# to install: pip install git+https://github.com/pybluez/pybluez.git#egg=pybluez

file_path = "bluetooth_devices.txt"
def load_devices(file_path):
    devices = []
    try:
        with open(file_path, "r", encoding="utf-8") as file:
            for line in file:
                name, addr = line.strip().split(" - ")
                devices.append((name, addr))
    except FileNotFoundError:
        # If the file does not exist, initialize it
        open(file_path, "w", encoding="utf-8").close()
    return devices

def save_device(file_path, name, addr):
    with open(file_path, "a", encoding="utf-8") as file:
        file.write(f"{name} - {addr}\n")

known_devices = load_devices(file_path)
print("Known Devices:", known_devices)

admin_device = known_devices[0] if known_devices else None
print("Admin Device:", admin_device)

nearby_devices = bluetooth.discover_devices(lookup_names=True)
print("Nearby Devices:", nearby_devices)

# Convert nearby_devices to tuples of (name, address) for consistent comparison
nearby_devices_tuples = [(name, addr) for addr, name in nearby_devices]
print("Nearby Devices (Formatted):", nearby_devices_tuples)

# Check if the admin device is nearby
admin_found = admin_device in nearby_devices_tuples

# Set message based on the presence of the admin device
if admin_found:
    admin_name, _ = admin_device
    message = f"admin '{admin_name}'"
else:
    new_device_found = False
    for addr, name in nearby_devices:
        if (name, addr) not in known_devices:
            new_device_found = True
            save_device(file_path, name, addr)  
            message = f"signup '{name}'"
            print(f"{name} - {addr}: New device found, will send '{message}'.")
            break

    if not new_device_found:
        message = f"login '{nearby_devices[0][1]}'"  

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
        
        # Send the single message (with the device name included)
        conn.sendall(message.encode())
        print(f"Message '{message}' sent to client.")