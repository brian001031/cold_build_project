import os
import socket
import time
import json
import requests
import subprocess
from threading import Thread
from flask import Flask, request, redirect, url_for, session, render_template, flash
from authlib.integrations.flask_client import OAuth
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from googleapiclient.http import MediaFileUpload
from google.oauth2.credentials import Credentials
from werkzeug.utils import secure_filename


# ========== Config ==========


#宣告ngrok_process 全域變數
ngrok_process = None

# 請填入你下載的 OAuth 2.0 client 的 JSON 憑證檔案
CLIENT_SECRETS_FILE = 'client_secret_web.json'
SCOPES = ['https://www.googleapis.com/auth/drive.file']  # 限定上傳檔案權限
# ---- 指定上傳到的 Folder ID ----
FOLDER_ID = '18TopGqiMiGqznraLrbr2t5sW93VLjFbi'  # 你的 Drive folder ID
TARGET_FOLDER_ID = FOLDER_ID  # 方便用名

#允許上傳的檔案類型
ALLOWED_EXTENSIONS = {'txt', 'json', 'png', 'jpeg' ,'jpg', 'bmp','pdf','py','js','gif','sql','scss','tsx', 'png','csv','sqlite'}

SESSION_TOKEN_KEY = 'google_token'  # session裡存token的key

# Flask 應用
app = Flask(__name__)
#app.secret_key = os.environ.get('FLASK_SECRET_KEY', 'replace_me_with_random_string')
#app.secret_key = os.environ.get(os.urandom(24).hex(), 'replace_me_with_random_string')
app.secret_key = os.environ.get('FLASK_SECRET_KEY', os.urandom(24))

# 初始化 OAuth
oauth = OAuth(app)
# 用 authlib 註冊 Google OAuth 客戶端
oauth.register(
    name='google',
    client_id=None,  # 稍後從JSON載入
    client_secret=None,
    server_metadata_url='https://accounts.google.com/.well-known/openid-configuration',
    client_kwargs={
        'scope': ' '.join(SCOPES),
        'access_type': 'offline',
        'prompt': 'consent'
    }
)


# ========== 輔助函式 ==========


# ========== 輔助函式 ==========

def load_client_config():
    """從 JSON 憑證載入 client_id / client_secret 至 oauth.config"""
    with open(CLIENT_SECRETS_FILE, 'r') as f:
      obj = json.load(f)
    web = obj.get('web')
    oauth.google.client_id = web.get('client_id')
    oauth.google.client_secret = web.get('client_secret')
    # redirect_uris 應包含你的 ngrok callback 路徑
    # 這邊不自動設定 redirect，authlib 會自動用 route 的 url_for

def allowed_file(filename):
    ext = filename.rsplit('.', 1)[-1].lower()
    return '.' in filename and ext in ALLOWED_EXTENSIONS

def get_client_ip():
    # 如果背後有代理，可能要調整這邊
    if request.headers.get('X-Forwarded-For'):
        return request.headers.get('X-Forwarded-For').split(',')[0]
    return request.remote_addr


# Web OAuth token 操作 Drive API
def get_drive_service_from_Oauth2(token):
    creds = Credentials(
        token=token['access_token'],
        refresh_token=token.get('refresh_token'),
        token_uri='https://oauth2.googleapis.com/token',
        client_id=oauth.google.client_id,
        client_secret=oauth.google.client_secret
    )
    service = build('drive', 'v3', credentials=creds)
    return service

# ---- 建立 Drive 服務物件  用 InstalledAppFlow 產生本地授權流程----
def get_drive_service(token):
    """根據 oauth token 建立 Drive service"""
    creds = None

    # 如果已授權過，直接讀取 token
    if os.path.exists('token.pickle'):
        with open('token.pickle', 'rb') as token:
            creds = pickle.load(token)

    # 否則進行授權流程
    if not creds or not creds.valid:
        if creds and creds.expired and creds.refresh_token:
            creds.refresh(Request())
        else:
            flow = InstalledAppFlow.from_client_secrets_file(CLIENT_SECRETS_FILE, SCOPES)
            creds = flow.run_local_server(port=0)  # ✅ 使用者瀏覽器登入

        # 存下 token，避免每次都要登入
        with open('token.pickle', 'wb') as token:
            pickle.dump(creds, token)

    service = build('drive', 'v3', credentials=creds)
    return service

# ========== Web 路由 ==========



@app.route('/')
def index():
    token = session.get(SESSION_TOKEN_KEY)
    return render_template('index.html', token=token,request=request)
	
	
@app.route('/login')
def login():
    load_client_config()
    #強制 redirect URI 使用 https 才能和Google Console 登錄的 URI 是 HTTPS匹配
    redirect_uri = url_for('oauth2callback', _external=True, _scheme='https')
    print("Redirect URI:", redirect_uri)  # 印出來檢查
    return oauth.google.authorize_redirect(redirect_uri)

