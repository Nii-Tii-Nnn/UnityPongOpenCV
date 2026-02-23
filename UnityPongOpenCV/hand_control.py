import cv2
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
import socket
import urllib.request
import os

# ---------------- DOWNLOAD MODEL ----------------
MODEL_PATH = "hand_landmarker.task"
MODEL_URL = "https://storage.googleapis.com/mediapipe-models/hand_landmarker/hand_landmarker/float16/latest/hand_landmarker.task"

if not os.path.exists(MODEL_PATH):
    print("Downloading hand landmarker model...")
    urllib.request.urlretrieve(MODEL_URL, MODEL_PATH)
    print("Model downloaded.")

# ---------------- SOCKET ----------------
HOST = "127.0.0.1"
PORT = 5050
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((HOST, PORT))

# ---------------- MEDIAPIPE TASKS ----------------
base_options = python.BaseOptions(model_asset_path=MODEL_PATH)
options = vision.HandLandmarkerOptions(
    base_options=base_options,
    num_hands=2,
    min_hand_detection_confidence=0.7,
    min_tracking_confidence=0.7
)
detector = vision.HandLandmarker.create_from_options(options)

# ---------------- CAMERA ----------------
cap = cv2.VideoCapture(0)

FINGER_TIPS = [8, 12, 16, 20]

# Hand landmark connections for drawing
HAND_CONNECTIONS = [
    (0, 1), (1, 2), (2, 3), (3, 4),
    (0, 5), (5, 6), (6, 7), (7, 8),
    (5, 9), (9, 10), (10, 11), (11, 12),
    (9, 13), (13, 14), (14, 15), (15, 16),
    (13, 17), (17, 18), (18, 19), (19, 20),
    (0, 17)
]


def count_fingers(landmarks, handedness):
    fingers = 0

    # Thumb
    if handedness == "Right":
        if landmarks[4].x < landmarks[3].x:
            fingers += 1
    else:
        if landmarks[4].x > landmarks[3].x:
            fingers += 1

    # Other fingers
    for tip in FINGER_TIPS:
        if landmarks[tip].y < landmarks[tip - 2].y:
            fingers += 1

    return fingers


def draw_landmarks(frame, landmarks, w, h):
    """Draw hand landmarks on the frame"""
    points = [(int(lm.x * w), int(lm.y * h)) for lm in landmarks]
    
    # Draw connections
    for connection in HAND_CONNECTIONS:
        start_idx, end_idx = connection
        cv2.line(frame, points[start_idx], points[end_idx], (0, 255, 0), 2)
    
    # Draw landmarks
    for point in points:
        cv2.circle(frame, point, 5, (255, 0, 0), -1)


# ---------------- MAIN LOOP ----------------
while True:
    ret, frame = cap.read()
    if not ret:
        break

    frame = cv2.flip(frame, 1)
    h, w, _ = frame.shape
    rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

    # Convert to MediaPipe Image
    mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb)
    result = detector.detect(mp_image)

    p1_cmd = "P1_DOWN"
    p2_cmd = "P2_DOWN"

    cv2.line(frame, (w // 2, 0), (w // 2, h), (0, 255, 0), 2)

    if result.hand_landmarks:
        for i, hand_landmarks in enumerate(result.hand_landmarks):
            draw_landmarks(frame, hand_landmarks, w, h)

            handedness = result.handedness[i][0].category_name
            fingers = count_fingers(hand_landmarks, handedness)

            palm_x = int(hand_landmarks[9].x * w)

            if palm_x < w // 2:
                p1_cmd = "P1_UP" if fingers == 5 else "P1_DOWN"
                cv2.putText(frame, f"P1: {fingers}",
                            (10, 40), cv2.FONT_HERSHEY_SIMPLEX,
                            1, (255, 0, 0), 2)
            else:
                p2_cmd = "P2_UP" if fingers == 5 else "P2_DOWN"
                cv2.putText(frame, f"P2: {fingers}",
                            (w//2 + 10, 40), cv2.FONT_HERSHEY_SIMPLEX,
                            1, (0, 0, 255), 2)

    sock.sendall((p1_cmd + "\n").encode())
    sock.sendall((p2_cmd + "\n").encode())

    cv2.imshow("Hand Controlled Pong", frame)

    if cv2.waitKey(1) & 0xFF == 27:
        break

cap.release()
detector.close()
sock.close()
cv2.destroyAllWindows()
