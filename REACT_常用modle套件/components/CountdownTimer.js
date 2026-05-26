import React, { useState, useEffect } from "react";

// 格式化秒数为 mm:ss
const formatTime = (seconds) => {
  const minutes = Math.floor(seconds / 60);
  const remainingSeconds = seconds % 60;
  return `${String(minutes).padStart(2, "0")}:${String(
    remainingSeconds
  ).padStart(2, "0")}`;
};

// 倒计时器组件
const CountdownTimer = ({ isActive, resetTimer }) => {
  const [seconds, setSeconds] = useState(0);

  useEffect(() => {
    if (isActive) {
      setSeconds(3); // 设置倒计时的初始秒数（例如 3 秒）
    }
  }, [isActive]);

  useEffect(() => {
    let interval = null;

    if (isActive && seconds > 0) {
      interval = setInterval(() => {
        setSeconds((prevSeconds) => prevSeconds - 1);
      }, 1000);
    } else if (seconds === 0) {
      // isActive = !isActive;
      //isActive =false;
      clearInterval(interval);
    }

    return () => clearInterval(interval);
  }, [isActive, seconds]);

  useEffect(() => {
    if (resetTimer) {
      setSeconds(3); // 点击按钮后重新开始倒计时
    }
  }, [resetTimer]);

  const headingStyle = {
    color: "green",
    fontSize: "20px",
    fontWeight: "bold",
    right: "100px",
    width: "150px",
    padding: "10px",
  };

  return (
    <div>
      {isActive && seconds > 0 && (
        <div align="right">
          <h1 style={headingStyle}>
            {"等待"}
            {formatTime(seconds)}
            {"秒"}
          </h1>
        </div>
      )}
      {seconds === 0 && (
        <div align="right">
          <h2 style={headingStyle}>繼續確認</h2>
        </div>
      )}
    </div>
  );
};

export default CountdownTimer;
