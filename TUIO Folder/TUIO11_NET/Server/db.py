#pip install firebase-admin
import firebase_admin
from firebase_admin import credentials, firestore

# Path to the service account key JSON file and connect to the db
cred = credentials.Certificate("dentalapp-87611-firebase-adminsdk-9li4a-7c66415331.json")
firebase_admin.initialize_app(cred)
db = firestore.client()

#--------------------------Devices CRUD--------------------------
# Create a document for the device and save the data
#device_MAC = "NFIENF-vvfnrin-rgk"
#device_name = "Device123"
#device_role = "student"
#face_coordinates = {"x": 34.5, "y": 56.7}
#progress = "50%"
#doc_ref = db.collection("devices").document(device_MAC)
#doc_ref.set({
#    "MAC":device_MAC,
#    "device_name": device_name,
#    "role": device_role, 
#    "face_coordinates": face_coordinates,
#    "progress": progress
#})

# Read a document for the device
#device_MAC = "NFIENF-vvfnrin-rgk"
#doc_ref = db.collection("devices").document(device_MAC)
#doc = doc_ref.get()
#data = doc.to_dict()
#MAC = data.get("MAC", None)
#device_name = data.get("device_name", None)
#device_role = data.get("role", None)
#face_coordinates = data.get("face_coordinates", None)
#progress = data.get("progress", None)

#Update a document for the device
#device_MAC = "NFIENF-vvfnrin-rgk"
#doc_ref = db.collection("devices").document(device_MAC)
#doc_ref.update({
#    "progress": "75%",  # Update progress to 75%
#    "role": "instructor",  # Update role
#})

# Delete the document for the specified MAC address
#device_MAC = "NFIENF-vvfnrin-rgk"
#doc_ref = db.collection("devices").document(device_MAC)
#doc_ref.delete()

