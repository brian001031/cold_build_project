import { useEffect, useState } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import axios from "axios";
import config from "../config"; // Assuming you have a config file for API base URL
//  <Route path="/electrical_inspection_dlg" element={<ElectricalInspection/>}/>

export default function AuthCallback() {
  const [apiBase, setApiBase] = useState(null);
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  useEffect(() => {
    const getAPIURL = async () => {
      await axios
        .get("http://localhost:3009/electricinspec/getapibaseURL")
        .then((res) => {
          console.log("取得getAPIURL  ngrok URL 成功", res.data.redUri);
          setApiBase(res.data.redUri);
        });
    };

    getAPIURL();
  }, []);

  useEffect(() => {
    if (!apiBase) return;
    const getToken = async () => {
      const params = new URLSearchParams(window.location.search);
      const code = params.get("code");

      console.log("從 getToken URL 取得的 code:", code);
      console.log("要送出ngrok 的 apibaseurl:", apiBase);

      if (code) {
        try {
          const response = await axios.post(
            //"http://localhost:3009/oauth2callback",
            `${apiBase}/oauth2callback`,
            //`${config.apiBaseUrl}/oauth2callback`,
            { code, apiBase },
            {
              headers: {
                "Content-Type": "application/json",
              },
            }
          );

          console.log("token 反映總Data = ", response.data);
          console.log(JSON.stringify(response.data, null, 2));

          //方法1:
          // const { access_token } = response.data;

          //方法2:
          const access_token = response.data.access_token;
          console.log("交換 token 成功", access_token);

          // 儲存 token（localStorage 或 context）
          localStorage.setItem("accessToken", access_token);

          // 導向 inspection 頁面
          navigate("/electrical_inspection_dlg");
        } catch (error) {
          console.error("交換 token 失敗", error);
        }
      } else {
        console.warn("沒有 code");
      }
    };

    getToken();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [apiBase, navigate]);

  return <div>授權中，請稍候...</div>;
}
