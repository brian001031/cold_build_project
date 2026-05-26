import { useEffect, useState } from "react";
import axios from "axios";
import config from "../config";

export default function AuthLogin() {
  const [authUrl, setAuthUrl] = useState("");

  useEffect(() => {
    const fetchAuthUrl = async () => {
      try {
        const response = await axios.get(
          `${config.apiBaseUrl}/electricinspec/authurl`
          // "http://localhost:3009/electricinspec/authurl"
        );

        console.log("取得 ngrok URL 成功", response.data.url);

        setAuthUrl(response.data.url); // 更新 AuthUrl 狀態
      } catch (error) {
        console.error("取得重新授權URL錯誤", error);
      }
    };
    fetchAuthUrl();
  }, []);

  return (
    <div>
      <h1>登入-GOOLE-授權頁面</h1>
      {authUrl ? (
        <a href={authUrl}>
          <button>使用 Google 登入</button>
        </a>
      ) : (
        <p>載入中...</p>
      )}
    </div>
  );
}
