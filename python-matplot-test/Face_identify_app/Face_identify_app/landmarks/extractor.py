import mediapipe as mp
# from mediapipe.tasks.python import vision
# from mediapipe.tasks.python.core import BaseOptions  # ✅ 修正匯入
import os
import cv2
import time
import numpy as np
import Face_identify_app

BaseOptions = mp.tasks.BaseOptions
FaceLandmarker = mp.tasks.vision.FaceLandmarker
FaceLandmarkerOptions = mp.tasks.vision.FaceLandmarkerOptions
VisionRunningMode = mp.tasks.vision.RunningMode
Image = mp.Image
ImageFormat = mp.ImageFormat

#model_path = os.path.join(os.path.dirname(__file__), "..", "models", "face_landmarker.task")


class FaceLandmarkExtractor:
    #def __init__(self, max_faces=1): 
    def __init__(self, max_faces=1, model_path=".\\models\\face_landmarker.task"): 
        self.max_faces = max_faces

        options = FaceLandmarkerOptions(
            base_options=BaseOptions(model_asset_path=str(model_path)),
           #  running_mode=VisionRunningMode.LIVE_STREAM,
            running_mode=VisionRunningMode.VIDEO,  # VIDEO 模式同步抓結果
            num_faces=max_faces,
            min_face_detection_confidence=0.5,
            min_tracking_confidence=0.5,
        )

        self.landmarker = FaceLandmarker.create_from_options(options)

    def extract(self, frame):
       # mediapipe Tasks API 要求 RGB 輸入
        rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        mp_image = Image(image_format=ImageFormat.SRGB, data=rgb)
       
        # 執行臉部 landmarks 偵測
        timestamp = int(time.time() * 1000)
        result = self.landmarker.detect_for_video(mp_image, timestamp_ms=timestamp)

        if not result.face_landmarks:
            return []

        faces = []
        for face in result.face_landmarks:
            landmarks = np.array([[lm.x, lm.y, lm.z] for lm in face])
            faces.append(landmarks)

        return faces



       # result = self.mesh.process(rgb)
        # if not result.multi_face_landmarks:
        #     return []
        # return [
        #     np.array([[lm.x, lm.y, lm.z] for lm in face.landmark])
        #     for face in result.multi_face_landmarks
        # ]

    def close(self):
        self.landmarker.close()