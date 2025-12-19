import mediapipe as mp

print("mediapipe version:", mp.__version__)
print("Available attributes:", dir(mp))

# 嘗試初始化 FaceMesh
try:
    mesh = mp.solutions.face_mesh.FaceMesh(static_image_mode=True)
    print("FaceMesh initialized successfully!")
except Exception as e:
    print("Error:", e)
    print("Failed to initialize FaceMesh.")