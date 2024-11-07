import matplotlib.pyplot as plt
import matplotlib.ticker as ticker
import matplotlib
import numpy as np

matplotlib.use('Agg')

#print('check self first')

N = 1000
fig_all = plt.figure(figsize=(20,4))

#plot 1:
xpoints = np.array([0, .5 ,1025])
ypoints = np.array([0, 5 ,256])

data_avg = xpoints / 20
fig = plt.subplot(1, 2, 1)
ax1 = plt.subplot(1, 2, 1)
ax2 = plt.subplot(1, 2, 1)
plt.title('number of blocks NN')
plt.xlabel('Game')
#plt.plot(xpoints,ypoints)
#fig = ax1.twinx()
ax2 = ax1.twinx()


ax1.set_ylabel('Opened Blocks',color='tab:blue')
ax1.plot(range(len(xpoints)),xpoints,color='tab:blue', alpha=0.75)
ax1.tick_params(axis='y',labelcolor= 'tab:blue')

ax2.set_ylabel('Opened Blocks (Avg of 250 Games)', color='black')
ax2.plot([ 250 * (i + 1) for i in range(len(data_avg))], data_avg, color='black', alpha=1)
ax2.tick_params(axis='y', labelcolor='black')
plt.tight_layout()
# plt.gca().margins(x=0)
# plt.gcf().canvas.draw()
# t1= plt.gca().get_xticklabels()

#調整X軸放縮比例
# maxsize = 1
# m = 0.2 #inch margin
# s = maxsize / plt.gcf().dpi * N + 2 *m
# margin = m / plt.gcf().get_size_inches()[0]
# plt.gcf().subplots_adjust(left = margin , right = 1.-margin)
# plt.gcf().set_size_inches(s,plt.gcf().get_size_inches()[1])

# plt.xticks(np.linspace(0,1000,500)) # 設置刻度間距: 表示0~1000 之間分成五份




#plt.savefig('./test-plot 1.jpg')

#plot 2:
# x = np.array([1, 4, 16, 25])
# y = np.array([2, 8, 32, 128])

# plt.subplot(1, 2, 2)
# plt.plot(x,y)
# plt.title("plot 2")

#調整X軸放縮比例
# maxsize = 1
# m = 0.2 #inch margin
# s = maxsize / plt.gcf().dpi * N + 2 *m
# margin = m / plt.gcf().get_size_inches()[0]
# plt.gcf().subplots_adjust(left = margin , right = 1.-margin)
# plt.gcf().set_size_inches(s,plt.gcf().get_size_inches()[1])

# plt.xticks(np.linspace(0,1000,500)) # 設置刻度間距: 表示0~1000 之間分成五份


#plt.suptitle("RUNOOB subplot Test")


#plt.legend(['plot 1','plot 2'])
plt.savefig('./test-plot-suptitle.jpg')

plt.show()