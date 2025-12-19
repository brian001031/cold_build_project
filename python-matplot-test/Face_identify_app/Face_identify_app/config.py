DB_PATH = "face_db"
MIN_STABLE_FRAMES = 7   # 約 0.25 秒穩定
BLUR_THRESHOLD = 41    # Laplacian sharpness , webcam 實際可達
SAVE_INTERVAL = 3.0     # 秒（避免每幀都存）
THRESHOLD_K = 2.5       # 臉面積篩選
FPS_MIN = 10            # 新增：最低 FPS