@app.route('/oauth2callback')
def oauth2callback():
    token = oauth.google.authorize_access_token()
    session[SESSION_TOKEN_KEY] = token
    flash('授權成功！現在你可以上傳檔案。')
    return redirect(url_for('upload_page'))

@app.route('/upload', methods=['GET', 'POST'])
def upload_page():
    token = session.get(SESSION_TOKEN_KEY)
    if token is None:
        return redirect(url_for('login'))

    if request.method == 'POST':
        if 'file' not in request.files:
            flash('沒有上傳檔案欄位')
            return redirect(request.url)
        file = request.files['file']
        if file.filename == '':
            flash('請選擇檔案')
            return redirect(request.url)
        if file and allowed_file(file.filename):
            filename = secure_filename(file.filename)
            local_path = os.path.join('/tmp', filename)
            file.save(local_path)

            meta = {
                'timestamp': time.strftime('%Y-%m-%d %H:%M:%S'),
                'client_ip': get_client_ip(),
                'filename': filename,
            }

            try:
                drive_service = get_drive_service_from_Oauth2(token)                               
                file_metadata = {
                    'name': filename,
                    'parents': [TARGET_FOLDER_ID]
                }
                media = MediaFileUpload(local_path, resumable=False)
                uploaded = drive_service.files().create(
                    body=file_metadata,
                    media_body=media,
                    fields='id, name'
                ).execute()
                print('上傳結果：', uploaded)

                flash('檔案上傳成功！ID: ' + uploaded.get('id'))
                return redirect(request.url)
            except Exception as e:
                flash('上傳失敗：' + str(e))
                return redirect(request.url)
        else:
            flash('不支援該檔案類型')
            return redirect(request.url)

    # GET 顯示表單
    return render_template('upload.html')
  
  
@app.route('/logout')
def logout():
    session.pop(SESSION_TOKEN_KEY, None)
    flash('已登出')
    return redirect(url_for('index'))
	
	
# === 找一個可用的本機 TCP port ===
def find_free_port():
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind(('', 0))
        return s.getsockname()[1]
	

# 啟動 ngrok reserved domain
def start_ngrok(port,domain):
    # 已安裝 ngrok 並有 authtoken ,啟動 ngrok http
	#subprocess.Popen(['ngrok', 'http', str(port)], stdout=subprocess.DEVNULL)
    #os.system(f'ngrok http {port} > /dev/null &')
    # 等待一點時間讓 ngrok 起來
    #time.sleep(2) # 等待 ngrok 起來
    
    global ngrok_process
    
    print(f"[ngrok] 啟動 ngrok (domain={domain}, port={port})...")    
    ngrok_process = subprocess.Popen([
        "ngrok",
        "http",
        f"--domain={domain}",
        str(port)
    ])
     
    time.sleep(3)  # 給 ngrok 一點時間建立 tunnel

# =====  ngrok public URL 取得公開網址 (確認成功) =====
def get_ngrok_url():
    try:
        response = requests.get('http://localhost:4040/api/tunnels')
       # tunnels = resp.json().get("tunnels", [])
       # for t in tunnels:
       #   if t.get("proto") == "https":
       #     return t.get("public_url")
                
        tunnels = response.json()['tunnels']
        return tunnels[0]['public_url'] if tunnels else None
    except Exception as e:
        print(f"[!] 無法取得 ngrok 公開網址: {e}")
        return None

# === 啟動 Flask 應用 ===
def run_app(port):
    app.run(host='0.0.0.0', port=port)
	
	
# === 主流程 ===
if __name__ == '__main__':
    port = find_free_port()
    
    # 你的 reserved domain（從 ngrok 控制台複製）
    RESERVED_DOMAIN = "untrapped-crenelated-cherryl.ngrok-free.dev"

    # 背景啟動 ngrok
    #Thread(target=lambda: start_ngrok(port)).start()
    Thread(target=start_ngrok, args=(port, RESERVED_DOMAIN)).start()

    # 顯示 ngrok 公網網址
    time.sleep(5)  # 等 ngrok 起來
       
    ngrok_url = get_ngrok_url()
     
    if ngrok_url:
        print(f"[✓] 公開網址: {ngrok_url}")
        print(f"[!] 請確保你已將此網址 + /oauth2callback 加入 Google Cloud OAuth 回調 URI")
    else:
        print("[X] 未能取得 ngrok 公網網址")
        exit(1)
      
    try:
        # 啟動 Flask 應用
        run_app(port)

    except KeyboardInterrupt:
        print("Stopping...")

    finally:
        if ngrok_process:
            print("Closing ngrok...")
            ngrok_process.terminate()
            ngrok_process.wait()
            print("ngrok closed.")
    
        
        
     
	
