import numpy as np

# ===============================
# 工具函式
# ===============================

def normalize(landmarks):
    landmarks = landmarks - landmarks[1]
    landmarks /= np.linalg.norm(landmarks, axis=1).mean()
    return landmarks

