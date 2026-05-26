import React from "react";
import axios from "axios";
import config from "../../config";
import dayjs from "dayjs";

const DownloadButton = () => {
  const handleDownload = async () => {
    try {
      const response = await axios({
        url: `${config.apiBaseUrl}/bento/download`, // 你的API路由
        method: "GET",
        responseType: "blob", // 重要
      });

      // 創建一個隱藏的可下載鏈接
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      // 獲取今天的日期
      const today = dayjs().format("YYYYMMDD");
      link.setAttribute("download", `${today}便當`); // 或者其他你想要的檔案名稱
      document.body.appendChild(link);
      link.click();
    } catch (error) {
      console.error("Error:", error);
    }
  };

  return <button onClick={handleDownload}>下載excel</button>;
};

export default DownloadButton;
