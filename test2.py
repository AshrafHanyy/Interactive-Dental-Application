import socket
import cv2
import mediapipe as mp
from dollarpy import Recognizer, Template, Point
import csv
import time

# Initialize MediaPipe hands
mp_drawing = mp.solutions.drawing_utils
mp_hands = mp.solutions.hands

# Load templates from CSV for recognizer
def load_templates_from_csv(csv_file_path):
    loaded_templates = []
    current_gesture = None
    current_points = []
    
    # Open the CSV and read each row
    with open(csv_file_path, mode='r') as file:
        reader = csv.reader(file)
        next(reader)  # Skip header row
        
        for row in reader:
            gesture_name, point_index, x, y = row[0], int(row[1]), float(row[2]), float(row[3])
            point = Point(x, y)
            
            # Check if we're still on the same gesture
            if current_gesture != gesture_name:
                # If not, save the previous gesture's template if it exists
                if current_gesture is not None:
                    loaded_templates.append(Template(current_gesture, current_points))
                # Start a new gesture template
                current_gesture = gesture_name
                current_points = [point]
            else:
                # Continue adding points to the current gesture template
                current_points.append(point)
        
        # Add the last gesture template
        if current_gesture is not None:
            loaded_templates.append(Template(current_gesture, current_points))
    
    print("Templates loaded from CSV:", len(loaded_templates))
    return loaded_templates

# Initialize recognizer and load templates
csv_file_path = 'hand_gesture_templates3.csv'
loaded_templates = load_templates_from_csv(csv_file_path)
recognizer = Recognizer(loaded_templates)

# Initialize the socket connection
def initialize_socket():
    soc = socket.socket()
    hostname = "localhost"  # 127.0.0.1 can also be used
    port = 65434
    soc.bind((hostname, port))
    soc.listen(1)
    print("Waiting for connection...")
    conn, addr = soc.accept()
    print("Device connected:", addr)
    return conn

# Send message through the socket
def send_message(conn, message):
    try:
        encoded_msg = message.encode('utf-8')
        conn.send(encoded_msg)
    except Exception as e:
        print("Failed to send message:", e)

# Use live camera feed for prediction and display in terminal
def recognize_from_camera(conn):
    cap = cv2.VideoCapture(0)
    if not cap.isOpened():
        print("Error: Could not open video feed.")
        return  # Exit the function if the camera could not be opened

    accumulated_points = []  
    frame_count = 0
    
    with mp_hands.Hands(min_detection_confidence=0.5, min_tracking_confidence=0.5) as hands:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break
            
            # Process frame
            frame = cv2.flip(frame, 1)
            image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            results = hands.process(image)
            
            if results.multi_hand_landmarks:
                for hand_landmarks, handedness in zip(results.multi_hand_landmarks, results.multi_handedness):
                    hand_label = handedness.classification[0].label  # 'Left' or 'Right'
                    
                    # Extract points from the detected hand
                    points = [Point(hand_landmarks.landmark[i].x, hand_landmarks.landmark[i].y) for i in range(21)]
                    accumulated_points.extend(points)
                    frame_count += 1
                   
                    if frame_count == 10:
                        start_time = time.time()
                        result = recognizer.recognize(accumulated_points)
                        end_time = time.time()
                        
                        # Handle result based on its type
                        if isinstance(result, list) and len(result) > 0 and hasattr(result[0], 'name'):
                            gesture_name = result[0].name
                            print(f"Hand: {hand_label} | Recognized Gesture: {gesture_name} | Score: {result[0].score} | Time taken: {end_time - start_time:.2f}s")
                            send_message(conn, gesture_name)  # Send gesture over socket
                        else:
                            print(f"Hand: {hand_label} | Recognized Gesture: {result} | Time taken: {end_time - start_time:.2f}s")
                            send_message(conn, str(result[0]))  # Send the result as string over socket
                        
                        # Reset the accumulator
                        accumulated_points = []
                        frame_count = 0
                    
                    # Draw landmarks on the video for reference
                    mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)
            
            # Display the video feed
            cv2.imshow("Hand Gesture Recognition", frame)
            if cv2.waitKey(10) & 0xFF == ord('q'):
                send_message(conn, "exit")  # Notify the connected device to exit
                break
    
    cap.release()
    cv2.destroyAllWindows()

# Main function to start recognition with socket integration
def main():
    conn = initialize_socket()  # Initialize socket connection
    try:
        recognize_from_camera(conn)  # Start gesture recognition with socket
    finally:
        conn.close()  # Ensure the connection is closed on exit

# Start the program
if __name__ == "__main__":
    main()
