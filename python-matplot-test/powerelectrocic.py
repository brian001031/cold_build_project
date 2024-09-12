import numpy as np                                        # 引入科學計算函式庫
import matplotlib.pyplot as plt                           # 引入繪圖函式庫

xmin, xmax, num = 0.01, 10, 100                           # 設定繪圖範圍、取點數
x = np.linspace(xmin, xmax, num)                          # 產生x
Q = 1E-4
q = 1E-4
k = 8.988E9
F = k * Q * q / x**2                                      # 靜電力F
U = k * Q * q / x                                         # 電位能U

plt.figure(figsize = (6, 4.5), dpi = 100)                 # 設定圖片尺寸
plt.xlabel('r (m)', fontsize = 16)                        # 設定坐標軸標籤
plt.xticks(fontsize = 12)                                 # 設定坐標軸數字格式
plt.yticks(fontsize = 12)
plt.grid(color = 'red', linestyle = '--', linewidth = 1)  # 設定格線顏色、種類、寬度
plt.ylim(0, 200)                                          # 設定y軸繪圖範圍
# 繪圖並設定線條顏色、寬度、圖例
line1, = plt.plot(x, F, color = 'red', linewidth = 3, label = 'Electric Force (N)')             
line2, = plt.plot(x, U, color = 'blue', linewidth = 3, label = 'Electric Potential Energy (J)')
plt.legend(handles = [line1, line2], loc='upper right')
plt.savefig('Fe_r_plot.svg')                              # 儲存圖片
plt.savefig('Fe_r_plot.png')
plt.show()                                                # 顯示圖片