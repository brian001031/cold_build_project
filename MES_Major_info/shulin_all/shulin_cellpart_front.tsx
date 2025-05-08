import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { shulin_cellpart_front_box } from "../../data";
import config from "../../styles/config";
import axios from "axios";
import "./shulin_cellpart_front.scss";

//---------------前段cell-cell-電芯-陽陰極處理序(1)----------------------------
function shulin_cellpart_front() {
  const apiMesUrl = import.meta.env.VITE_MES_API_URL;
  const [mesdata, setmesdata] = useState([]);
  const [currentdata, setcurrentData] = useState(""); // 存放字串數據
  const [previousData, setPreviousData] = useState(""); // 儲存上次獲取的字串
  const [isUpdated, setIsUpdated] = useState(false); // 用來標示字串是否變更
  const [progress, setProgress] = useState(0); // 進度條的進度

  const keymatch = [
    "(最新工作序號)",
    "(設備數量[線上/總])",
    "(生產人員[線上/總])",
    "(生產工單)",
    "(本日產能)",
  ];

  let mesmap_part1: Map<unknown, unknown>;
  const [CellMap_front, setCellMap_front] = useState<[unknown, unknown][]>([]);

  // 用來儲存字串的陣列，每個字串會有一個 "updated" 標記，來表示是否更新
  const [data, setData] = useState<{ text: string; updated: boolean }[]>([]);
  const [dataMap, setDataMap] = useState<{
    [key in (typeof keymatch)[number]]: string;
  }>({});

  useEffect(() => {
    // 使用 fetch 請求後端 API

    const timer = setInterval(() => {
      const fetchData_WO = async () => {
        try {
          const response = await axios.get(
            // `${apiMesUrl}/mes/cellpart_front`
            // `${config.apiBaseUrl}/mes/cellpart_front`
            "http://localhost:3009/mes/cellpart_front"
          );

          const woresult = await response.data;
          const values = woresult.split(",");

          // console.log(values);

          //判斷本日產能是否有更新
          //console.log(values[3]);

          //將ID存入後續判斷是否有變更
          //setData(values);

          setcurrentData(values[0]);

          mesmap_part1 = new Map(
            values.map((item: { name: any }, index: any) => [
              keymatch[index],
              item,
            ]) // 使用keymatch陣列的索引作為 key，item 作為 value
          );

          // console.log(mesmap_part1);

          // 將 Map 轉換為陣列
          const arrayFromMap: [unknown, unknown][] = Array.from(mesmap_part1);

          // 將轉換後的陣列設置到 state 中
          setCellMap_front(arrayFromMap);

          // const map =
          //   // 使用 reduce() 將值與 key 結合，並消除隱式的 any 類型
          //   values.reduce(
          //     (acc: { [x: string]: any }, value: any, index: string | number) => {
          //       acc[key[index]] = value;
          //       return acc;
          //     },
          //     {} as { [key in (typeof keymatch)[number]]: string }
          //   ); // 明確指定類型

          // setDataMap(map);
        } catch (error) {
          console.error("Error fetching WO data:", error);
        }
      };

      fetchData_WO();
    }, 3000); // 每3秒鐘执行一次(1000毫秒X3)
    return () => clearInterval(timer); // 清除計時器
  }, []);

  useEffect(() => {
    //currentdata 數據value偵測是否有變化
    if (currentdata !== previousData) {
      setPreviousData(currentdata); // 更新已儲存的字串
      setIsUpdated(true); // 標記數據更新
      setProgress(0); // 重置進度條
    }
  }, [currentdata, previousData]); // 當 currentdata 改變時觸發

  // 進度條更新效果
  useEffect(() => {
    if (isUpdated) {
      const timer = setInterval(() => {
        setProgress((prev) => {
          if (prev >= 100) {
            clearInterval(timer);
            setIsUpdated(false); // 更新完成後停止動畫
            return 100;
          }
          return prev + 20;
        });
      }, 500);
      return () => clearInterval(timer);
    }
  }, [isUpdated]);

  return (
    <div className="shulin_cellpart_front">
      {shulin_cellpart_front_box.map((item, index) => (
        <div className={`box box${item.id}`}>
          <div className="image-container">
            <img className="image" src={item.icon} alt=" "></img>
          </div>
          <span className="title">{item.title}</span>
          {/* <span className="productvalue">{item.equipment_qty}</span>
          <span className="productvalue">{item.op_num}</span>
          <span className="productvalue">{item.workorder}</span> */}
          <span className="productvalue">{item.currentday_capacity}</span>
          <ul>
            {CellMap_front.map(([key, value], index) => (
              <span
                key={index}
                style={{
                  fontSize: "20",
                  color: "#FFACB3",
                  display: "block",
                }}
              >
                {key as React.ReactNode}
                <span
                  style={{
                    fontSize: "20",
                    display: "block",
                    fontWeight: "bold",
                    color: isUpdated ? "#FF5809" : "#FFFF37",
                    transition: "color 0.3s ease-in-out",
                  }}
                >
                  {value as React.ReactNode}
                </span>
              </span>
            ))}
          </ul>
          {/* {Object.entries(mesmap_part1).map((value, index) => (
            <span
              style={{
                fontSize: "20",
                color: "#75",
                display: "block",
              }}
            >
              {value[0]}
              <span
                style={{
                  fontSize: "20",
                  color: "#FFD306",
                  display: "block",
                }}
              >
                {""}
                {value[1]}
              </span>
            </span>
          ))}
          ; */}
        </div>
      ))}
      {/* {Object.entries(dataMap).map((value, index) => (
        <span style={{ fontSize: "25px", color: "#750075" }}>
          {value[0]}
          {"-"}
          {value[1]}
        </span>
      ))} */}
      {/* <pre>{JSON.stringify(dataMap, null, 2)}</pre>; */}

      <div
        style={{
          //marginBottom: "20px",
          width: "100%",
          backgroundColor: "#f0Ff0",
          borderRadius: "10x",
        }}
      >
        <div
          style={{
            height: "20px",
            width: `${progress}%`,
            backgroundColor: progress === 100 ? "green" : "blue",
            borderRadius: "10px",
            transition: "width 0.5s ease-in-out",
          }}
        >
          {isUpdated && (
            <span
              style={{
                fontSize: "110px",
                color: "#EA0000",
                display: "inline-block",
                textAlign: "end",
                position: "relative",
                paddingLeft: "100px",
                scrollPaddingRight: "20%",
                paddingBottom: "50px",
              }}
            >
              更新中
            </span>
          )}
        </div>
      </div>
      <span className="span_shulintext">樹 林 廠</span>
    </div>
  );
}

export default shulin_cellpart_front;
