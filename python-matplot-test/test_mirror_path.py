import os

# 假設網路磁碟已經映射為 Z:\
network_drive_path = r"Z:\\TrayCellData\\R_bak_HTBI\\CC"

# 檢查該資料夾是否存在
if os.path.exists(network_drive_path):
    print(f"資料夾 {network_drive_path} 存在！")
else:
    print(f"資料夾 {network_drive_path} 不存在。")

# 列出該資料夾內的檔案
files = os.listdir(network_drive_path)
print("資料夾中的檔案：", files)