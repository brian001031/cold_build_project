import discord
import asyncio
import os
from dotenv import load_dotenv

# 載入 .env
load_dotenv()

# 取得 Discord Bot Token
MACHINE_STR = os.getenv("DISCORD_BOT")

# 要發送的頻道 ID (整數)
CHANNEL_ID = 1297720621609779341

intents = discord.Intents.default()
intents.guilds = True  # 只開這個就好

client = discord.Client(intents=intents)

@client.event
async def on_ready():
    print(f"✅ 已登入：{client.user}")
  

    try:
        channel = await client.fetch_channel(CHANNEL_ID)
        #🚀 Hi Bro !  Final Check Send this Myslef channel Reciver confirm!~.O
        await channel.send("< WBC2026 will begin 20260305 ~20260318 >")
        print("✅ 訊息已送出")
    except Exception as e:
        print("❌ 發送失敗:", e)

    await client.close()  # 傳送完自動關閉

client.run(MACHINE_STR)