import React from "react";
import config from "../config";

const Photo_FlexTemplate = ({ photoPaths }) => {
  const styles = {
    container: {
      display: "flex",
      flexWrap: "wrap", // 使圖片自動換行
      gap: "10px", // 設置圖片之間的間距
      objectFit: "contain",
    },
    image: {
      width: "calc(26.33% - 10px)", // 使圖片佔據 3 列，每列 26.33% 寬度
      height: "190px", // 高度手動設置
      display: "block", // 確保圖片顯示為塊級元素
    },
  };

  return (
    <div style={styles.container}>
      {photoPaths.map((photo, index) => {
        const imageExtensions = [
          "jpg",
          "jpeg",
          "png",
          "gif",
          "webp",
          "bmp",
          "svg",
        ];
        const extension = photo.split(".").pop().toLowerCase();

        if (!imageExtensions.includes(extension)) return null;

        //圖像格式時才渲染 <img> 標籤
        return (
          <img
            key={index}
            className="d-block w-10"
            src={`${config.apiBaseUrl}/uploads/${photo}`}
            // src={`http://localhost:3009/uploads/${photo}`}
            alt={`photoPaths ${index + 1}`}
            style={styles.image}
          />
        );
      })}
    </div>
  );
};

export default Photo_FlexTemplate;
