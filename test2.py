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

# Use live camera feed for prediction and display in terminal
def recognize_from_camera():
    cap = cv2.VideoCapture(0)
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
                   
                    if frame_count == 50:
                        start_time = time.time()
                        result = recognizer.recognize(accumulated_points)
                        end_time = time.time()
                        
                        # Handle result based on its type
                        if isinstance(result, list) and len(result) > 0 and hasattr(result[0], 'name'):
                            # If result has name and score attributes
                            print(f"Hand: {hand_label} | Recognized Gesture: {result[0].name} | Score: {result[0].score} | Time taken: {end_time - start_time:.2f}s")
                        else:
                            print(f"Hand: {hand_label} | Recognized Gesture: {result} | Time taken: {end_time - start_time:.2f}s")
                        
                        # Reset the accumulator
                        accumulated_points = []
                        frame_count = 0
                    
                    # Draw landmarks on the video for reference
                    mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)
            
            # Display the video feed
            cv2.imshow("Hand Gesture Recognition", frame)
            if cv2.waitKey(10) & 0xFF == ord('q'):
                break
    
    cap.release()
    cv2.destroyAllWindows()

# Start the recognition from camera
recognize_from_camera()
