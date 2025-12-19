# examples/run_camera.py
import cv2
from Face_identify_app.landmarks.extractor import FaceLandmarkExtractor
import Face_identify_app.config as config
from Face_identify_app.landmarks.normalize import normalize
# from Face_identify_app.features.raw import extract_feature
# from Face_identify_app.clustering.assign import assign
# from Face_identify_app.utils.vision import blur_score
import time
import numpy as np
from pathlib import Path
from mediapipe.tasks.python import vision



# ===============================
# 設定
# ===============================
MODEL_PATH = ".\\models\\face_landmarker.task"
SAVE_DIR = Path(r"c:\captures")
SAVE_DIR.mkdir(parents=True, exist_ok=True)

MAX_FACES = 3
SAVE_INTERVAL = config.SAVE_INTERVAL
BLUR_THRESHOLD = config.BLUR_THRESHOLD

best_face = None
best_area = 0.0


# MediaPipe FaceMesh 精簡索引----start--------------------------
#簡化 landmarks（468 ➜ 55）
FACE_OUTLINE = [
    10, 338, 297, 332, 284, 251, 389, 356,
    454, 323, 361, 288, 397, 365, 379, 378, 400
]

LEFT_EYE = [33, 160, 158, 133, 153, 144]
RIGHT_EYE = [362, 385, 387, 263, 373, 380]

NOSE = [1, 2, 98, 327, 168, 197]

MOUTH = [
    61, 146, 91, 181, 84, 17, 314, 405,
    321, 375, 291, 308, 324, 318, 402, 317
]

SIMPLIFIED_INDEX = (
    FACE_OUTLINE + LEFT_EYE + RIGHT_EYE + NOSE + MOUTH
)

def simplify_landmarks(landmarks ,factor=2 , margin=0.01):
    # 精簡 landmarks（468 ➜ 234）
    #return landmarks[::factor]
    # 精簡 landmarks（468 ➜ 55）
    #return landmarks[SIMPLIFIED_INDEX]

    """
    landmarks: np.array (468,3) 原始 landmarks
    simplified_index: list[int] 55 個索引
    return: np.array (~110,3) 左右對稱
    """
    selected = landmarks[SIMPLIFIED_INDEX]  # 55 點
   # 臉左右邊界
    x_min = selected[:, 0].min()
    x_max = selected[:, 0].max()   
    cx = (x_min + x_max) / 2
    # 計算臉部質心 (X, Y)
   # cx = np.mean(selected[:, 0])
    cy = np.mean(selected[:, 1])

    # 對稱點
    mirrored = selected.copy()
    # 3*cx - x , 會把鏡像點 拉得過遠，有些點落在畫面外，經過 mask 過濾 → 只剩一半。
    # 2*cx - x，代表「相對臉中心對稱」
    mirrored[:, 0] = 2*cx - mirrored[:, 0]

    # 過濾鏡像超出臉邊界 margin 的點
    mask = (mirrored[:, 0] >= x_min - margin) & (mirrored[:, 0] <= x_max + margin)
    mirrored_filtered = mirrored[mask]
    combined = np.vstack([selected, mirrored_filtered])
    # 不再過濾，全部保留
    #combined = np.vstack([selected, mirrored])
    return combined

# MediaPipe FaceMesh 精簡索引----end--------------------------

#模糊度計算
def blur_score(gray):
    return cv2.Laplacian(gray, cv2.CV_64F).var()

# 繪製標記點
def draw_landmarks(frame, landmarks, color=(0,255,0)):
    h, w = frame.shape[:2]    
    for (x, y, _) in landmarks:
        cx, cy = int(x * w), int(y * h)
        cv2.circle(frame, (cx, cy), 2, color, -1)

# 計算臉部區域大小
def face_area(landmarks):
    xs = landmarks[:, 0]
    ys = landmarks[:, 1]
    return (xs.max() - xs.min()) * (ys.max() - ys.min())

# 主程式
def main():
 
 global best_face, best_area


 # 開啟攝影機
 cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)
 #VGA 640x480 HD 1280x720 
 cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1280)
 cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 720)

  # 初始化 FaceLandmarker (LIVE_STREAM 模式)
 extractor = FaceLandmarkExtractor(max_faces=MAX_FACES)

 last_save_time = 0.0
 prev_time = time.time()

 print("Press ESC to exit")

 while True:
    ret, frame = cap.read()
    if not ret:
        break

    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    sharpness = blur_score(gray)

    faces = extractor.extract(frame)

     #FPS計算
    current_time = time.time()
    fps = 1.0 / (current_time - prev_time)
    prev_time = current_time

    # 顏色池（最多 5 張臉）
    colors = [
        (0, 255, 0),
        (255, 0, 0),
        (0, 0, 255),
        (255, 255, 0),
        (255, 0, 255),
    ]

   
    #判別最每張臉部輪廓面積
    for i, landmarks in enumerate(faces):
        color = colors[i % len(colors)]

        simple_landmarks = simplify_landmarks(landmarks , factor=2 , margin=0.03)
        #取標準五官所有點數除於5,簡易 ----start--------------------------
        draw_landmarks(frame, simple_landmarks, color)         
        area = face_area(simple_landmarks)
        #--------------------------------end--------------------------

        #取全部468點,詳細
        #draw_landmarks(frame, landmarks, color)
        #area = face_area(landmarks)

        # 判斷最大面積
        if area > best_area:            
            best_area = area
            best_face = landmarks
            print(f"New best area: {area:.4f} (previous: {best_area:.4f})")
            print(f"best_face updated.")
            print(f"sharpness: {sharpness:.2f}")
    
    # 顯示資訊
    # ===============================
    # 自動存檔（最佳臉 + 清晰）
    # ===============================
    if best_face is not None and sharpness > BLUR_THRESHOLD and (current_time - last_save_time) > SAVE_INTERVAL:
        ts = time.strftime("%Y%m%d_%H%M%S")
        path = SAVE_DIR / f"face_{ts}.jpg"
        success = cv2.imwrite(str(path), frame)   
                 
        if success:
            last_save_time = current_time
            print(f"[SAVE] 已存檔: {path}")
        else:
            print(f"[ERROR] 存檔失敗: {path}")

    # UI overlay
    if current_time - last_save_time < 3.0:
        cv2.putText(
            frame,
            f"偵測FPS: {fps:.1f}",
            (20, 30),
            cv2.FONT_HERSHEY_SIMPLEX,
            0.8,
            (0, 255, 0),
            2,
        )
        # cv2.putText(
        #     frame,
        #     f"Faces: {len(faces)}  Sharpness: {sharpness:.0f}",
        #     (20, 60),
        #     cv2.FONT_HERSHEY_SIMPLEX,
        #     0.7,
        #     (0, 255, 255),
        #     2,
        # )

    cv2.imshow("人臉辨識(Face Identify)攝像頭camera :", frame)



    # for landmarks in extractor.extract(frame):
    #     landmarks = normalize(landmarks)
    #     # feat = extract_feature(landmarks)
    #     # group = assign(feat)
    #     # print("assigned:", group)
    #     print("assigned:", "skipped in this example")

    # cv2.imshow("cam", frame)

    if cv2.waitKey(1) == 27: # ESC key
        break

 # 迴圈外釋放資源
 cap.release()
 cv2.destroyAllWindows()
 extractor.close()

if __name__ == '__main__':
     main()